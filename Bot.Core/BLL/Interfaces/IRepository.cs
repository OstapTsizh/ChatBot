using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.BLL.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T Get(object id);
        IEnumerable<T> Find(Func<T, Boolean> predicate);
        void Create(T item);
        void Update(T item);
        void Delete(object id);
    }
}
