using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using StuddyBot.Core.BLL.Interfaces;
using StuddyBot.Core.BLL.Repositories;
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
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                Console.WriteLine(message + "      THREAD: {0}", Thread.CurrentThread.ManagedThreadId);
                _MyDialog = new MyDialog();
                _MyDialog.Message = message;
                _MyDialog.Sender = sender;
                _MyDialog.Time = time;

                _MyDialog.DialogsId = dialogId;


                _unitOfWork.MyDialogs.Create(_MyDialog);

                _unitOfWork.Save();

                scope.Complete();
            }
        }

        public async Task<string> UserLanguageInDb(string user_id)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                var user = _unitOfWork.Users.Get(user_id);
                if (user != null)
                {
                    return user.Language;                    
                }
                scope.Complete();
            }
            
            return string.Empty;
        }

        public async Task<string> LogUser(string user_id, string userLanguage)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
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
                        throw e;
                    }
                }
                scope.Complete();
            }
            

            return user_id;
        }

        public async Task AddConversationReference(string userId, string conversationReference)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                var user = _unitOfWork.Users.Get(userId);
                if (user != null)
                {
                    try
                    {
                        user.ConversationReference = conversationReference;
                        _unitOfWork.Save();
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine(e.InnerException);
                        throw e;
                    }
                }
                scope.Complete();
            }

            
        }

        public async Task<int> LogDialog(string userId)//, string userLanguage)
        {//public async Task<int> LogDialog(string userId)

            //var user = Task.Run(() => _unitOfWork.Users.Get(userId)).Result;
            //if (user == null)
            //{
            //    try
            //    {
            //        _unitOfWork.Users.Create(new User { Id = userId, Language = userLanguage });
            //        //_unitOfWork.Save();
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.InnerException);
            //        throw e;
            //    }
            //}

            // before was only this

            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                try
                {
                    //Task.Run(() => _unitOfWork.Dialogs.Create(new Dialogs { UserId = userId }))
                    //    .ContinueWith((t) => _unitOfWork.Save());
                    //await Task.Run(() => _unitOfWork.Save());
                    //await Task.Run(() => _unitOfWork.Dialogs.Create(new Dialogs { UserId = userId }));
                    //await Task.Run(() => _unitOfWork.Save());
                    _unitOfWork.Dialogs.Create(new Dialogs { UserId = userId });
                    _unitOfWork.Save();
                    var dialogs = _unitOfWork.Dialogs.GetAll();
                    //var maxId = await Task.Run(() => dialogs.Max(d => d.Id));
                    var maxId = dialogs.Max(d => d.Id);
                    return maxId;
                }
                catch (Exception e)
                {
                    if (e == null)
                    {
                        Console.WriteLine(e);
                    }
                    else
                    {
                        Console.WriteLine(e.InnerException);
                    }
                    //Console.WriteLine(e.InnerException);
                    throw e;
                }
            ////    finally
            ////    {
            ////        scope.Complete();
            ////    }
            ////}
            ////using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
            ////    new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            ////    {

            //    try
            //    {
            //        var dialogs = _unitOfWork.Dialogs.GetAll();
            //        //var maxId = await Task.Run(() => dialogs.Max(d => d.Id));
            //        var maxId = dialogs.Max(d => d.Id);
            //        return maxId;
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e.InnerException);
            //        throw e;
            //    }
                finally
                {
                    scope.Complete();
                }
            }

            
        }

        public async Task ChangeUserLanguage(string user_id, string userLanguage)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew,
                new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                try
                {
                    var user = _unitOfWork.Users.Get(user_id);
                    user.Language = userLanguage;
                    _unitOfWork.Save();
                    

                    //try
                    //{
                    //    user.Language = userLanguage;
                    //Task.Run(() => _unitOfWork.Save()); //TaskScheduler.FromCurrentSynchronizationContext()
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.InnerException);
                    throw e;
                }
                finally
                {
                    scope.Complete();
                }
            }

            
            
        }
    }
}
