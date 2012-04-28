﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using Communication;
using Communication.BaseSystem;
using Communication.Net;
using Communication.Binary;
using Communication.Json;
using Server.Services;

namespace Server
{
    /// <summary>
    /// 本类的所有非继承的公共成员均是线程安全的。
    /// </summary>
    public class BinaryServer : TcpServer<BinaryServer, BinarySession>
    {
        private ServerImplementation si;
        private JsonLogAspectWrapper<SessionContext> law;
        public BinaryServer<SessionContext> InnerServer { get; private set; }
        public ServerContext ServerContext { get; private set; }
        public int MaxBadCommands { get; set; }
        public Boolean ClientDebug { get; set; }

        private Boolean EnableLogNormalInValue = true;
        private Boolean EnableLogNormalOutValue = true;
        private Boolean EnableLogUnknownErrorValue = true;
        private Boolean EnableLogCriticalErrorValue = true;
        private Boolean EnableLogPerformanceValue = true;
        private Boolean EnableLogSystemValue = true;

        /// <summary>只能在启动前修改，以保证线程安全</summary>
        public Boolean EnableLogNormalIn
        {
            get
            {
                return EnableLogNormalInValue;
            }
            set
            {
                if (IsRunning) { throw new InvalidOperationException(); }
                EnableLogNormalInValue = value;
            }
        }
        /// <summary>只能在启动前修改，以保证线程安全</summary>
        public Boolean EnableLogNormalOut
        {
            get
            {
                return EnableLogNormalOutValue;
            }
            set
            {
                if (IsRunning) { throw new InvalidOperationException(); }
                EnableLogNormalOutValue = value;
            }
        }
        /// <summary>只能在启动前修改，以保证线程安全</summary>
        public Boolean EnableLogUnknownError
        {
            get
            {
                return EnableLogUnknownErrorValue;
            }
            set
            {
                if (IsRunning) { throw new InvalidOperationException(); }
                EnableLogUnknownErrorValue = value;
            }
        }
        /// <summary>只能在启动前修改，以保证线程安全</summary>
        public Boolean EnableLogCriticalError
        {
            get
            {
                return EnableLogCriticalErrorValue;
            }
            set
            {
                if (IsRunning) { throw new InvalidOperationException(); }
                EnableLogCriticalErrorValue = value;
            }
        }
        /// <summary>只能在启动前修改，以保证线程安全</summary>
        public Boolean EnableLogPerformance
        {
            get
            {
                return EnableLogPerformanceValue;
            }
            set
            {
                if (IsRunning) { throw new InvalidOperationException(); }
                EnableLogPerformanceValue = value;
            }
        }
        /// <summary>只能在启动前修改，以保证线程安全</summary>
        public Boolean EnableLogSystem
        {
            get
            {
                return EnableLogSystemValue;
            }
            set
            {
                if (IsRunning) { throw new InvalidOperationException(); }
                EnableLogSystemValue = value;
            }
        }

        public ConcurrentDictionary<SessionContext, BinarySession> SessionMappings = new ConcurrentDictionary<SessionContext, BinarySession>();

        public BinaryServer()
        {
            ServerContext = new ServerContext();
            ServerContext.GetSessions = () => SessionMappings.Keys;
            si = new ServerImplementation(ServerContext);
            law = new JsonLogAspectWrapper<SessionContext>(si);
            law.ClientCommandIn += (c, CommandName, Parameters) =>
            {
                if (EnableLogNormalIn)
                {
                    var CommandLine = String.Format(@"/{0} {1}", CommandName, Parameters);
                    RaiseSessionLog(new SessionLogEntry { Token = c.SessionTokenString, RemoteEndPoint = c.RemoteEndPoint, Time = DateTime.UtcNow, Type = "In", Message = CommandLine });
                }
            };
            law.ClientCommandOut += (c, CommandName, Parameters) =>
            {
                if (EnableLogNormalOut)
                {
                    var CommandLine = String.Format(@"/svr {0} {1}", CommandName, Parameters);
                    RaiseSessionLog(new SessionLogEntry { Token = c.SessionTokenString, RemoteEndPoint = c.RemoteEndPoint, Time = DateTime.UtcNow, Type = "Out", Message = CommandLine });
                }
            };
            law.ServerCommand += (c, CommandName, Parameters) =>
            {
                if (EnableLogNormalOut)
                {
                    var CommandLine = String.Format(@"/svr {0} {1}", CommandName, Parameters);
                    RaiseSessionLog(new SessionLogEntry { Token = c.SessionTokenString, RemoteEndPoint = c.RemoteEndPoint, Time = DateTime.UtcNow, Type = "Out", Message = CommandLine });
                }
            };

            InnerServer = new BinaryServer<SessionContext>(law);
            InnerServer.ServerEvent += OnServerEvent;
            ServerContext.SchemaHash = InnerServer.Hash.ToString("X16", System.Globalization.CultureInfo.InvariantCulture);

            base.MaxConnectionsExceeded += OnMaxConnectionsExceeded;
            base.MaxConnectionsPerIPExceeded += OnMaxConnectionsPerIPExceeded;
        }

        public void RaiseError(SessionContext c, String CommandName, String Message)
        {
            si.RaiseError(c, CommandName, Message);
        }

        private void OnServerEvent(SessionContext c, String CommandName, UInt32 CommandHash, Byte[] Parameters)
        {
            BinarySession Session = null;
            SessionMappings.TryGetValue(c, out Session);
            if (Session != null)
            {
                Session.WriteCommand(CommandName, CommandHash, Parameters);
            }
        }

        public event Action<SessionLogEntry> SessionLog;
        public void RaiseSessionLog(SessionLogEntry Entry)
        {
            if (SessionLog != null)
            {
                SessionLog(Entry);
            }
        }

        private void OnMaxConnectionsExceeded(BinarySession s)
        {
            if (s != null && s.IsRunning)
            {
                s.RaiseError("", "Client host rejected: too many connections, please try again later.");
            }
        }
        private void OnMaxConnectionsPerIPExceeded(BinarySession s)
        {
            if (s != null && s.IsRunning)
            {
                s.RaiseError("", "Client host rejected: too many connections from your IP(" + s.RemoteEndPoint.Address + "), please try again later.");
            }
        }
    }
}