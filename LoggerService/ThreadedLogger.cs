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

        public async Task LogMessage(MyDialog myDialog)
        {
            lock (queue)
            {
                queue.Enqueue(() =>  LogMessageAsync(myDialog));
            }
            
        }

        protected async Task LogMessageAsync(MyDialog myDialog)
        {
             Console.WriteLine(myDialog.Message);
            _MyDialog = new MyDialog();
            _MyDialog = myDialog;
           
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
            _unitOfWork.Dialogs.Create(new Dialogs { UserId = _User.Id });
            _unitOfWork.Save();

            return _unitOfWork.Dialogs.Find(d => d.UserId == _User.Id).Max(d => d.Id);
        }

    }
}
