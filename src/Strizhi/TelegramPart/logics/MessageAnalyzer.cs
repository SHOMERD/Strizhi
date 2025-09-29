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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;
using Strizhi.TelegramPart.logics.MessageAnalyzers;

namespace Strizhi.TelegramPart.logics
{
    public class MessageAnalyzer
    {
        public DataBase dataBase { get; set; }
        public ITelegramBotClient botClient { get; set; }
        public UserMessageAnalyzer userMessageAnalyzer { get; set; }

        public MessageСonstructor messageСonstructor { get; set; }


        public MessageAnalyzer(DataBase dataBase, ITelegramBotClient BotClient, MessageСonstructor messageСonstructor)
        {
            userMessageAnalyzer = new UserMessageAnalyzer(dataBase, BotClient, messageСonstructor);

            this.messageСonstructor = messageСonstructor;
            this.dataBase = dataBase;
            botClient = BotClient;

        }


        public async Task AnalyzUpdate(Telegram.Bot.Types.Update update)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    AnalyzMessage(update);
                    break;

                case UpdateType.CallbackQuery:
                    await userMessageAnalyzer.ReadUserCallback(update.CallbackQuery.From.Id, update.CallbackQuery);
                    break;

                default:
                    break;

            }
            return;

        }


        public async Task AnalyzMessage(Telegram.Bot.Types.Update update)
        {
            var message = update.Message;

            var user = message.From;

            var chat = message.Chat;


            Console.WriteLine($"{user}, написал \"{message.Text}\"");
            await botClient.SendMessage(939091303, $"{user}, написал \"{message.Text}\"");

            
            userMessageAnalyzer.ReadUserText(user.Id, message);


        }

        

        






    }
}
