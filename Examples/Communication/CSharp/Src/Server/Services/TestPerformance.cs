﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Communication;

namespace Server.Services
{
    public partial class ServerImplementation : IApplicationServer
    {
        public async Task<TestAddReply> TestAdd(TestAddRequest r)
        {
            return await Task.Factory.StartNew(() => TestAddReply.CreateResult(r.Left + r.Right));
        }

        public TestMultiplyReply TestMultiply(TestMultiplyRequest r)
        {
            var v = r.Operand;
            var o = 0.0;
            for (int k = 1; k <= 1000000; k += 1)
            {
                o += v * (k * 0.000001);
            }
            return TestMultiplyReply.CreateResult(o);
        }

        public TestAverageReply TestAverage(TestAverageRequest r)
        {
            if (r.Values.Count == 0) { return TestAverageReply.CreateResult(Optional<AverageResult>.Empty); }
            return TestAverageReply.CreateResult(new AverageResult { Value = r.Values.Average(v => v.Value) });
        }

        public TestTextReply TestText(TestTextRequest r)
        {
            return TestTextReply.CreateResult(r.Text);
        }

        public TestMessageReply TestMessage(TestMessageRequest r)
        {
            var Sessions = ServerContext.Sessions.ToList();
            var m = new TestMessageReceivedEvent { Message = r.Message };
            SessionContext.SendMessageCount += 1;
            foreach (var rc in Sessions)
            {
                if (rc == SessionContext) { continue; }
                rc.SessionLock.EnterWriteLock();
                try
                {
                    rc.ReceivedMessageCount += 1;
                    if (rc.EventPump != null)
                    {
                        rc.EventPump.TestMessageReceived(m);
                    }
                }
                finally
                {
                    rc.SessionLock.ExitWriteLock();
                }
            }
            return TestMessageReply.CreateSuccess(Sessions.Count);
        }

        public event Action<TestMessageReceivedEvent> TestMessageReceived;
    }
}
