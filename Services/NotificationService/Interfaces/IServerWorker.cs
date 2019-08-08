using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Services.NotificationService.Interfaces
{
    interface IServerWorker
    {
        void Initialize();
        void DoWork(CancellationToken token);
        string WorkerName { get; }
    }
}
