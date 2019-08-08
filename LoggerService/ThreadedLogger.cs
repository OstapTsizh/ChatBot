using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StuddyBot.Core.BLL.Interfaces;
using StuddyBot.Core.DAL.Entities;

namespace LoggerService
{
    public class ThreadedLogger
    {
        IUnitOfWork _unitOfWork;
        private MyDialog _MyDialog { get; set; }

        Queue<Action> queue = new Queue<Action>();
       
        public ThreadedLogger(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            ProcessQueue();            
        }

       
        async Task ProcessQueue()
        {
            while (true)
            {
                
                await Task.Delay(15_000);

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

        public async Task LogMessage(string message, string sender, DateTimeOffset time, int dialogId)
        {
            lock (queue)
            {
                queue.Enqueue(() =>  LogMessageAsync(message, sender, time, dialogId));
            }

           
        }

        protected async Task LogMessageAsync(string message, string sender, DateTimeOffset time, int dialogId)
        {
             Console.WriteLine(message + "      THREAD: {0}", Thread.CurrentThread.ManagedThreadId);
            _MyDialog = new MyDialog();
            _MyDialog.Message = message;
            _MyDialog.Sender = sender;
            _MyDialog.Time = time;

            _MyDialog.DialogsId = dialogId;
           
            _unitOfWork.MyDialogs.Create(_MyDialog);

            _unitOfWork.Save();
        }

        public async Task<string> LogUser(string user_id, string userLanguage)
        {
            var user = _unitOfWork.Users.Get(user_id);
            if (user == null)
            {
                try
                {
                    _unitOfWork.Users.Create(new User { Id = user_id, Language = userLanguage });
                    
                    _unitOfWork.Save();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.InnerException);
                    throw;
                }
                //_unitOfWork.Users.Create(new User { Id = user_id, Language = userLanguage});
                //_unitOfWork.Save();
            }

            return user_id;
        }

        public async Task<int> LogDialog(string userId)
        {
            _unitOfWork.Dialogs.Create(new Dialogs { UserId = userId });
            _unitOfWork.Save();

            return _unitOfWork.Dialogs.GetAll().Max(d => d.Id);

        }

    }
}
