using Microsoft.Bot.Builder.Dialogs;
using Microsoft.EntityFrameworkCore;
using StuddyBot.Core.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StuddyBot.Core.DAL.Data
{
    public class StuddyBotContext : DbContext
    {
        public StuddyBotContext() : base() { }

        public StuddyBotContext(DbContextOptions<StuddyBotContext> options)
            : base(options)
        { }


        public DbSet<MyDialog> Dialog { get; set; }
        public DbSet<Dialogs> Dialogs { get; set; }
        public DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-1I6TUGA;Initial Catalog=StuddyBotBD;Integrated Security=True;");
           
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    // использование Fluent API
        //    base.OnModelCreating(modelBuilder);
        //}



    }
}

