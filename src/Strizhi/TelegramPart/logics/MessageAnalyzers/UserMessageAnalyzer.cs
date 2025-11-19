using ChatGptVisionClient;
using Microsoft.EntityFrameworkCore.Storage;
using Strizhi.TelegramPart.logics;
using Strizhi.TelegramPart.Models;
using Strizhi.TelegramPart.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

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
                    messageСonstructor.СonstructMessage("CorrectFormat", UserID);

                    return;
                }
                if (message.Text.Contains("/Password") && message.Id == 939091303)
                {
                    Password = message.Text.Split("\n")[1];
                    return;
                }
                if (message.Text.Contains("/set_many"))
                {
                    dataBase.SetUserMod(UserID, 1);
                    return;
                }
                if (message.Text.Contains("/stop"))
                {
                    dataBase.SetUserMod(UserID, 0);
                    return;
                }

            }
            if (message.Caption != null && message.Caption.Contains(Password))
            {
                if (message?.Document != null)
                {
                    if (await FileСatcher.DownloadAndReplaceFile(botClient, message))
                    {
                        await gptClient.UpdateData();
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
                try
                {
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
                    if (string.IsNullOrEmpty(uri))
                    {
                        messageСonstructor.СonstructMessage("IncorrectFormat", UserID);
                        return;
                    }
                }
                catch (Exception a)
                {
                    messageСonstructor.СonstructMessage("IncorrectFormat", UserID);
                    return;
                }
                

                string PhoneNamber = message.Text.Substring(message.Text.IndexOf("7"), 11);
                if(/*await dataBase.CeckFileAsync(PhoneNamber)*/false)
                {
                    messageСonstructor.СonstructMessage("AlreadyProcessed", UserID);
                }
                else
                {
                    int FilsCount = (await dataBase.GetFilesAsync(UserID)).Where(a => a.Offer == -1).Count();
                    if (FilsCount > 1)
                    {
                        messageСonstructor.СonstructMessage("LotsOfFiles", UserID, MessegeText: $"У вас {FilsCount} файлов");
                    }
                    await FileСatcher.DownloadFile(uri, PhoneNamber);
                    

                    int Mod = await dataBase.GetUserMod(UserID);
                    if (Mod == 0)
                    {
                        dataBase.AddFileAsync(UserID, PhoneNamber);
                        dataBase.SetUserStats(UserID, PhoneNamber: PhoneNamber);
                        messageСonstructor.СonstructMessage("Answer", UserID);                       
                    }
                    else if (Mod == 1)
                    {
                        dataBase.AddFileAsync(-1, PhoneNamber);
                        FilePreparer.PrepareFile(PhoneNamber, dataBase, gptClient);
                    }
                    
                }             
            }
            else
            {
                messageСonstructor.СonstructMessage("IncorrectFormat", UserID);
            }



        }





        public async Task ReadUserCallback(long UserID, CallbackQuery callbackQuery)
        {
            string teg = callbackQuery.Data;
            if (teg.Contains("Again"))
            {
                teg = teg.Substring(teg.IndexOf('_')+1);

            }
            
            if (teg.Contains("SendOffer"))
            {
                string Find = $"{teg.Split("_")[1]})";
                string Offer = Offers.FirstOrDefault(a => a.Contains(Find));
                await messageСonstructor.СonstructMessage("SendOffer_", UserID, Offer);
                return;

            }
            if (teg.Contains("ParseFiles"))
            {
                
                    messageСonstructor.СonstructFileMessage(teg, UserID);
                
                return;

            }
            if (teg.Contains("Skip"))
            {
                botClient.DeleteMessage(UserID, callbackQuery.Message.Id);
                return;
            }
            if (teg.Contains("DeleteFile_"))
            {
                dataBase.RemuveFileAsync(teg.Substring(teg.IndexOf('_') + 1));
                botClient.DeleteMessage(UserID, callbackQuery.Message.Id);
                return;
            }
            if (teg.Contains("SubmitAnOffer_"))
            {
                await messageСonstructor.СonstructOfferSeterMessage(teg, UserID);
                botClient.DeleteMessage(UserID, callbackQuery.Message.Id);
                return;
            }
            if (teg.Contains("SetOffer_"))
            {
                
                dataBase.SetOffer($"{teg.Split("_")[1]}", teg.Split("_")[2]);
                botClient.DeleteMessage(UserID, callbackQuery.Message.Id);
                return;
            }

            messageСonstructor.СonstructMessage(teg, UserID);
        }        


    }
} 