﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BaseSystem
{
    public class CountedThreadPool : TaskScheduler, IDisposable
    {
        private CancellationTokenSource TokenSource;
        private BlockingCollection<Task> Tasks;
        private Thread[] Threads;

        public CountedThreadPool(String Name, int ThreadCount)
        {
            TokenSource = new CancellationTokenSource();

            //ConcurrentQueue在同一个线程既可以作为生产者又可以作为消费者，且调用的函数处理的内容很少时，存在scalability不好的问题
            //http://download.microsoft.com/download/B/C/F/BCFD4868-1354-45E3-B71B-B851CD78733D/PerformanceCharacteristicsOfThreadSafeCollection.pdf
            Tasks = new BlockingCollection<Task>(new ConcurrentBag<Task>());

            var Token = TokenSource.Token;
            Threads = Enumerable.Range(0, ThreadCount).Select((i, tt) => new Thread(() =>
            {
                Thread.CurrentThread.Name = Name + "[" + i.ToString() + "]";
                SynchronizationContext.SetSynchronizationContext(new CallbackSynchronizationContext(QueueUserWorkItem));
                while (true)
                {
                    if (Token.IsCancellationRequested) { return; }
                    Task t;
                    try
                    {
                        t = Tasks.Take(Token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    TryExecuteTask(t);
                }
            })).ToArray();
            foreach (var tt in Threads)
            {
                tt.Start();
            }
        }

        public void QueueUserWorkItem(Action WorkItem)
        {
            Task.Factory.StartNew(WorkItem, CancellationToken.None, TaskCreationOptions.None, this);
        }

        public void Dispose()
        {
            TokenSource.Cancel();
            foreach (var t in Threads)
            {
                t.Join();
            }

            Tasks.Dispose();
            TokenSource.Dispose();
        }

        protected override void QueueTask(Task Task)
        {
            Tasks.Add(Task);
        }

        protected override bool TryExecuteTaskInline(Task Task, bool TaskWasPreviouslyQueued)
        {
            if (!TaskWasPreviouslyQueued)
            {
                return TryExecuteTask(Task);
            }
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return Tasks.ToList();
        }
    }
}
