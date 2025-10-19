using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using ChatGptVisionClient;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Strizhi.TelegramPart.logics
{
    public class FileСatcher
    {
        public static async Task<bool> DownloadFile(string uri, string fileName)
        {
            if (string.IsNullOrEmpty(uri)) {
                return false;
            }
            uri = uri + "/txt";

            byte[] data;

            using (var client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(uri))
            using (HttpContent content = response.Content)
            {
                data = await content.ReadAsByteArrayAsync();
                string str = System.Text.Encoding.Default.GetString(data);
                str = await TextCleaner.CleanText(str);
                data = System.Text.Encoding.UTF8.GetBytes(str);
                using (FileStream file = File.Create(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\Clients\\" + fileName + ".txt")))
                    file.Write(data, 0, data.Length);
            }
            return true;

        }


        public static async Task DeliteFile(string fileName)
        {
            File.Delete(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\Clients\\" + fileName + ".txt"));
        }


        public static async Task<bool> DownloadAndReplaceFile(ITelegramBotClient botClient, Message document)
        {
            string filePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\" + document.Document.FileName);
            if (File.Exists(filePath))
            {
                var file = await botClient.GetFile(document.Document.FileId);



                // Скачиваем файл во временную память
                using (var stream = new MemoryStream())
                {
                    await botClient.DownloadFile(file.FilePath, stream);
                    stream.Position = 0;

                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
                return true;

            }else {return false;}

        }

        public static void Loger(string text)
        {
            string fileName = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\" +"LOGS.txt");
            if (!File.Exists(fileName))
            {
                File.Create(fileName);
            }
            File.AppendAllText(fileName, $"\n\n%%               {DateTime.Now}\n" + text+ "&&");
        }
    }
}
