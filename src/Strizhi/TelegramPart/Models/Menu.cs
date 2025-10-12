using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strizhi.TelegramPart.Models
{
    public class Menu
    {
        public string Teg { get; set; }

        public string ParentTeg { get; set; }

        public string MessageText { get; set; }

        public int PromtNumber { get; set; }

        public List<string> ButtonsTexts { get; set; }
        public List<string> ButtonsTegs { get; set; }

        public Menu()
        {
            Teg = "";
            ParentTeg = "";
            MessageText = "";
            PromtNumber = -1;
            ButtonsTexts = new List<string>();
            ButtonsTegs = new List<string>();
        }
    }
}
