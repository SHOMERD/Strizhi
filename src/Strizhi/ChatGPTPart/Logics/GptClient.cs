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
        // Возьмите ключ из окружения: setx OPENAI_API_KEY "sk-..."
        private string ApiKey;
        private string c;

        public GptClient(string c)
        {
            _http = new HttpClient();
            this.ApiKey = c;
        }

        public async Task SengToGPT(string Namber)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                Console.WriteLine("Set OPENAI_API_KEY environment variable first.");
                return;
            }
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            // Пример: пути к файлам, которые нужно отправить
            var filesToUpload = new[] { "doc1.pdf", "notes.txt" };

            // 1) Загрузить файлы и получить file_id'ы
            var fileIds = new List<string>();
            foreach (var path in filesToUpload)
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"File not found: {path}");
                    continue;
                }

                var fileId = await UploadFileAsync(path);
                Console.WriteLine($"Uploaded {path} -> {fileId}");
                fileIds.Add(fileId);
            }

            if (fileIds.Count == 0)
            {
                Console.WriteLine("No files uploaded, exiting.");
                return;
            }

            // 2) Вызвать Responses API, передав file_id'ы как input_file
            var question = "Суммируй ключевые идеи из этих файлов и дай 3 главных вывода.";
            var responseText = await SendResponseWithFilesAsync(fileIds, question);

            Console.WriteLine("Model response:");
            Console.WriteLine(responseText);
        }

        // Загружает один файл к /v1/files и возвращает file.id
        private async Task<string> UploadFileAsync(string filePath)
        {
            using var content = new MultipartFormDataContent();

            // purpose=user_data обычно для файлов, которые вы хотите, чтобы модель использовала в ответах
            content.Add(new StringContent("user_data"), "purpose");

            var fileStream = File.OpenRead(filePath);
            var streamContent = new StreamContent(fileStream);
            // Поправьте MIME при желании, не обязательно
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "file", Path.GetFileName(filePath));

            var resp = await this._http.PostAsync("https://api.openai.com/v1/files", content);
            var txt = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Upload failed: {resp.StatusCode} {txt}");

            using var doc = JsonDocument.Parse(txt);
            // формат ответа: { "id":"file-xxx", ... }
            var id = doc.RootElement.GetProperty("id").GetString();
            return id!;
        }

        // Вызывает /v1/responses и передаёт в input массив, где каждый файл — {type:"input_file", file_id: "..."}
        private async Task<string> SendResponseWithFilesAsync(IEnumerable<string> fileIds, string userQuestion)
        {
            // Соберём content array, где сначала будут объекты input_file, затем input_text
            var fileInputs = new List<object>();
            foreach (var fid in fileIds)
            {
                fileInputs.Add(new Dictionary<string, object>
                {
                    ["type"] = "input_file",
                    ["file_id"] = fid
                });
            }

            // Затем дополним запрос текстовым вопросом
            fileInputs.Add(new Dictionary<string, object>
            {
                ["type"] = "input_text",
                ["text"] = userQuestion
            });

            // Полное тело запроса в формате Responses API
            var body = new
            {
                model = "gpt-4.1", // или другой поддерживаемый моделью Responses API
                input = new[]
                {
                new
                {
                    role = "user",
                    content = fileInputs
                }
            }
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var resp = await _http.PostAsync("https://api.openai.com/v1/responses",
                new StringContent(json, Encoding.UTF8, "application/json"));

            var txt = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Responses API failed: {resp.StatusCode} {txt}");

            // Парсим ответ (в simplest form берем output_text если есть)
            using var doc = JsonDocument.Parse(txt);
            if (doc.RootElement.TryGetProperty("output_text", out var outText))
            {
                return outText.GetString() ?? "";
            }

            // Альтернатива: извлечь текст из choices/output/annotations в зависимости от формата
            return txt;
        }
    }
}
