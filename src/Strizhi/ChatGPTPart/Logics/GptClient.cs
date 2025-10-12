using Microsoft.EntityFrameworkCore.Metadata;
using Strizhi.TelegramPart.logics;
using Strizhi.TelegramPart.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Strizhi.ChatGPTPart.Models;
using Newtonsoft.Json;



namespace ChatGptVisionClient
{
    public class GptClient
    {
        private readonly HttpClient _http;
        private string ApiKey;
        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        private string offersFile { get; set; }

        private List<Promt> prompts {get; set; }


        public MessageСonstructor messageСonstructor { get; set; }



        public GptClient(MessageСonstructor messageСonstructor)
        {
            this.messageСonstructor = messageСonstructor;            
            _http = new HttpClient();
            UpdateData();

            if (string.IsNullOrEmpty(ApiKey))
            {
                messageСonstructor.ErrorMessage("Set OPENAI_API_KEY environment variable first.");
            }
        }



        public async Task<Answer> RequestToAI(int RequestTag, string Client = null)
        {
            string Task = "";
            for (int i = 0; i < prompts[RequestTag].Files.Count; i++)
            {
                Task += $"Файл {i+1}:\n[{ReadFile(prompts[RequestTag].Files[i], Client)}]\n";
            }
            Task += $"\nЗадание:\n{prompts[RequestTag].PromtText}";

            if (true)
            {
                Answer answer = new Answer();
                answer.Text = "Заглушка";
                answer.Parts = new List<Parts>() { new Parts("ButtonTag", "Description"), new Parts("Tag", "Ddaad") };
                return answer;
            }

            return await SengToGPT(Task);
        }



        public async Task<Answer> SengToGPT(string Prompt)
        {
            var requestBody = SetRequestBody(Prompt);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            var response = await client.PostAsync(apiUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                messageСonstructor.ErrorMessage($"Ошибка: {response.StatusCode}\n{responseString}");
                return null;
            }

            using var doc = JsonDocument.Parse(responseString);
            var message = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return JsonConvert.DeserializeObject<Answer>(message);
        }

        private object SetRequestBody(string Prompt)
        {
            var schema = new
            {
                name = "anser",
                strict = true,
                json_schema = new
                {
                    name = "anser",
                    strict = true,
                    schema = new
                    {
                        
                        type = "object",
                        properties = new
                        {
                            text = new { type = "string" },
                            parts = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        ButtonTag = new { type = "string" },
                                        Description = new { type = "string" },                                     
                                    },
                                    required = new[] { "ButtonTag", "Description"},
                                    additionalProperties = false
                                }
                            }
                        },
                        required = new[] { "text" },
                        additionalProperties = false
                    }
                }
            };


            return new
            {
                model = "gpt-5-nano", // или "gpt-5" при доступе
                messages = new object[]
                {
                new { role = "system", content = "You are an assistant who analyzes files/websites. " +
                                                "You must return JSON strictly according to the specified schema. " +
                                                "The response will be used by a person who speaks Russian. " },
                new { role = "user", content = Prompt }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = schema
                }


            };

        }


        public async Task<string> ReadFile(string FileName, string Client = null)
        {
            string FilePath = "";
            if (FileName == "ClienInfo")
            {
                FilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"TGBOT\\Clients\\{Client}.txt");
            }
            else
            {
                FilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"TGBOT\\{FileName}");
            }
            
            return await File.ReadAllTextAsync(FilePath);
        }



        public async Task UpdateData(string FileName = null)
        {
            if (FileName == null)
            {
                string offersFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\offers.txt");
                string promptFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\Promt.json");
                string GPTTokenPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TGBOT\\GPTToken.txt");

                ApiKey = await File.ReadAllTextAsync(GPTTokenPath); ;
                offersFile = await File.ReadAllTextAsync(offersFilePath);
                prompts = JsonConvert.DeserializeObject<List<Promt>>(await File.ReadAllTextAsync(promptFilePath));

            }
            else {
                string FilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), $"TGBOT\\{FileName}");
                switch (FileName)
                {
                    case "offers.txt":
                        offersFile = await File.ReadAllTextAsync(FilePath);
                        break;
                    case "Promt.json":
                        prompts = JsonConvert.DeserializeObject<List<Promt>>(await File.ReadAllTextAsync(FilePath));                        
                        break;
                    case "GPTToken.txt":
                        ApiKey = await File.ReadAllTextAsync(FilePath);
                        break;
                    default:
                        break;
                }

            }

        }
    }
}
