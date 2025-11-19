using ChatGptVisionClient;
using Newtonsoft.Json;
using Strizhi.ChatGPTPart.Models;
using Strizhi.TelegramPart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Strizhi.TelegramPart.logics
{
    public class FilePreparer
    {
        public static async Task PrepareFile(string FileName, DataBase dataBase, GptClient gptClient) { 

            UserFile userFile = await dataBase.GetFileAsync(FileName);
            await dataBase.SetGPTAnsers(FileName,
                MainAnalysis:    JsonConvert.SerializeObject(await gptClient.RequestToAI(0, FileName)),
                MainOffer:       JsonConvert.SerializeObject(await gptClient.RequestToAI(1, FileName)),
                MoreOffer:       JsonConvert.SerializeObject(await gptClient.RequestToAI(2, FileName)),
                Apartments:      JsonConvert.SerializeObject(await gptClient.RequestToAI(3, FileName)),
                Competitors:     JsonConvert.SerializeObject(await gptClient.RequestToAI(4, FileName))
                );

            
        }
    }


    
}
