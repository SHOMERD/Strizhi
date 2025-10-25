using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types;

namespace Strizhi.TelegramPart.Models
{
    public class MyContext : DbContext
    {
        public DbSet<TheUser> Users { get; set; }
        public DbSet<UserFile> Files { get; set; }
        public string DbPath { get; }

        public MyContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "TGBOT\\StrizhiUsers.db");
        }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
