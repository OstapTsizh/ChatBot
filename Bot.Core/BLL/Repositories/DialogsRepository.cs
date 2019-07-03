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
    public class DialogsRepository: IRepository<Dialogs>
    {
        private StuddyBotContext db;

        public DialogsRepository(StuddyBotContext context)
        {
            db = context;
        }

        public Dialogs Get(int id)
        {
            return db.Dialogs.Find(id);
        }

        public IEnumerable<Dialogs> GetAll()
        {
            return db.Dialogs;
        }

        public void Create(Dialogs item)
        {
            db.Dialogs.Add(item);
        }

        public void Update(Dialogs item)
        {
            db.Entry(item).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            Dialogs dialog = db.Dialogs.Find(id);
            if (dialog != null)
                db.Dialogs.Remove(dialog);
        }

        public IEnumerable<Dialogs> Find(Func<Dialogs, bool> predicate)
        {
            return db.Dialogs.Where(predicate).ToList();
        }

      
    }
}
