using Services.NotificationService.Interfaces;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.NotificationService
{
    public class NotificationService : ServiceBase
    {
        private readonly List<Task> _workerTasks = new List<Task>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public NotificationService()
        {
            //Set initializer DbContext here if needed:
            //Database.SetInitializer<DbContext>(null);

        }

        protected override void OnStart(string[] args)
        {
            StartService();
        }

        protected override void OnStop()
        {
            StopService();
        }

        public void StartService()
        {

            AddAssemblyWorkers();

            foreach (var workerTask in _workerTasks)
            {
                workerTask.Start();
            }

            Console.WriteLine("Notification Service succesfuly started.");
        }

        public void StopService()
        {
            _cancellationTokenSource.Cancel();
            Task.WaitAll(_workerTasks.ToArray());
        }

        private void AddAssemblyWorkers()
        {
            foreach (var type in GetType().Assembly.GetTypes())
            {
                InitServerWorker(type);
            }
        }

        private void InitServerWorker(Type type)
        {
            if ((type == typeof(IServerWorker)) || !typeof(IServerWorker).IsAssignableFrom(type))
            {
                return;
            }

            var serverWorker = Activator.CreateInstance(type) as IServerWorker;

            if (serverWorker == null)
            {
                return;
            }

            serverWorker.Initialize();

            var workerTask = new Task(() => serverWorker.DoWork(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            _workerTasks.Add(workerTask);

        }
    }
}

