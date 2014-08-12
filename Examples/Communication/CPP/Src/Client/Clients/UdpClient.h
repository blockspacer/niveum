﻿#pragma once

#include "IContext.h"
#include "ISerializationClient.h"
#include "StreamedClient.h"

#include "BaseSystem/LockedVariable.h"
#include "BaseSystem/Cryptography.h"

#include <memory>
#include <cstdint>
#include <climits>
#include <cmath>
#include <cstring>
#include <vector>
#include <set>
#include <map>
#include <string>
#include <exception>
#include <stdexcept>
#include <functional>
#include <boost/asio.hpp>
#include <boost/date_time.hpp>
#ifdef _MSC_VER
#undef SendMessage
#endif

namespace Client
{
    namespace Streamed
    {
        class UdpClient
        {
        private:
            std::shared_ptr<IStreamedVirtualTransportClient> VirtualTransportClient;
            boost::asio::ip::udp::endpoint RemoteEndPoint;
            BaseSystem::LockedVariable<bool> IsRunningValue;
        public:
            bool IsRunning()
            {
                return IsRunningValue.Check<bool>([](bool v) { return v; });
            }
        private:
            BaseSystem::LockedVariable<int> SessionIdValue;
        public:
            int SessionId()
            {
                return SessionIdValue.Check<int>([](int v) { return v; });
            }
        private:
            BaseSystem::LockedVariable<bool> ConnectedValue;
            BaseSystem::LockedVariable<std::shared_ptr<class SecureContext>> SecureContextValue;
        public:
            std::shared_ptr<class SecureContext> SecureContext()
            {
                return SecureContextValue.Check<std::shared_ptr<class SecureContext>>([](std::shared_ptr<class SecureContext> v) { return v; });
            }
            void SetSecureContext(std::shared_ptr<class SecureContext> sc)
            {
                SecureContextValue.Update([=](std::shared_ptr<class SecureContext> v) { return sc; });
            }
        private:
            boost::asio::io_service &io_service;
            boost::asio::ip::udp::socket Socket;
            std::vector<std::uint8_t> ReadBuffer;

        public:
            static const int MaxPacketLength = 1400;
            static const int ReadingWindowSize = 1024;
            static const int WritingWindowSize = 16;
            static const int IndexSpace = 65536;
            static const int InitialPacketTimeoutMilliseconds = 500;
            static const int MaxSquaredPacketResentCount = 3;
            static const int MaxLinearPacketResentCount = 10;
        private:
            static int GetTimeoutMilliseconds(int ResentCount)
            {
                if (ResentCount <= MaxSquaredPacketResentCount) { return InitialPacketTimeoutMilliseconds * (1 << ResentCount); }
                return InitialPacketTimeoutMilliseconds * (1 << MaxSquaredPacketResentCount) * (std::min(ResentCount, MaxLinearPacketResentCount) - MaxSquaredPacketResentCount + 1);
            }

            class Part
            {
            public:
                int Index;
                std::shared_ptr<std::vector<std::uint8_t>> Data;
                boost::posix_time::ptime ResendTime;
                int ResentCount;
            };
            class PartContext
            {
            private:
                int WindowSize;
            public:
                PartContext(int WindowSize)
                    : MaxHandled(IndexSpace - 1)
                {
                    this->WindowSize = WindowSize;
                }

                int MaxHandled;
                std::map<int, std::shared_ptr<Part>> Parts;
                std::shared_ptr<Part> TryTakeFirstPart()
                {
                    if (Parts.size() == 0) { return nullptr; }
                    auto First = *Parts.begin();
                    auto Key = std::get<0>(First);
                    auto Value = std::get<1>(First);
                    if (IsSuccessor(Key, MaxHandled))
                    {
                        Parts.erase(Key);
                        MaxHandled = Key;
                        return Value;
                    }
                    return nullptr;
                }
                bool IsEqualOrAfter(int New, int Original)
                {
                    return ((New - Original + IndexSpace) % IndexSpace) < WindowSize;
                }
                static bool IsSuccessor(int New, int Original)
                {
                    return ((New - Original + IndexSpace) % IndexSpace) == 1;
                }
                static int GetSuccessor(int Original)
                {
                    return (Original + 1) % IndexSpace;
                }
                bool HasPart(int Index)
                {
                    if (IsEqualOrAfter(MaxHandled, Index))
                    {
                        return true;
                    }
                    if (Parts.count(Index) > 0)
                    {
                        return true;
                    }
                    return false;
                }
                bool TryPushPart(int Index, std::shared_ptr<std::vector<std::uint8_t>> Data, int Offset, int Length)
                {
                    if (((Index - MaxHandled + IndexSpace) % IndexSpace) > WindowSize)
                    {
                        return false;
                    }
                    auto b = std::make_shared<std::vector<std::uint8_t>>();
                    b->resize(Length, 0);
                    memcpy(&(*b)[0], &(*Data)[Offset], Length);
                    auto p = std::make_shared<Part>();
                    p->Index = Index;
                    p->Data = b;
                    p->ResendTime = boost::posix_time::microsec_clock::universal_time() + boost::posix_time::milliseconds(GetTimeoutMilliseconds(0));
                    p->ResentCount = 0;
                    Parts[Index] = p;
                    return true;
                }
                bool TryPushPart(int Index, std::shared_ptr<std::vector<std::uint8_t>> Data)
                {
                    if (((Index - MaxHandled + IndexSpace) % IndexSpace) > WindowSize)
                    {
                        return false;
                    }
                    auto p = std::make_shared<Part>();
                    p->Index = Index;
                    p->Data = Data;
                    p->ResendTime = boost::posix_time::microsec_clock::universal_time() + boost::posix_time::milliseconds(GetTimeoutMilliseconds(0));
                    p->ResentCount = 0;
                    Parts[Index] = p;
                    return true;
                }

                void Acknowledge(int Index, const std::vector<int> &Indices)
                {
                    MaxHandled = Index;
                    while (true)
                    {
                        if (Parts.size() == 0) { return; }
                        auto First = *Parts.begin();
                        auto Key = std::get<0>(First);
                        auto Value = std::get<1>(First);
                        if (Key <= Index)
                        {
                            Parts.erase(Key);
                        }
                        if (Key >= Index)
                        {
                            break;
                        }
                    }
                    for (auto i : Indices)
                    {
                        if (Parts.count(i) > 0)
                        {
                            Parts.erase(i);
                        }
                    }
                }

                void ForEachTimedoutPacket(boost::posix_time::ptime Time, std::function<void(int, std::shared_ptr<std::vector<std::uint8_t>>)> f)
                {
                    for (auto p : Parts)
                    {
                        auto Key = std::get<0>(p);
                        auto Value = std::get<1>(p);
                        if (Value->ResendTime <= Time)
                        {
                            f(Key, Value->Data);
                            Value->ResendTime = boost::posix_time::microsec_clock::universal_time() + boost::posix_time::milliseconds(GetTimeoutMilliseconds(Value->ResentCount));
                            Value->ResentCount += 1;
                        }
                    }
                }
            };
            class UdpReadContext
            {
            public:
                std::shared_ptr<PartContext> Parts;
                std::set<int> NotAcknowledgedIndices;
            };
            class UdpWriteContext
            {
            public:
                std::shared_ptr<PartContext> Parts;
                int WritenIndex;
                std::shared_ptr<boost::asio::deadline_timer> Timer;
            };
            BaseSystem::LockedVariable<std::shared_ptr<UdpReadContext>> RawReadingContext;
            BaseSystem::LockedVariable<std::shared_ptr<UdpWriteContext>> CookedWritingContext;

        public:
            UdpClient(boost::asio::io_service &io_service, boost::asio::ip::udp::endpoint RemoteEndPoint, std::shared_ptr<IStreamedVirtualTransportClient> VirtualTransportClient)
                : io_service(io_service), Socket(io_service), IsRunningValue(false), SessionIdValue(0), ConnectedValue(false), SecureContextValue(nullptr), RawReadingContext(nullptr), CookedWritingContext(nullptr), IsDisposed(false)
            {
                ReadBuffer.resize(MaxPacketLength, 0);
                RawReadingContext.Update([](std::shared_ptr<UdpReadContext> cc)
                {
                    auto c = std::make_shared<UdpReadContext>();
                    c->Parts = std::make_shared<PartContext>(ReadingWindowSize);
                    return c;
                });
                CookedWritingContext.Update([](std::shared_ptr<UdpWriteContext> cc)
                {
                    auto c = std::make_shared<UdpWriteContext>();
                    c->Parts = std::make_shared<PartContext>(WritingWindowSize);
                    c->WritenIndex = IndexSpace - 1;
                    c->Timer = nullptr;
                    return c;
                });

                this->RemoteEndPoint = RemoteEndPoint;
                this->VirtualTransportClient = VirtualTransportClient;

                //在Windows下关闭SIO_UDP_CONNRESET报告，防止接受数据出错
                //http://support.microsoft.com/kb/263823/en-us
#if _MSC_VER
                //TODO
                //Socket.io_control(SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
#endif

                VirtualTransportClient->ClientMethod = [=]()
                {
                    OnWrite(*VirtualTransportClient, []() {}, []() { throw std::logic_error("InvalidOperationException"); });
                };
            }

            virtual ~UdpClient()
            {
                Close();
            }

        private:
            void OnWrite(const IStreamedVirtualTransportClient &vtc, std::function<void()> OnSuccess, std::function<void()> OnFailure)
            {
                auto ByteArrays = VirtualTransportClient->TakeWriteBuffer();
                int TotalLength = 0;
                for (auto b : ByteArrays)
                {
                    TotalLength += static_cast<int>(b->size());
                }
                auto WriteBuffer = std::make_shared<std::vector<std::uint8_t>>();
                WriteBuffer->resize(GetMinNotLessPowerOfTwo(TotalLength), 0);
                int Offset = 0;
                for (auto b : ByteArrays)
                {
                    memcpy(&(*WriteBuffer)[Offset], &(*b)[0], b->size());
                }
                auto RemoteEndPoint = this->RemoteEndPoint;
                auto SessionId = this->SessionId();
                auto Connected = this->ConnectedValue.Check<bool>([](bool v) { return v; });
                auto SecureContext = this->SecureContextValue.Check<std::shared_ptr<class SecureContext>>([](std::shared_ptr<class SecureContext> v) { return v; });
                std::vector<int> Indices;
                RawReadingContext.DoAction([&](std::shared_ptr<UdpReadContext> c)
                {
                    if (c->NotAcknowledgedIndices.size() == 0) { return; }
                    while (c->NotAcknowledgedIndices.size() > 0)
                    {
                        auto First = *c->NotAcknowledgedIndices.begin();
                        if (c->Parts->IsEqualOrAfter(c->Parts->MaxHandled, First))
                        {
                            c->NotAcknowledgedIndices.erase(First);
                        }
                        else
                        {
                            break;
                        }
                    }
                    Indices.push_back(c->Parts->MaxHandled);
                    for (auto i : c->NotAcknowledgedIndices)
                    {
                        Indices.push_back(i);
                    }
                    c->NotAcknowledgedIndices.clear();
                });
                if ((ByteArrays.size() == 0) && (Indices.size() == 0))
                {
                    OnSuccess();
                    return;
                }
                bool Success = true;
                std::vector<std::shared_ptr<std::vector<std::uint8_t>>> Parts;
                CookedWritingContext.DoAction([&](std::shared_ptr<UdpWriteContext> c)
                {
                    auto Time = boost::posix_time::microsec_clock::universal_time();
                    auto WritingOffset = 0;
                    while (WritingOffset < TotalLength)
                    {
                        auto Index = PartContext::GetSuccessor(c->WritenIndex);

                        auto NumIndex = static_cast<int>(Indices.size());
                        if (NumIndex > 0xFFFF)
                        {
                            Success = false;
                            return;
                        }

                        auto IsACK = NumIndex > 0;
                        auto Flag = 0;
                        if (!Connected)
                        {
                            Flag |= 4; //INI
                            IsACK = false;
                        }

                        auto Length = std::min(12 + (IsACK ? 2 + NumIndex * 2 : 0) + TotalLength - WritingOffset, MaxPacketLength);
                        auto DataLength = Length - (12 + (IsACK ? 2 + NumIndex * 2 : 0));
                        auto Buffer = std::make_shared<std::vector<std::uint8_t>>();
                        Buffer->resize(Length, 0);
                        (*Buffer)[0] = static_cast<std::uint8_t>(SessionId & 0xFF);
                        (*Buffer)[1] = static_cast<std::uint8_t>((SessionId >> 8) & 0xFF);
                        (*Buffer)[2] = static_cast<std::uint8_t>((SessionId >> 16) & 0xFF);
                        (*Buffer)[3] = static_cast<std::uint8_t>((SessionId >> 24) & 0xFF);

                        if (IsACK)
                        {
                            Flag |= 1; //ACK
                            (*Buffer)[12] = static_cast<std::uint8_t>(NumIndex & 0xFF);
                            (*Buffer)[13] = static_cast<std::uint8_t>((NumIndex >> 8) & 0xFF);
                            int j = 0;
                            for (auto i : Indices)
                            {
                                (*Buffer)[14 + j * 2] = static_cast<std::uint8_t>(i & 0xFF);
                                (*Buffer)[14 + j * 2 + 1] = static_cast<std::uint8_t>((i >> 8) & 0xFF);
                                j += 1;
                            }
                            Indices.clear();
                        }

                        memcpy(&(*Buffer)[12 + (IsACK ? 2 + NumIndex * 2 : 0)], &(*WriteBuffer)[WritingOffset], DataLength);
                        WritingOffset += DataLength;

                        if (SecureContext != nullptr)
                        {
                            Flag |= 2; //ENC
                        }
                        (*Buffer)[4] = static_cast<std::uint8_t>(Flag & 0xFF);
                        (*Buffer)[5] = static_cast<std::uint8_t>((Flag >> 8) & 0xFF);
                        (*Buffer)[6] = static_cast<std::uint8_t>(Index & 0xFF);
                        (*Buffer)[7] = static_cast<std::uint8_t>((Index >> 8) & 0xFF);

                        std::int32_t Verification = 0;
                        if (SecureContext != nullptr)
                        {
                            std::vector<std::uint8_t> SHABuffer;
                            SHABuffer.resize(4);
                            memcpy(&SHABuffer[0], &(*Buffer)[4], 4);
                            auto SHA1 = Algorithms::Cryptography::SHA1(SHABuffer);
                            std::vector<std::uint8_t> Key;
                            Key.resize(SecureContext->ServerToken.size() + SHA1.size());
                            memcpy(&Key[0], &SecureContext->ServerToken[0], SecureContext->ServerToken.size());
                            memcpy(&Key[SecureContext->ServerToken.size()], &SHA1[0], SHA1.size());
                            auto HMACBytes = Algorithms::Cryptography::HMACSHA1(Key, *Buffer);
                            HMACBytes.resize(4);
                            Verification = HMACBytes[0] | (static_cast<std::int32_t>(HMACBytes[1]) << 8) | (static_cast<std::int32_t>(HMACBytes[2]) << 16) | (static_cast<std::int32_t>(HMACBytes[3]) << 24);
                        }
                        else
                        {
                            Verification = Algorithms::Cryptography::CRC32(*Buffer);
                        }

                        (*Buffer)[8] = static_cast<std::uint8_t>(Verification & 0xFF);
                        (*Buffer)[9] = static_cast<std::uint8_t>((Verification >> 8) & 0xFF);
                        (*Buffer)[10] = static_cast<std::uint8_t>((Verification >> 16) & 0xFF);
                        (*Buffer)[11] = static_cast<std::uint8_t>((Verification >> 24) & 0xFF);

                        auto Part = std::make_shared<class Part>();
                        Part->Index = Index;
                        Part->ResendTime = Time + boost::posix_time::milliseconds(GetTimeoutMilliseconds(0));
                        Part->Data = Buffer;
                        Part->ResentCount = 0;
                        if (!c->Parts->TryPushPart(Index, Buffer))
                        {
                            Success = false;
                            return;
                        }
                        Parts.push_back(Part->Data);

                        c->WritenIndex = Index;
                    }
                    if (c->Timer == nullptr)
                    {
                        c->Timer = std::make_shared<boost::asio::deadline_timer>(io_service);
                        c->Timer->expires_from_now(boost::posix_time::milliseconds(GetTimeoutMilliseconds(0)));
                        c->Timer->async_wait([=](const boost::system::error_code& error)
                        {
                            if (error == boost::system::errc::success)
                            {
                                Check();
                            }
                        });
                    }
                });
                for (auto p : Parts)
                {
                    try
                    {
                        SendPacket(RemoteEndPoint, p);
                    }
                    catch (...)
                    {
                        Success = false;
                        break;
                    }
                }
                if (!Success)
                {
                    OnFailure();
                }
                else
                {
                    OnSuccess();
                }
            }

            void Check()
            {
                auto IsRunning = this->IsRunning();

                auto RemoteEndPoint = this->RemoteEndPoint;
                auto SessionId = this->SessionId();
                auto Connected = this->ConnectedValue.Check<bool>([](bool v) { return v; });
                auto SecureContext = this->SecureContextValue.Check<std::shared_ptr<class SecureContext>>([](std::shared_ptr<class SecureContext> v) { return v; });
                std::vector<int> Indices;
                RawReadingContext.DoAction([&](std::shared_ptr<UdpReadContext> c)
                {
                    if (c->NotAcknowledgedIndices.size() == 0) { return; }
                    while (c->NotAcknowledgedIndices.size() > 0)
                    {
                        auto First = *c->NotAcknowledgedIndices.begin();
                        if (c->Parts->IsEqualOrAfter(c->Parts->MaxHandled, First))
                        {
                            c->NotAcknowledgedIndices.erase(First);
                        }
                        else
                        {
                            break;
                        }
                    }
                    Indices.push_back(c->Parts->MaxHandled);
                    for (auto i : c->NotAcknowledgedIndices)
                    {
                        Indices.push_back(i);
                    }
                });

                std::shared_ptr<boost::asio::deadline_timer> Timer = nullptr;
                std::vector<std::shared_ptr<std::vector<std::uint8_t>>> Parts;
                CookedWritingContext.DoAction([&](std::shared_ptr<UdpWriteContext> cc)
                {
                    if (cc->Timer == nullptr) { return; }
                    Timer = cc->Timer;
                    cc->Timer = nullptr;
                    if (!IsRunning) { return; }

                    if (Indices.size() > 0)
                    {
                        auto Index = Indices[0];

                        auto NumIndex = Indices.size();
                        if (NumIndex > 0xFFFF)
                        {
                            return;
                        }

                        auto Flag = 8; //AUX

                        auto Length = 12 + 2 + NumIndex * 2;
                        auto Buffer = std::make_shared<std::vector<std::uint8_t>>();
                        Buffer->resize(Length, 0);
                        (*Buffer)[0] = static_cast<std::uint8_t>(SessionId & 0xFF);
                        (*Buffer)[1] = static_cast<std::uint8_t>((SessionId >> 8) & 0xFF);
                        (*Buffer)[2] = static_cast<std::uint8_t>((SessionId >> 16) & 0xFF);
                        (*Buffer)[3] = static_cast<std::uint8_t>((SessionId >> 24) & 0xFF);

                        Flag |= 1; //ACK
                        (*Buffer)[12] = static_cast<std::uint8_t>(NumIndex & 0xFF);
                        (*Buffer)[13] = static_cast<std::uint8_t>((NumIndex >> 8) & 0xFF);
                        int j = 0;
                        for (auto i : Indices)
                        {
                            (*Buffer)[14 + j * 2] = static_cast<std::uint8_t>(i & 0xFF);
                            (*Buffer)[14 + j * 2 + 1] = static_cast<std::uint8_t>((i >> 8) & 0xFF);
                            j += 1;
                        }
                        Indices.clear();

                        if (SecureContext != nullptr)
                        {
                            Flag |= 2; //ENC
                        }
                        (*Buffer)[4] = static_cast<std::uint8_t>(Flag & 0xFF);
                        (*Buffer)[5] = static_cast<std::uint8_t>((Flag >> 8) & 0xFF);
                        (*Buffer)[6] = static_cast<std::uint8_t>(Index & 0xFF);
                        (*Buffer)[7] = static_cast<std::uint8_t>((Index >> 8) & 0xFF);

                        std::int32_t Verification = 0;
                        if (SecureContext != nullptr)
                        {
                            std::vector<std::uint8_t> SHABuffer;
                            SHABuffer.resize(4);
                            memcpy(&SHABuffer[0], &(*Buffer)[4], 4);
                            auto SHA1 = Algorithms::Cryptography::SHA1(SHABuffer);
                            std::vector<std::uint8_t> Key;
                            Key.resize(SecureContext->ServerToken.size() + SHA1.size());
                            memcpy(&Key[0], &SecureContext->ServerToken[0], SecureContext->ServerToken.size());
                            memcpy(&Key[SecureContext->ServerToken.size()], &SHA1[0], SHA1.size());
                            auto HMACBytes = Algorithms::Cryptography::HMACSHA1(Key, *Buffer);
                            HMACBytes.resize(4);
                            Verification = HMACBytes[0] | (static_cast<std::int32_t>(HMACBytes[1]) << 8) | (static_cast<std::int32_t>(HMACBytes[2]) << 16) | (static_cast<std::int32_t>(HMACBytes[3]) << 24);
                        }
                        else
                        {
                            Verification = Algorithms::Cryptography::CRC32(*Buffer);
                        }

                        (*Buffer)[8] = static_cast<std::uint8_t>(Verification & 0xFF);
                        (*Buffer)[9] = static_cast<std::uint8_t>((Verification >> 8) & 0xFF);
                        (*Buffer)[10] = static_cast<std::uint8_t>((Verification >> 16) & 0xFF);
                        (*Buffer)[11] = static_cast<std::uint8_t>((Verification >> 24) & 0xFF);

                        Parts.push_back(Buffer);
                    }

                    if (cc->Parts->Parts.size() == 0) { return; }
                    auto t = boost::posix_time::microsec_clock::universal_time();
                    cc->Parts->ForEachTimedoutPacket(t, [&](int i, std::shared_ptr<std::vector<std::uint8_t>> d) { Parts.push_back(d); });
                    auto Wait = std::numeric_limits<int>::max();
                    for (auto Pair : cc->Parts->Parts)
                    {
                        auto p = std::get<1>(Pair);
                        auto pWait = (p->ResendTime - t).total_milliseconds() + 1;
                        if (pWait < Wait)
                        {
                            Wait = static_cast<int>(pWait);
                        }
                    }
                    cc->Timer = std::make_shared<boost::asio::deadline_timer>(io_service);
                    cc->Timer->expires_from_now(boost::posix_time::milliseconds(Wait));
                    cc->Timer->async_wait([=](const boost::system::error_code& error)
                    {
                        if (error == boost::system::errc::success)
                        {
                            Check();
                        }
                    });
                });
                if (Timer != nullptr)
                {
                    Timer->cancel();
                    Timer = nullptr;
                }
                for (auto p : Parts)
                {
                    try
                    {
                        SendPacket(RemoteEndPoint, p);
                    }
                    catch (...)
                    {
                        break;
                    }
                }
            }

            void SendPacket(boost::asio::ip::udp::endpoint RemoteEndPoint, std::shared_ptr<std::vector<std::uint8_t>> Data)
            {
                Socket.send_to(boost::asio::buffer(*Data), RemoteEndPoint);
            }

            static int GetMinNotLessPowerOfTwo(int v)
            {
                //计算不小于TotalLength的最小2的幂
                if (v < 1) { return 1; }
                auto n = 0;
                auto z = v - 1;
                while (z != 0)
                {
                    z >>= 1;
                    n += 1;
                }
                auto Value = 1 << n;
                if (Value == 0) { throw std::logic_error("InvalidOperationException"); }
                return Value;
            }

        public:
            void Connect()
            {
                IsRunningValue.Update([](bool b)
                {
                    if (b) { throw std::logic_error("InvalidOperationException"); }
                    return true;
                });
                if (RemoteEndPoint.address().is_v4())
                {
                    Socket.open(boost::asio::ip::udp::v4());
                    Socket.bind(boost::asio::ip::udp::endpoint(boost::asio::ip::address_v4::any(), 0));
                }
                else
                {
                    Socket.open(boost::asio::ip::udp::v6());
                    Socket.bind(boost::asio::ip::udp::endpoint(boost::asio::ip::address_v6::any(), 0));
                }
            }

            /// <summary>异步连接</summary>
            /// <param name="Completed">正常连接处理函数</param>
            /// <param name="UnknownFaulted">未知错误处理函数</param>
            void ConnectAsync(boost::asio::ip::udp::endpoint RemoteEndPoint, std::function<void(void)> Completed, std::function<void(const boost::system::error_code &)> UnknownFaulted)
            {
                try
                {
                    Connect();
                }
                catch (boost::system::system_error &e)
                {
                    UnknownFaulted(e.code());
                    return;
                }
                Completed();
            }

        private:
            static bool IsSocketErrorKnown(const boost::system::error_code &se)
            {
                if (se == boost::system::errc::connection_aborted) { return true; }
                if (se == boost::system::errc::connection_reset) { return true; }
                if (se == boost::asio::error::eof) { return true; }
                if (se == boost::system::errc::operation_canceled) { return true; }
                return false;
            }

            void CompletedSocket(std::shared_ptr<std::vector<std::uint8_t>> Buffer, std::function<void(std::function<void(void)>)> DoResultHandle, std::function<void(const boost::system::error_code &)> UnknownFaulted)
            {
                try
                {
                    if (Buffer->size() < 12)
                    {
                        return;
                    }
                    auto SessionId = (*Buffer)[0] | (static_cast<std::int32_t>((*Buffer)[1]) << 8) | (static_cast<std::int32_t>((*Buffer)[2]) << 16) | (static_cast<std::int32_t>((*Buffer)[3]) << 24);
                    auto Flag = (*Buffer)[4] | (static_cast<std::int32_t>((*Buffer)[5]) << 8);
                    auto Index = (*Buffer)[6] | (static_cast<std::int32_t>((*Buffer)[7]) << 8);
                    auto Verification = (*Buffer)[8] | (static_cast<std::int32_t>((*Buffer)[9]) << 8) | (static_cast<std::int32_t>((*Buffer)[10]) << 16) | (static_cast<std::int32_t>((*Buffer)[11]) << 24);
                    (*Buffer)[8] = 0;
                    (*Buffer)[9] = 0;
                    (*Buffer)[10] = 0;
                    (*Buffer)[11] = 0;

                    auto IsEncrypted = (Flag & 2) != 0;
                    auto SecureContext = this->SecureContextValue.Check<std::shared_ptr<class SecureContext>>([](std::shared_ptr<class SecureContext> v) { return v; });
                    if ((SecureContext != nullptr) != IsEncrypted)
                    {
                        return;
                    }

                    if (IsEncrypted)
                    {
                        std::vector<std::uint8_t> SHABuffer;
                        SHABuffer.resize(4);
                        memcpy(&SHABuffer[0], &(*Buffer)[4], 4);
                        auto SHA1 = Algorithms::Cryptography::SHA1(SHABuffer);
                        std::vector<std::uint8_t> Key;
                        Key.resize(SecureContext->ClientToken.size() + SHA1.size());
                        memcpy(&Key[0], &SecureContext->ClientToken[0], SecureContext->ClientToken.size());
                        memcpy(&Key[SecureContext->ClientToken.size()], &SHA1[0], SHA1.size());
                        auto HMACBytes = Algorithms::Cryptography::HMACSHA1(Key, *Buffer);
                        HMACBytes.resize(4);
                        auto HMAC = HMACBytes[0] | (static_cast<std::int32_t>(HMACBytes[1]) << 8) | (static_cast<std::int32_t>(HMACBytes[2]) << 16) | (static_cast<std::int32_t>(HMACBytes[3]) << 24);
                        if (HMAC != Verification) { return; }
                    }
                    else
                    {
                        //如果Flag中不包含ENC，则验证CRC32
                        if (Algorithms::Cryptography::CRC32(*Buffer) != Verification) { return; }

                        //只有尚未加密时可以设定
                        this->SessionIdValue.Update([=](int v) { return SessionId; });
                        auto Connected = false;
                        ConnectedValue.Update([&](bool v)
                        {
                            Connected = v;
                            return true;
                        });
                    }

                    int Offset = 12;
                    std::vector<int> Indices;
                    if ((Flag & 1) != 0)
                    {
                        auto NumIndex = (*Buffer)[Offset] | (static_cast<std::int32_t>((*Buffer)[Offset + 1]) << 8);
                        if (NumIndex > ReadingWindowSize) //若Index数量较大，则丢弃包
                        {
                            return;
                        }
                        Offset += 2;
                        Indices.resize(NumIndex, 0);
                        for (int k = 0; k < NumIndex; k += 1)
                        {
                            Indices[k] = (*Buffer)[Offset + k * 2] | (static_cast<std::int32_t>((*Buffer)[Offset + k * 2 + 1]) << 8);
                        }
                        Offset += NumIndex * 2;
                    }

                    auto Length = static_cast<std::int32_t>(Buffer->size()) - Offset;

                    if (Indices.size() > 0)
                    {
                        CookedWritingContext.DoAction([&](std::shared_ptr<UdpWriteContext> c)
                        {
                            auto First = Indices[0];
                            Indices.erase(Indices.begin());
                            c->Parts->Acknowledge(First, Indices);
                        });
                    }

                    bool Pushed = false;
                    std::vector<std::shared_ptr<std::vector<std::uint8_t>>> Parts;
                    RawReadingContext.DoAction([&](std::shared_ptr<UdpReadContext> c)
                    {
                        if (c->Parts->HasPart(Index))
                        {
                            Pushed = true;
                            return;
                        }
                        Pushed = c->Parts->TryPushPart(Index, Buffer, Offset, Length);
                        if (Pushed)
                        {
                            c->NotAcknowledgedIndices.insert(Index);
                            while (c->NotAcknowledgedIndices.size() > 0)
                            {
                                auto First = *c->NotAcknowledgedIndices.begin();
                                if (c->Parts->IsEqualOrAfter(c->Parts->MaxHandled, First))
                                {
                                    c->NotAcknowledgedIndices.erase(First);
                                }
                                else
                                {
                                    break;
                                }
                            }

                            while (true)
                            {
                                auto p = c->Parts->TryTakeFirstPart();
                                if (p == nullptr) { break; }
                                Parts.push_back(p->Data);
                            }
                        }
                    });

                    for (auto p : Parts)
                    {
                        auto ReadBuffer = VirtualTransportClient->GetReadBuffer();
                        auto ReadBufferLength = VirtualTransportClient->GetReadBufferOffset() + VirtualTransportClient->GetReadBufferLength();
                        if (static_cast<int>(p->size()) > static_cast<int>(ReadBuffer->size()) - ReadBufferLength)
                        {
                            UnknownFaulted(boost::system::error_code(boost::system::errc::no_buffer_space, boost::system::system_category()));
                            return;
                        }
                        memcpy(&(*ReadBuffer)[ReadBufferLength], &(*p)[0], p->size());

                        auto c = p->size();
                        while (true)
                        {
                            auto r = VirtualTransportClient->Handle(c);
                            if (r->OnContinue())
                            {
                                break;
                            }
                            else if (r->OnCommand())
                            {
                                DoResultHandle(r->Command->HandleResult);
                                auto RemainCount = VirtualTransportClient->GetReadBufferLength();
                                if (RemainCount <= 0)
                                {
                                    break;
                                }
                                c = 0;
                            }
                            else
                            {
                                throw std::logic_error("InvalidOperationException");
                            }
                        }
                    }
                }
                catch (boost::system::system_error &e)
                {
                    UnknownFaulted(e.code());
                    return;
                }
            }

        public:
            /// <summary>接收消息</summary>
            /// <param name="DoResultHandle">运行处理消息函数，应保证不多线程同时访问BinarySocketClient</param>
            /// <param name="UnknownFaulted">未知错误处理函数</param>
            void ReceiveAsync(std::function<void(std::function<void(void)>)> DoResultHandle, std::function<void(const boost::system::error_code &)> UnknownFaulted)
            {
                if (!IsRunningValue.Check<bool>([](bool b) { return b; })) { return; }

                auto ServerEndPoint = this->RemoteEndPoint;
                auto ReadHandler = [=](const boost::system::error_code &se, std::size_t Count)
                {
                    if (se != boost::system::errc::success)
                    {
                        if (IsSocketErrorKnown(se)) { return; }
                        UnknownFaulted(se);
                        return;
                    }
                    auto Buffer = std::make_shared<std::vector<std::uint8_t>>();
                    Buffer->resize(Count, 0);
                    memcpy(&(*Buffer)[0], &ReadBuffer[0], Count);
                    CompletedSocket(Buffer, DoResultHandle, UnknownFaulted);
                    Buffer = nullptr;
                    ReceiveAsync(DoResultHandle, UnknownFaulted);
                };
                Socket.async_receive_from(boost::asio::buffer(ReadBuffer), ServerEndPoint, ReadHandler);
            }

        private:
            bool IsDisposed;
        public:
            void Close()
            {
                if (IsDisposed) { return; }
                IsDisposed = true;

                IsRunningValue.Update([&](bool b) { return false; });
                try
                {
                    Socket.close();
                }
                catch (...)
                {
                }
                std::shared_ptr<boost::asio::deadline_timer> Timer = nullptr;
                CookedWritingContext.DoAction([&](std::shared_ptr<UdpWriteContext> c)
                {
                    if (c->Timer != nullptr)
                    {
                        Timer = c->Timer;
                        c->Timer = nullptr;
                    }
                });
                if (Timer != nullptr)
                {
                    Timer->cancel();
                    Timer = nullptr;
                }
            }
        };
    }
}