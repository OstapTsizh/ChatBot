using Services.Helpers;
using Services.NotificationService.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.NotificationService.Workers
{
    public class NotificationWorker : IServerWorker
    {
        private TimeSpan _checkPeriod;
        private DateTime _lastExecuting;

        private static HttpClient _httpClient;

        //private StuddyBotContext _db; 


        public void Initialize()
        {
            _checkPeriod = ApplicationConfiguration.AlphaWorkerPeriod;

            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:3978") };
        }

        public string WorkerName
        {
            get { return typeof(NotificationWorker).Name; }
        }

        public async void DoWork(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (HasOccurrence())
                {
                    try
                    {
                        await DoWorkInternalAsync();
                        _lastExecuting = DateTime.Now;
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.ToString(), WorkerName);
                        Console.WriteLine("error.", WorkerName);
                    }
                }
                Thread.Sleep(2_000);

            }
        }

        private async Task DoWorkInternalAsync()
        {
            //Do what you want to do here
            Console.WriteLine("Notificating users...", WorkerName);

            // TODO: collection of courses
            // foreach course in courses where startDate <= 7days
            //       course.NotificateUsers

            var _uri = new UriBuilder(_httpClient.BaseAddress);

            _uri.Path += "api/Notify";

            await _httpClient.GetAsync(_uri.ToString());
        }

        private bool HasOccurrence()
        {
            return (DateTime.Now - _checkPeriod) > _lastExecuting;
        }
    }
}
