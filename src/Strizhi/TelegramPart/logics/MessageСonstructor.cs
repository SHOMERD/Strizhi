using ChatGptVisionClient;
using Strizhi.TelegramPart.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Strizhi.ChatGPTPart.Models;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Strizhi.TelegramPart.logics
{
    public class MessageСonstructor
    {
        List<Menu> menus { get; set; }
        public ITelegramBotClient botClient { get; set; }
        private DataBase dataBase { get; set; }
        public GptClient gptClient { get; set; }


        public MessageСonstructor(ITelegramBotClient BotClient, DataBase dataBase)
        {
            menus = JsonConvert.DeserializeObject<List<Menu>>(File.ReadAllText(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\BotStructure.json")));
            botClient = BotClient;
            this.dataBase = dataBase;
        }



        public async Task<bool> СonstructMessage(string Tag, long UserID, string MessegeText = null)
        {
            Menu activeMenu = await GetMenu(Tag);
            Menu menu = new Menu();

            if (activeMenu == null)
            {
                return false;
            }
            

            if (string.IsNullOrEmpty(MessegeText))
            {
                MessegeText = activeMenu.MessageText;
            }


            if (activeMenu.PromtNumber != -1)
            {
                
                Message Pad = await botClient.SendMessage(
                    chatId: UserID,
                    text: "Пока это сообщение есть, GPT обрабатывает запрос"
                );
                menu = await ProcessPromt(activeMenu, activeMenu.PromtNumber, await dataBase.GetUserPhoneNamber(UserID));

                MessegeText = menu.MessageText;
                await botClient.DeleteMessage(UserID, Pad.Id);
               
            }

            List<string> ButtonsTexts = new List<string>();
            List<string> ButtonsTegs = new List<string>();
            if (menu.Teg != "Error")
            {
                ButtonsTexts.AddRange(activeMenu.ButtonsTexts);
                ButtonsTegs.AddRange(activeMenu.ButtonsTegs);
            }
            ButtonsTexts.AddRange(menu.ButtonsTexts);            
            ButtonsTegs.AddRange(menu.ButtonsTegs);

            FileСatcher.Loger("отправлено сообщение\n"+MessegeText);
            if (MessegeText.Length > 4000)
            {              
                var result = new List<string>();
                int startIndex = 0;
                while (startIndex < MessegeText.Length)
                {
                    
                    int endIndex = Math.Min(startIndex + 4000, MessegeText.Length);
                    string chunk = MessegeText.Substring(startIndex, endIndex - startIndex);
                    result.Add(chunk);
                    startIndex += 4000;
                }

                for (int i = 0; i < result.Count-1; i++)
                {
                    await botClient.SendMessage(
                        chatId: UserID,
                        text: result[i]
                        );
                }
                await botClient.SendMessage(
                    chatId: UserID,
                    text: result.Last(),
                    replyMarkup: (await GetKeyboardButtons(ButtonsTexts, ButtonsTegs, activeMenu.ParentTeg))
                    );
            }
            else
            {
                await botClient.SendMessage(
                chatId: UserID,
                text: MessegeText,
                replyMarkup: (await GetKeyboardButtons(ButtonsTexts, ButtonsTegs, activeMenu.ParentTeg))
                );
            }
            
            return true;
        }

        public async Task<bool> СonstructFileMessage(string Tag, long UserID)
        {
            List<UserFile> userFiles = await dataBase.GetFilesAsync(UserID);
            Menu menu = await GetMenu("ParseFiles");
            List<string> ButtonsTexts = new List<string>();
            List<string> ButtonsTegs  = new List<string>();

            for (int i = 0; i < userFiles.Count; i++)
            {
                ButtonsTexts.AddRange(menu.ButtonsTexts);
                ButtonsTegs.AddRange(menu.ButtonsTegs);
                ButtonsTegs[0] += userFiles[i].FileName;
                ButtonsTegs[1] += userFiles[i].FileName;
                await botClient.SendMessage(
                    chatId: UserID,
                    text: $"Файл по номеру {userFiles[i].FileName}\nПердположительное имя \"{userFiles[i].СlientName}\"",
                    replyMarkup: (await GetKeyboardButtons(ButtonsTexts, ButtonsTegs))
                );
            }

            return true;
        }
        public async Task<bool> СonstructOfferSeterMessage(string Tag, long UserID)
        {
            Menu menu = await GetMenu("SubmitAnOffer_");

            List<string> ButtonsTexts = new List<string>();
            List<string> ButtonsTegs = new List<string>();

            string offersFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\offers.txt");
            List<string> Offers = (await System.IO.File.ReadAllTextAsync(offersFilePath)).Split("-----------------------------------").ToList();
            
            Offers.RemoveAll(a=> a =="");
            Offers.RemoveAll(a => a == "\n");

            string FileName = Tag.Substring(Tag.IndexOf('_') + 1);

            ButtonsTexts = Offers.Select(a => a.Substring(1,a.IndexOf(")"))).ToList();
            ButtonsTegs = Offers.Select(a => $"SetOffer_{FileName}_"+ a.Substring(1, a.IndexOf(")")-1)).ToList();


            await botClient.SendMessage(
                    chatId: UserID,
                    text: $"Номер офера:",
                    replyMarkup: (await GetKeyboardButtons(ButtonsTexts, ButtonsTegs))
                );
            

            return true;
        }
        public Uri ChekUrl(string Url)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                try
                {
                    return new Uri(Url);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        async Task<Menu> ProcessPromt(Menu activeMenu, int PromtNumber, string PhoneNamber)
        {
            Menu menu = new Menu();
            Answer Anser = await gptClient.RequestToAI(activeMenu.PromtNumber, PhoneNamber);
            if (Anser == null)
            {
                menu.MessageText = $"GPT не смог ответить из-за технической ошибки";
                menu.ButtonsTegs = new List<string>() { $"Again_{activeMenu.Teg}" };
                menu.ButtonsTexts = new List<string>() { $"Заново" };
                menu.Teg = "Error";
                return menu;
            }
            

            menu = await SetMenuBatons(activeMenu, Anser);
            menu.MessageText = await GetAnseText(Anser);
            
            return menu;
        }

        async Task<string> GetAnseText(Answer Anser)
        {
            string MessageText = Anser.Text;
            for (int i = 0; i < Anser.Parts.Count; i++)
            {
                MessageText += "\n__________\n";
                MessageText += Anser.Parts[i].Description;
            }

            return MessageText;
        }

        async Task<Menu> SetMenuBatons(Menu activeMenu, Answer Anser)
        {
            Menu menu = new Menu();
            string BattonTeg = "";
            string BattonText = "";
            switch (activeMenu.PromtNumber)
            {
                case 1:
                    BattonTeg = "SendOffer_";
                    BattonText = "Прислать офер ";
                    break;
                case 2:
                    BattonTeg = "SendOffer_";
                    BattonText = "Прислать офер ";
                    break;
                default:                   
                    break;
            }

            List<string> Tags = Anser.Parts.Select(part => BattonTeg + part.ButtonTag).ToList();
            List<string> Texts = Anser.Parts.Select(part => BattonText + part.BriefDescription).ToList();
            menu.ButtonsTegs.AddRange(Tags);
            menu.ButtonsTexts.AddRange(Texts);

            return menu;
        }


        public async Task<InlineKeyboardMarkup> GetKeyboardButtons(List<string> ButtonsTexts = null, List<string> ButtonsTegs = null, string ParentTeg = null)
        {
            List<InlineKeyboardButton[]> inlineKeyboardButton = new List<InlineKeyboardButton[]>();
            if (ButtonsTexts != null && ButtonsTegs != null && ButtonsTexts.Count == ButtonsTegs.Count)
            {
                for (int i = 0; i < ButtonsTexts.Count; i++)
                {
                    if (ChekUrl(ButtonsTegs[i]) != null)
                    {
                        inlineKeyboardButton.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithUrl(ButtonsTexts[i], ButtonsTegs[i]) });
                    }
                    else
                    {
                        inlineKeyboardButton.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(ButtonsTexts[i], ButtonsTegs[i]) });
                    }
                }


                if (!string.IsNullOrEmpty(ParentTeg))
                {
                    inlineKeyboardButton.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Назад", ParentTeg) });
                }              

            }
            return new InlineKeyboardMarkup(inlineKeyboardButton);
        }
        

        public async Task<Menu> GetMenu(string Tag)
        {
            for (int i = 0; i < menus.Count; i++)
            {
                if (menus[i].Teg == Tag)
                {
                    return menus[i];
                }
            }
            return null;
        }

        public async Task<bool> ErrorMessage(string MessegeText)
        {
            await botClient.SendMessage(
                chatId: 939091303,
                text: MessegeText
                );


            return true;
        }


        public async Task UpdateData()
        {
            menus = JsonConvert.DeserializeObject<List<Menu>>(File.ReadAllText(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\BotStructure.json")));

        }


    }
}
