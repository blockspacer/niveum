﻿#pragma once

#include "Servers/IContext.h"
#include "SessionContext.h"

#include "BaseSystem/Optional.h"
#include "BaseSystem/LockedVariable.h"
#include "BaseSystem/ThreadLocalRandom.h"

#include <cstdint>
#include <vector>
#include <unordered_set>
#include <string>
#include <functional>
#include <memory>
#include <stdexcept>

namespace Server
{
    class ServerContext : public IServerContext, public std::enable_shared_from_this<ServerContext>
    {
    public:
        ServerContext();

        std::wstring HeadCommunicationSchemaHash;
        Optional<std::wstring> CommunicationSchemaHashToVersion(std::wstring Hash)
        {
            if (Hash == HeadCommunicationSchemaHash) { return Optional<std::wstring>::CreateHasValue(L""); }
            if (Hash == L"690E84A3D3914C38") { return Optional<std::wstring>::CreateHasValue(L"1"); }
            return Optional<std::wstring>::Empty();
        }

        bool EnableLogNormalIn() { return EnableLogNormalInValue; }
        bool EnableLogNormalOut() { return EnableLogNormalOutValue; }
        bool EnableLogUnknownError() { return EnableLogUnknownErrorValue; }
        bool EnableLogCriticalError() { return EnableLogCriticalErrorValue; }
        bool EnableLogPerformance() { return EnableLogPerformanceValue; }
        bool EnableLogSystem() { return EnableLogSystemValue; }
        bool ServerDebug() { return ServerDebugValue; }
        bool ClientDebug() { return ClientDebugValue; }

        void EnableLogNormalIn(bool value) { EnableLogNormalInValue = value; }
        void EnableLogNormalOut(bool value) { EnableLogNormalOutValue = value; }
        void EnableLogUnknownError(bool value) { EnableLogUnknownErrorValue = value; }
        void EnableLogCriticalError(bool value) { EnableLogCriticalErrorValue = value; }
        void EnableLogPerformance(bool value) { EnableLogPerformanceValue = value; }
        void EnableLogSystem(bool value) { EnableLogSystemValue = value; }
        void ServerDebug(bool value) { ServerDebugValue = value; }
        void ClientDebug(bool value) { ClientDebugValue = value; }

        std::function<void()> Shutdown; //跨线程事件(订阅者需要保证线程安全)
        void RaiseShutdown()
        {
            if (Shutdown != nullptr) { Shutdown(); }
        }

        std::function<void(std::shared_ptr<SessionLogEntry>)> SessionLog;
        void RaiseSessionLog(std::shared_ptr<SessionLogEntry> Entry)
        {
            if (SessionLog != nullptr)
            {
                SessionLog(Entry);
            }
        }

    private:
        BaseSystem::LockedVariable<std::unordered_set<std::shared_ptr<SessionContext>>> SessionSet;
        BaseSystem::ThreadLocalRandom RNG;
    public:
        std::shared_ptr<std::vector<std::shared_ptr<SessionContext>>> Sessions()
        {
            return SessionSet.Check<std::shared_ptr<std::vector<std::shared_ptr<SessionContext>>>>([](const std::unordered_set<std::shared_ptr<SessionContext>> &ss) { return std::make_shared<std::vector<std::shared_ptr<SessionContext>>>(std::begin(ss), std::end(ss)); });
        }
        void RegisterSession(std::shared_ptr<ISessionContext> SessionContext)
        {
            auto sc = std::dynamic_pointer_cast<class SessionContext>(SessionContext);
            if (sc == nullptr) { throw std::logic_error("InvalidOperationException"); }
            SessionSet.DoAction([=](std::unordered_set<std::shared_ptr<class SessionContext>> &ss)
            {
                ss.insert(sc);
            });
        }
        bool TryUnregisterSession(std::shared_ptr<ISessionContext> SessionContext)
        {
            auto sc = std::dynamic_pointer_cast<class SessionContext>(SessionContext);
            if (sc == nullptr) { throw std::logic_error("InvalidOperationException"); }
            auto Success = false;
            SessionSet.DoAction([sc, &Success](std::unordered_set<std::shared_ptr<class SessionContext>> &ss)
            {
                if (ss.count(sc) > 0)
                {
                    ss.erase(sc);
                    Success = true;
                }
            });
            return Success;
        }

        std::shared_ptr<ISessionContext> CreateSessionContext()
        {
            std::vector<std::uint8_t> SessionToken;
            SessionToken.push_back(RNG.NextInt<std::uint8_t>(0, 255));
            SessionToken.push_back(RNG.NextInt<std::uint8_t>(0, 255));
            SessionToken.push_back(RNG.NextInt<std::uint8_t>(0, 255));
            SessionToken.push_back(RNG.NextInt<std::uint8_t>(0, 255));
            return std::make_shared<SessionContext>(SessionToken);
        }
        std::shared_ptr<IServerImplementation> CreateServerImplementation(std::shared_ptr<SessionContext> SessionContext);
        std::pair<std::shared_ptr<IServerImplementation>, std::shared_ptr<IBinarySerializationServerAdapter>> CreateServerImplementationWithBinaryAdapter(std::shared_ptr<ISessionContext> SessionContext);

    private:
        bool EnableLogNormalInValue;
        bool EnableLogNormalOutValue;
        bool EnableLogUnknownErrorValue;
        bool EnableLogCriticalErrorValue;
        bool EnableLogPerformanceValue;
        bool EnableLogSystemValue;
        bool ServerDebugValue;
        bool ClientDebugValue;
    };
}
