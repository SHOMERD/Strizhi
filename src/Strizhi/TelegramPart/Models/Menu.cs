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

        public List<string> Files { get; set; }

        public List<string> ButtonsTexts { get; set; }
        public List<string> ButtonsTegs { get; set; }

        public List<string> ChildrenMenuTegs { get; set; }



    }
}
