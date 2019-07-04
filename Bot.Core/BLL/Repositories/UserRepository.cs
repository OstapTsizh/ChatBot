using Microsoft.EntityFrameworkCore;
using StuddyBot.Core.BLL.Interfaces;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StuddyBot.Core.BLL.Repositories
{
    class UserRepository: IRepository<User>
    {
        private StuddyBotContext db;

        public UserRepository(StuddyBotContext context)
        {
            db = context;
        }

        public User Get(int id)
        {
            return db.User.Find(id);
        }

        public IEnumerable<User> GetAll()
        {
            return db.User;
        }

        public void Create(User item)
        {
            db.User.Add(item);
        }

        public void Update(User item)
        {
            db.Entry(item).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            User user = db.User.Find(id);
            if (user != null)
                db.User.Remove(user);
        }

        public IEnumerable<User> Find(Func<User, bool> predicate)
        {
            return db.User.Where(predicate).ToList();
        }
    }
}
