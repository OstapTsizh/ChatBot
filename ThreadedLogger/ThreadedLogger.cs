using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoggerService
{
    #region V - 1
    public class ThreadedLogger : IDisposable
    {
        Queue<Action> queue = new Queue<Action>();

        public ThreadedLogger()
        {
            ProcessQueue();
        }


        async Task ProcessQueue()
        {
            while (true)
            {
                await Task.Delay(10_000);

                Queue<Action> queueCopy;

                lock (queue)
                {
                    queueCopy = new Queue<Action>(queue);
                    queue.Clear();
                }

                foreach (var log in queueCopy)
                {
                    log();
                }
            }
        }

        public async Task LogMessage(string row)
        {
            lock (queue)
            {
                queue.Enqueue(() => LogMessageAsync(row));
            }

        }

        protected async Task LogMessageAsync(string row)
        {
            // TODO: write async to db
            // db -> DI

            Console.WriteLine(row);


        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }

    #endregion

    #region V - 2
    //public class ThreadedLogger : LoggerBase
    //{

    //    Queue<Action> queue = new Queue<Action>();
    //    AutoResetEvent hasNewItems = new AutoResetEvent(false);
    //    volatile bool waiting = false;

    //    public ThreadedLogger() : base()
    //    {
    //        Thread loggingThread = new Thread(new ThreadStart(ProcessQueue));
    //        loggingThread.IsBackground = true;
    //        loggingThread.Start();
    //    }


    //    void ProcessQueue()
    //    {
    //        while (true)
    //        {
    //            waiting = true;
    //            hasNewItems.WaitOne(10_000, true);
    //            waiting = false;

    //            Queue<Action> queueCopy;
    //            lock (queue)
    //            {
    //                queueCopy = new Queue<Action>(queue);
    //                queue.Clear();
    //            }

    //            foreach (var log in queueCopy)
    //            {
    //                log();
    //            }
    //        }
    //    }

    //    public void LogMessage(string row)
    //    {
    //        lock (queue)
    //        {
    //            queue.Enqueue(() => AsyncLogMessage(row));
    //        }
    //        hasNewItems.Set();
    //    }

    //    protected void AsyncLogMessage(string row)
    //    { }


    //    public void Flush()
    //    {
    //        while (!waiting)
    //        {
    //            Thread.Sleep(1);
    //        }

    //        foreach (var item in queue)
    //        {
    //            Console.WriteLine(queue.Dequeue());
    //        }
    //    }

    //    public override void Log(ILogMessage message)
    //    {

    //    }

    //    public override Task LogAsync(ILogMessage message)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    #endregion
}
