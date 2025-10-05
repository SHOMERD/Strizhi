using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ChatGptVisionClient
{
    public class GptClient
    {
        private readonly HttpClient _http;
        private string ApiKey;
        private string c;
        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        public GptClient(string c)
        {
            _http = new HttpClient();
            this.ApiKey = c;
            //SengToGPT();
        }

        public async Task<string> SengToGPT(string ClientFileName)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {             
                return "Set OPENAI_API_KEY environment variable first.";
            }
            string filePath1 = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "offers.docx");
            string filePath2 = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"{ClientFileName}.txt");
            string promptFilePath2 = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Promt.txt");


            string file1Content = await File.ReadAllTextAsync(filePath1);
            string file2Content = await File.ReadAllTextAsync(filePath2);
            string prompt = await File.ReadAllTextAsync(promptFilePath2);

            // Собрать контекст для запроса
            string fullPrompt = $"Файл 1:\n{file1Content}\n\nФайл 2:\n{file2Content}\n\nЗадание:\n{prompt}";

            // Создать JSON-запрос
            var requestBody = new
            {
                model = "gpt-4.1", // или "gpt-5" при доступе
                messages = new object[]
                {
                new { role = "system", content = "Ты помощник, который анализирует файлы." },
                new { role = "user", content = fullPrompt }
                },
                temperature = 0.7
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            var response = await client.PostAsync(apiUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Ошибка: {response.StatusCode}\n{responseString}");
                return $"Ошибка: {response.StatusCode}\n{responseString}";
            }

            using var doc = JsonDocument.Parse(responseString);
            var message = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return message;
        }

    }
}
