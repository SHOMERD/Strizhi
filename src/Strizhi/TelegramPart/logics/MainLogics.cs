using ChatGptVisionClient;
using Strizhi.TelegramPart.logics.MessageAnalyzers;
using System;
using System.Data.SqlTypes;
using System.IO;
using Telegram;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Strizhi.TelegramPart.logics;
using static System.Net.Mime.MediaTypeNames;
namespace Strizhi.TelegramPart.logics
{
    public class MainLogics
    {
        private ITelegramBotClient botClient { get; set; }

        private ReceiverOptions receiverOptions { get; set; }

        private MessageСonstructor messageСonstructor { get; set; }
        private MessageAnalyzer messageAnalyzer { get; set; }
        private DataBase dataBase { get; set; }


        public MainLogics()
        {
            Start();
        }


        public async Task Start()
        {

            dataBase = new DataBase();

            Directory.CreateDirectory(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT"));
            Directory.CreateDirectory(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\Clients"));
            SetTelegramBotClient();

            messageСonstructor = new MessageСonstructor(botClient, dataBase);          
            GptClient gptClient = new GptClient(messageСonstructor);
            messageСonstructor.gptClient = gptClient;


            messageAnalyzer = new MessageAnalyzer(dataBase, botClient, messageСonstructor, gptClient);


            Console.WriteLine("Оху*ть, РВБОТАЕТ!!!");
        }


        private async Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {

            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            botClient.SendMessage(939091303, ErrorMessage);
            Console.WriteLine(ErrorMessage);
            FileСatcher.Loger(ErrorMessage);

        }


        private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                messageAnalyzer.AnalyzUpdate(update);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                FileСatcher.Loger(ex.ToString());

            }

        }

        private async void SetTelegramBotClient()
        {

            string f = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\TelegramBotToken.txt");
            string c = File.ReadAllText(f);

            Console.WriteLine(c);
            if (string.IsNullOrEmpty(c))
            {
                Console.WriteLine("Телеграм токена нет");
                FileСatcher.Loger("Телеграм токена нет");
            }
            botClient = new TelegramBotClient(c);

            receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                UpdateType.Message,
                UpdateType.CallbackQuery,
                UpdateType.MyChatMember,
                UpdateType.ChatMember,
                UpdateType.ChatJoinRequest
            },

                DropPendingUpdates = true,
            };

            using var cts = new CancellationTokenSource();


            botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

            var me = await botClient.GetMe();
            Console.WriteLine($"{me.FirstName} запущен!");
        }


    }
}
