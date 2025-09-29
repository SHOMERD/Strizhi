using Strizhi.TelegramPart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;
using Strizhi.TelegramPart.Models;
using Strizhi.TelegramPart.logics;
using System.IO;
using static System.Net.WebRequestMethods;

namespace Strizhi.TelegramPart.logics.MessageAnalyzers
{
    public class UserMessageAnalyzer
    {
        public DataBase dataBase { get; set; }
        public ITelegramBotClient botClient { get; set; }
        public MessageСonstructor messageСonstructor { get; set; }

        public UserMessageAnalyzer(DataBase dataBase, ITelegramBotClient BotClient, MessageСonstructor messageСonstructor)
        {
            this.dataBase = dataBase;
            botClient = BotClient;
            this.messageСonstructor = messageСonstructor;
        }






        public async void ReadUserText(long UserID, Message message)
        {
            if (message != null)
            {
                if (!string.IsNullOrEmpty(message.Text)  && message.Text.Contains("/start"))
                {
      
                    
                }
            }

        }





        public async Task ReadUserCallback(long UserID, CallbackQuery callbackQuery)
        {
            switch (callbackQuery.Data)
            {
                case "Again":
                    

                    break;
                default:
                    break;
            }

        }        


    }
}