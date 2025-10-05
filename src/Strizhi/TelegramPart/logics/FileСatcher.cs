using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                using (FileStream file = File.Create(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName +".txt")))
                    file.Write(data, 0, data.Length);
            }
            return true;

        }
        

        public static async Task DeliteFile(string fileName)
        {
            
        }


    }
}
