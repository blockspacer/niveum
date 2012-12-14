﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Communication;
using Communication.BaseSystem;

namespace Server
{
    public class ServerContext
    {
        //跨线程共享只读访问

        public event Action Shutdown; //跨线程事件(订阅者需要保证线程安全)
        public void RaiseShutdown()
        {
            if (Shutdown != null) { Shutdown(); }
        }

        public ICollection<SessionContext> Sessions { get { return SessionSet.Check(ss => ss.ToArray()); } }
        public LockedVariable<HashSet<SessionContext>> SessionSet = new LockedVariable<HashSet<SessionContext>>(new HashSet<SessionContext>());
    }
}
