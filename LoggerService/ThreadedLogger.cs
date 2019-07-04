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
        private Dialogs _Dialogs { get; set; }
        private MyDialog _MyDialog { get; set; }
        private User _User { get; set; }

        Queue<Action> queue = new Queue<Action>();
       
        public ThreadedLogger(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
           // _unitOfWork.Save();
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
               // _unitOfWork.Save();
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

        public async Task GetExistingUserId(string user_id)
        {
            _User = new User { Id = user_id };
        }

        public int GetExistingDialogId()
        {
            return _Dialogs.Id;
        }

        public async Task LogUser(string user_id)
        {
            _User = new User { Id = user_id };
            _unitOfWork.Users.Create(_User);
            _unitOfWork.Save();
        }

        public  int LogDialog()
        {
            _Dialogs = new Dialogs { UserId = _User.Id };
            _unitOfWork.Dialogs.Create(_Dialogs);
            _unitOfWork.Save();

            _Dialogs.Id = _unitOfWork.Dialogs.GetAll().Max(d => d.Id);
            return _Dialogs.Id;

        }

    }
}
