using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strizhi.TelegramPart.logics
{
    public class TextCleaner()
    {
        public static async Task<string> CleanText(string text)
        {
            
            List<string> strings = text.Split("\n").ToList();
            List<string> DeliteFlags = new List<string>{"Телефон:", "СНИЛС:", "ИНН:", "Email:", "Автомобили:", "Паспорт:", "Документ:", "Дата выдачи паспорта:", "Код подразделения:", "Код КЛАДР:",
                "Связанные телефоны:", "Ссылка:", "ID", "id", "Кем выдан паспорт:", "Дата выдачи водительского удостоверения:", "Хеш пользователя", "ОГРН ИП:", "ОКВЭД:", "ОГРН юр. лица:",
                "Фотография:", "Домен VK:", "link:", "Полис ОМС:", "IP-адрес", "Маскированный номер карты:", "Номер заказа:", "Дата вылета:", "Логин:", "Пункт выдачи:" };

            for (int i = 0; i < DeliteFlags.Count(); i++)
            {
                strings.RemoveAll(a => a.Contains(DeliteFlags[i]));
            }
            for (int i = 0; i < strings.Count(); i++)
            {
                if( strings[i].Contains("Адрес") || strings[i].Contains("адрес") || strings[i].Contains("Водительское удостоверение"))
                {
                    strings[i] = new string(strings[i].Select(c => char.IsDigit(c) ? '*' : c).ToArray());
                }
            }
            strings.RemoveAll(a => string.IsNullOrEmpty(a));


            text = String.Join("\n", strings.ToArray());
            return text;

        }
        //Водительское удостоверение: 9910749195   !!!!!
        //Адрес:      
        //=== СДЭК 2022 ===  (повторения)
        //strings.RemoveAll(a => a.Contains("Юридический адрес работодателя:"));





    }
}
