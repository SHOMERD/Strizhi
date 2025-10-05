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

namespace Strizhi.TelegramPart.logics
{
    public class MessageСonstructor
    {
        List<Menu> menus { get; set; }
        public ITelegramBotClient botClient { get; set; }
        private DataBase dataBase { get; set; }
        private GptClient gptClient { get; set; }


        public MessageСonstructor(ITelegramBotClient BotClient, DataBase dataBase)
        {
            //menus = JsonConvert.DeserializeObject<List<Menu>>(File.ReadAllText(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("\\bin")) + "/BotStructure.json"));
            botClient = BotClient;
            this.dataBase = dataBase;
        }



        public async Task<bool> СonstructMessage(string MessegeText, long UserID)
        {
            MessegeText = "Ответ от чата:\n\n" + MessegeText;

            List<InlineKeyboardButton[]> inlineKeyboardButton = new List<InlineKeyboardButton[]>();
            inlineKeyboardButton.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Переотправить", $"Again_{dataBase.GetUserPhoneNamber(UserID)}") });

            await botClient.SendMessage(
                chatId: UserID,
                text: MessegeText,
                replyMarkup: new InlineKeyboardMarkup(inlineKeyboardButton)
                );


            return true;
        }



    }
}
