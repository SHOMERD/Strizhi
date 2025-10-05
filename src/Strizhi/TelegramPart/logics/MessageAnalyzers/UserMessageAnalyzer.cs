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
using ChatGptVisionClient;

namespace Strizhi.TelegramPart.logics.MessageAnalyzers
{
    public class UserMessageAnalyzer
    {
        public DataBase dataBase { get; set; }
        public ITelegramBotClient botClient { get; set; }
        public MessageСonstructor messageСonstructor { get; set; }
        public GptClient gptClient { get; set; }

        public UserMessageAnalyzer(DataBase dataBase, ITelegramBotClient BotClient, MessageСonstructor messageСonstructor, GptClient gptClient)
        {
            this.dataBase = dataBase;
            botClient = BotClient;
            this.messageСonstructor = messageСonstructor;
            this.gptClient = gptClient;

        }






        public async void ReadUserText(long UserID, Message message)
        {
            if (message != null)
            {
                if (message.Text.Contains("/start"))
                {
                    if (await dataBase.CeckUserAsync(UserID))
                    {
                        await botClient.SendMessage(
                             message.Chat.Id,
                             text: $"Здравствуйте {message.From.FirstName.Trim('@')}" +
                             $"\nРады снова вас видеть в нашем боте"
                             );
                    }
                    else
                    {
                        await dataBase.AddUserAsync(UserID, message.From.Username);
                        await botClient.SendMessage(
                            message.Chat.Id,
                            text: $"Здравствуйте {message.From.FirstName.Trim('@')}");
                    }
                    return;
                }
            }

            if (message.ReplyMarkup != null)
            {
                string uri = "";

                List<System.Collections.Generic.IEnumerable<Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton>> a = message.ReplyMarkup.InlineKeyboard.ToList();
                for (int i = 0; i < a.Count; i++)
                {
                    for (int j = 0; j < a[i].Count(); j++)
                    {
                        if (a[i].ToList()[0].Text.Contains("Открыть полный отчет"))
                        {
                            uri = a[i].ToList()[0].Url;
                        }
                    }

                }

                string PhoneNamber = message.Text.Substring(message.Text.IndexOf("7"), 11);
                if (PhoneNamber != await dataBase.GetUserPhoneNamber(UserID))
                {
                    await FileСatcher.DeliteFile(await dataBase.GetUserPhoneNamber(UserID));
                }           

                await FileСatcher.DownloadFile(uri, PhoneNamber);
                string anser = await gptClient.SengToGPT(PhoneNamber);
                dataBase.SetUserStats(UserID, PhoneNamber: PhoneNamber);
                messageСonstructor.СonstructMessage(anser, UserID);

            }



        }





        public async Task ReadUserCallback(long UserID, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data.Contains("Again"))
            {
                string anser = await gptClient.SengToGPT((callbackQuery.Data.Split('_'))[1]);
                messageСonstructor.СonstructMessage(anser, UserID);

            }

        }        


    }
}