﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Communication;
using Communication.Net;
using Communication.Json;

namespace Client
{
    public sealed class JsonClient : IJsonSender, IDisposable
    {
        private IClientImplementation<ClientContext> ci;
        public JsonClient<ClientContext> InnerClient { get; private set; }
        private ClientContext Context;

        private IPEndPoint RemoteEndPoint;
        private StreamedAsyncSocket s;

        public JsonClient(IPEndPoint RemoteEndPoint, IClientImplementation<ClientContext> ci)
        {
            this.RemoteEndPoint = RemoteEndPoint;
            s = new StreamedAsyncSocket(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
            this.ci = ci;
            InnerClient = new JsonClient<ClientContext>(this, ci);
            Context = new ClientContext();
            Context.DequeueCallback = InnerClient.DequeueCallback;
        }

        public void Connect()
        {
            s.InnerSocket.Connect(RemoteEndPoint);
        }

        void IJsonSender.Send(String CommandName, String Parameters)
        {
            var Message = "/" + CommandName + " " + Parameters + "\r\n";
            var Bytes = Encoding.UTF8.GetBytes(Message);
            s.InnerSocket.Send(Bytes);
        }

        private Byte[] Buffer = new Byte[8 * 1024];
        private int BufferLength = 0;

        private Boolean IsSocketErrorKnown(SocketError se)
        {
            if (se == SocketError.ConnectionAborted) { return true; }
            if (se == SocketError.ConnectionReset) { return true; }
            if (se == SocketError.Shutdown) { return true; }
            if (se == SocketError.OperationAborted) { return true; }
            return false;
        }

        public void Receive(Action<SocketError> UnknownFaulted)
        {
            Action<SocketError> Faulted = se =>
            {
                if (!IsSocketErrorKnown(se))
                {
                    UnknownFaulted(se);
                }
            };

            Action<int> Completed = null;
            Completed = Count =>
            {
                if (Count == 0)
                {
                    return;
                }
                var FirstPosition = 0;
                var CheckPosition = BufferLength;
                BufferLength += Count;
                while (true)
                {
                    var LineFeedPosition = -1;
                    for (int i = CheckPosition; i < BufferLength; i += 1)
                    {
                        Byte b = Buffer[i];
                        if (b == '\n')
                        {
                            LineFeedPosition = i;
                            break;
                        }
                    }
                    if (LineFeedPosition >= 0)
                    {
                        var LineBytes = Buffer.Skip(FirstPosition).Take(LineFeedPosition - FirstPosition).Where(b => b != '\r').ToArray();
                        var Line = Encoding.UTF8.GetString(LineBytes, 0, LineBytes.Length);

                        var triple = Line.Split(new Char[] { ' ' }, 3);
                        if (triple.Length != 3) { throw new InvalidOperationException(); }
                        if (triple[0] != "/svr") { throw new InvalidOperationException(); }
                        InnerClient.HandleResult(Context, triple[1], triple[2]);

                        FirstPosition = LineFeedPosition + 1;
                        CheckPosition = FirstPosition;
                    }
                    else
                    {
                        break;
                    }
                }
                if (FirstPosition > 0)
                {
                    var CopyLength = BufferLength - FirstPosition;
                    for (int i = 0; i < CopyLength; i += 1)
                    {
                        Buffer[i] = Buffer[FirstPosition + i];
                    }
                    BufferLength = CopyLength;
                }
                if (s == null) { return; }
                s.ReceiveAsync(Buffer, BufferLength, Buffer.Length - BufferLength, Completed, Faulted);
            };

            s.ReceiveAsync(Buffer, BufferLength, Buffer.Length - BufferLength, Completed, Faulted);
        }

        public void Close()
        {
            s.Shutdown(SocketShutdown.Both);
            s.Close();
        }

        public void Dispose()
        {
            s.Dispose();
            s = null;
        }
    }
}