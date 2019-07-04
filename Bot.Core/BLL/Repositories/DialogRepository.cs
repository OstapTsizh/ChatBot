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
    public class DialogRepository : IRepository<MyDialog>
    {
        private StuddyBotContext db;

        public DialogRepository(StuddyBotContext context)
        {
            db = context;
        }

        public MyDialog Get(int id)
        {
            return db.Dialog.Find(id);
        }

        public IEnumerable<MyDialog> GetAll()
        {
            return db.Dialog;
        }

        public void Create(MyDialog item)
        {
            db.Dialog.Add(item);
        }

        public void Update(MyDialog item)
        {
            db.Entry(item).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            MyDialog dialog = db.Dialog.Find(id);
            if (dialog != null)
                db.Dialog.Remove(dialog);
        }

        public IEnumerable<MyDialog> Find(Func<MyDialog, bool> predicate)
        {
            return db.Dialog.Where(predicate).ToList();
        }
        
    }
}
