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

        List<string> Offers { get; set; }
        private string Password;

        public UserMessageAnalyzer(DataBase dataBase, ITelegramBotClient BotClient, MessageСonstructor messageСonstructor, GptClient gptClient)
        {
            this.dataBase = dataBase;
            botClient = BotClient;
            this.messageСonstructor = messageСonstructor;
            this.gptClient = gptClient;

            SetOffers();
        }


        public async void SetOffers()
        {
            string offersFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\offers.txt");

            Offers = (await System.IO.File.ReadAllTextAsync(offersFilePath)).Split("-----------------------------------").ToList();
        }



        public async void ReadUserText(long UserID, Message message)
        {
            

            if (message.Text != null)
            {
                if (message.Text.Contains("/start"))
                {                   
                    await botClient.SendMessage(
                        message.Chat.Id,
                        text: $"Здравствуйте {message.From.FirstName.Trim('@')}");

                    return;
                }
                if (message.Text.Contains("/Password") || message.Id == 939091303)
                {
                    Password = message.Text.Split("\n")[1];
                    return;
                }              
            }
            if (message.Caption != null && message.Caption.Contains(Password))
            {
                if (message?.Document != null)
                {
                    if (await FileСatcher.DownloadAndReplaceFile(botClient, message))
                    {
                        await botClient.SendMessage(message.Chat.Id, "Файл успешно заменён!");
                    }
                    else
                    {
                        await botClient.SendMessage(message.Chat.Id, "Файла нет!");
                    }
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
                dataBase.SetUserStats(UserID, PhoneNamber: PhoneNamber);

                messageСonstructor.СonstructMessage("Answer", UserID);             
               

            }



        }





        public async Task ReadUserCallback(long UserID, CallbackQuery callbackQuery)
        {
            string teg = callbackQuery.Data;
            if (teg.Contains("Again"))
            {
                teg = teg.Substring(teg.IndexOf('_')+1);

            }
            if (teg.Contains("Reset_File"))
            {
                await FileСatcher.DeliteFile(await dataBase.GetUserPhoneNamber(UserID));
                dataBase.SetUserStats(UserID, PhoneNamber: "-");
            }
            if (teg.Contains("SendOffer"))
            {
                string Find = $"{teg.Split("_")[1]})";
                string Offer = Offers.FirstOrDefault(a => a.Contains(Find));
                await messageСonstructor.СonstructMessage("SendOffer_", UserID, Offer);

            }

            messageСonstructor.СonstructMessage(teg, UserID);
        }        


    }
}