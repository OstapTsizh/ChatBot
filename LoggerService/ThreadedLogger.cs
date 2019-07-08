using System;
using System.Collections.Generic;
using System.Linq;
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
           // _unitOfWork.Dialogs.Create(new Dialogs { UserId = "125" });
            _unitOfWork.Save();
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
                    log.Invoke();
                }
            }
        }

        public async Task LogMessage(string message, string sender, DateTimeOffset time)
        {
            lock (queue)
            {
                queue.Enqueue(() =>  LogMessageAsync(message, sender, time));
            }
            
        }

        protected async Task LogMessageAsync(string message, string sender, DateTimeOffset time)
        {
             Console.WriteLine(message);
            _MyDialog = new MyDialog();
            _MyDialog.Message = message;
            _MyDialog.Sender = message;
            _MyDialog.Time = time;
            _MyDialog.DialogsId = _Dialogs.Id;
           
            _unitOfWork.MyDialogs.Create(_MyDialog);

            _unitOfWork.Save();
        }

        public void GetExistingUserId(string user_id)
        {
            _User = new User { Id = user_id };
        }

        public string LogUser(string user_id)
        {
            _User = new User { Id = user_id };
            _unitOfWork.Users.Create(_User);
            _unitOfWork.Save();
            return user_id;
        }

        public int LogDialog()
        {
            _Dialogs = new Dialogs { UserId = _User.Id };
            _unitOfWork.Dialogs.Create(_Dialogs);
            _unitOfWork.Save();

            _Dialogs.Id = _unitOfWork.Dialogs.Find(d => d.UserId == _User.Id).Max(d => d.Id);

            return _Dialogs.Id;
        }

    }
}
