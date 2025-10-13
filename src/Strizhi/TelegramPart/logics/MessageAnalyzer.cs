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
using ChatGptVisionClient;

namespace Strizhi.TelegramPart.logics
{
    public class MessageAnalyzer
    {
        public DataBase dataBase { get; set; }
        public ITelegramBotClient botClient { get; set; }
        public UserMessageAnalyzer userMessageAnalyzer { get; set; }
        public MessageСonstructor messageСonstructor { get; set; }

        private bool Otladka = false;

        GptClient gptClient { get; set; }


        public MessageAnalyzer(DataBase dataBase, ITelegramBotClient BotClient, MessageСonstructor messageСonstructor, GptClient gptClient)
        {
            userMessageAnalyzer = new UserMessageAnalyzer(dataBase, BotClient, messageСonstructor, gptClient);

            this.messageСonstructor = messageСonstructor;
            this.dataBase = dataBase;
            botClient = BotClient;
            this.gptClient = gptClient;

        }


        public async Task AnalyzUpdate(Telegram.Bot.Types.Update update)
        {
            
            switch (update.Type)
            {
                case UpdateType.Message:
                    AnalyzMessage(update);
                    if (update.Message.Id == 939091303)
                    {
                        if (update.Message.Text == "-1")
                        {
                            Otladka = false;
                        }
                        else if (update.Message.Text == "1")
                        {
                            Otladka = false;
                        }
                        if (update.Message.Text.Contains("gpt-5"))
                        {
                            gptClient.GPTVersion = update.Message.Text;
                        }

                    }
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


            
            if (Otladka)
            {
                await botClient.SendMessage(939091303, $"{user}, написал \"{message.Text}\"");
                Console.WriteLine($"{user}, написал \"{message.Text}\"");
            }
            
            userMessageAnalyzer.ReadUserText(user.Id, message);


        }

        

        






    }
}
