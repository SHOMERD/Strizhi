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

        public async Task SetUserStats(long UserID, string PhoneNamber = "no")
        {
            TheUser theUser = await GetUserAsync(UserID);
          
            if (!string.IsNullOrEmpty(PhoneNamber))
            {              
                theUser.PhoneNamber = PhoneNamber;
            }


            Context.Users.Update(theUser);
            Context.SaveChanges();

        }

        public async Task SetOffer(string FileName = null, string OfferNamber = "")
        {
            UserFile userFile = await GetFileAsync(FileName);

            if (!string.IsNullOrEmpty(OfferNamber))
            {
                userFile.Offer = Convert.ToInt32(OfferNamber);
                Context.Files.Update(userFile);
                Context.SaveChanges();
            }
          
        }
        public async Task<TheUser> GetUserAsync(long GUserID)
        {
            TheUser theUser = Context.Users.First(a => a.UserID == GUserID);
            return theUser;
        }
        public async Task<string> GetUserPhoneNamber(long GUserID)
        {
            string theUser = (Context.Users.First(a => a.UserID == GUserID)).PhoneNamber;
            return theUser;
        }
       
        public async Task<UserFile> GetFileAsync(int FileID)
        {
            UserFile userFile = Context.Files.First(a => a.Id == FileID );
            return userFile;
        }
        public async Task<UserFile> GetFileAsync(string FileName)
        {
            UserFile userFile = Context.Files.First(a => a.FileName == FileName);
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


        public async Task AddUserAsync(long UserID)
        {
            TheUser user = new TheUser() { UserID = UserID, PhoneNamber ="no" };


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

        public async Task CreateDataBase()
        {
            using var db = new MyContext();

            var connection = new SqliteConnection($"Data Source={db.DbPath}");
            connection.Open();
            SqliteCommand command = new SqliteCommand();
            command.Connection = connection;

            command.CommandText =
                "CREATE TABLE Users(Id INTEGER PRIMARY KEY AUTOINCREMENT, UserID BIGINTEGER , PhoneNamber TEXT);"+
                "CREATE TABLE Files(Id INTEGER PRIMARY KEY AUTOINCREMENT, UserID BIGINTEGER , FileName TEXT, Offer INTEGER, СlientName TEXT);";

        command.ExecuteNonQuery();

        }

    }
}
