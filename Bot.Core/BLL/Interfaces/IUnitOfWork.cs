using StuddyBot.Core.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.BLL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Dialogs> Dialogs { get; }
        IRepository<MyDialog> MyDialogs { get; }
       
        void Save();
    }
}
