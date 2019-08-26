using StuddyBot.Core.BLL.Interfaces;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StuddyBot.Core.BLL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private StuddyBotContext db;
        private DialogRepository dialogRepository;
        private DialogsRepository dialogsRepository;
        private UserRepository userRepository;

        public UnitOfWork(StuddyBotContext studdyBotContext)
        {
            db = studdyBotContext;
        }

        public IRepository<MyDialog> MyDialogs
        {
            get
            {
                if (dialogRepository == null)
                    dialogRepository = new DialogRepository(db);
                return dialogRepository;
            }
        }

        public IRepository<Dialogs> Dialogs
        {
            get
            {
                if (dialogsRepository == null)
                    dialogsRepository = new DialogsRepository(db);
                return dialogsRepository;
            }
        }

        public IRepository<User> Users
        {
            get
            {
                if (userRepository == null)
                    userRepository = new UserRepository(db);
                return userRepository;
            }
        }

      
        public void Save()
        {
            db.SaveChanges();
        }

        private bool disposed = false;
        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
