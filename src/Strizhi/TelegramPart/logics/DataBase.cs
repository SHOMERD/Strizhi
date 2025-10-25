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

        public async Task SetUserStats(long UserID, string Username = null, int ExpectedDataStatus = int.MinValue, int ChildExpectedDataStatus = int.MinValue, string PhoneNamber = "no")
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
       
        public async Task<UserFile> GetFileAsync(int FileID)
        {
            UserFile userFile = Context.Files.Where(a => a.Id == FileID).FirstOrDefault();
            return userFile;
        }
        public async Task<UserFile> GetFileAsync(string FileName)
        {
            UserFile userFile = Context.Files.Where(a => a.FileName == FileName).FirstOrDefault();
            return userFile;
        }
        public async Task<List<UserFile>> GetFilesAsync(long UserID)
        {
            List<UserFile> userFile = Context.Files.Where(a => a.UserID == UserID).ToList() ;
            return userFile;
        }
        public async Task<int> GetFilesCountAsync(long UserID)
        {
            List<UserFile> userFile = Context.Files.Where(a => a.UserID == UserID).ToList();
            return userFile.Count;
        }

        public async Task<bool> CeckUserAsync(long UserID)
        {
            try
            {
                TheUser user = await GetUserAsync(UserID);
                return user != null;
            }
            catch (Exception)
            {
                return false;
            }
            
        } 
        public async Task<bool> CeckFileAsync(string FileName)
        {
            try
            {
                UserFile File = await GetFileAsync(FileName);
                return File != null;
            }
            catch (Exception)
            {
                return false;
            }
            
        } 


        public async Task AddUserAsync(long UserID, string Username)
        {
            TheUser user = new TheUser() { UserID = UserID, Username = Username, PhoneNamber ="no" };


            if (!await CeckUserAsync(UserID))
            {
                Context.Users.Add(user);
            }
            Context.SaveChanges();

        }
        public async Task AddFileAsync(long UserID, string FileName)
        {
            UserFile userFile = new UserFile() { Offer = -1, UserID = UserID,  FileName= FileName, СlientName = FileСatcher.GetClientName(FileName) };


            if (!await CeckFileAsync(FileName))
            {
                Context.Files.Add(userFile);
            }
            Context.SaveChanges();

        }

        public async Task RemuveFileAsync(int FileID)
        {
            var File = await GetFileAsync(FileID);
            if (File != null)
            {
                FileСatcher.DeliteFile(File.FileName);
                Context.Remove(File);
                Context.SaveChanges();
            }

        }
        public async Task RemuveFileAsync(string FileName)
        {
            var File = await GetFileAsync(FileName);
            if (File != null)
            {
                FileСatcher.DeliteFile(File.FileName);
                Context.Remove(File);
                Context.SaveChanges();
            }

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
                "CREATE TABLE Users(Id INTEGER PRIMARY KEY AUTOINCREMENT, Username TEXT, UserID BIGINTEGER , PhoneNamber TEXT);"+
                "CREATE TABLE Files(Id INTEGER PRIMARY KEY AUTOINCREMENT, UserName TEXT, UserID BIGINTEGER , FileName TEXT, Offer INTEGER, СlientName TEXT);";

        command.ExecuteNonQuery();

        }

    }
}
