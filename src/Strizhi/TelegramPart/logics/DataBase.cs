using ChatGptVisionClient;
using Strizhi.TelegramPart.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace Strizhi.TelegramPart.logics
{

    public class DataBase
    {
        public MyContext Context { get; set; }

        public DataBase()
        {
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\StrizhiUsers.db");
            Context = new MyContext();
            try
            {
                Context.Users.Where(A => A.Id == 0).FirstOrDefault();
            }
            catch (Exception)
            {
                CreateDataBase();
            }
        }

        public async Task SetUserStats(long UserID, string Username = null, int ExpectedDataStatus = int.MinValue, int ChildExpectedDataStatus = int.MinValue, string PhoneNamber = "")
        {
            TheUser theUser = await GetUserAsync(UserID);

            if (!string.IsNullOrEmpty(Username))
            {
                theUser.Username = Username;
            }

            if (!string.IsNullOrEmpty(PhoneNamber))
            {              
                theUser.PhoneNamber = PhoneNamber;
            }


            Context.Users.Update(theUser);
            Context.SaveChanges();

        }

        public async Task SetUserPhoto(long UserID,string FileId)
        {
            string f = Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("\\bin")) + "\\TelegramBotToken.txt";
            string c = File.ReadAllText(f);
            string url = $"https://api.telegram.org/file/bot{c}/{FileId}";
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{UserID}.jpg");



            using (var httpClient = new HttpClient())
            {
                var bytes = await httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(path, bytes);
            }
        }

        public async Task<TheUser> GetUserAsync(long GUserID)
        {
            TheUser theUser = Context.Users.Where(a => a.UserID == GUserID).FirstOrDefault();
            return theUser;
        }



        public async Task<TheUser> GetUserAsync(string Username)
        {
            TheUser theUser = Context.Users.Where(a => a.Username == Username).FirstOrDefault();
            return theUser;
        }
        public async Task<string> GetUserPhoneNamber(long GUserID)
        {
            string theUser = (Context.Users.Where(a => a.UserID == GUserID).FirstOrDefault()).PhoneNamber;
            return theUser;
        }

        public async Task<bool> CeckUserAsync(long UserID)
        {
            TheUser user = await GetUserAsync(UserID);
            return user != null;
        }


        public async Task AddUserAsync(long UserID, string Username)
        {
            TheUser user = new TheUser() { UserID = UserID, Username = Username, PhoneNamber ="" };


            if (!await CeckUserAsync(UserID))
            {
                Context.Users.Add(user);
            }
            Context.SaveChanges();

        }



        public async Task RemuveUserAsync(long UserID)
        {
            var user = await GetUserAsync(UserID);
            if (user != null)
            {
                Context.Remove(user);
                Context.SaveChanges();
            }

        }

        public async Task RemuveChatFromMembersChecker(long ChatID)
        {
            RemuveUserAsync(ChatID);
        }

        public async Task CreateDataBase()
        {
            using var db = new MyContext();

            var connection = new SqliteConnection($"Data Source={db.DbPath}");
            connection.Open();
            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;

            command.CommandText =
                "CREATE TABLE Users(Id INTEGER PRIMARY KEY AUTOINCREMENT, Username TEXT, UserID BIGINTEGER , PhoneNamber TEXT);";

            command.ExecuteNonQuery();

        }

    }
}
