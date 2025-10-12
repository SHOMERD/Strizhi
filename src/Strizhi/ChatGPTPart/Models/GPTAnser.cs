using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strizhi.ChatGPTPart.Models
{
    public class Answer
    {
        public string Text { get; set; }
        public List<Parts> Parts { get; set; }
    }

    public class Parts
    {
        public Parts(string ButtonTag, string Description)
        {
            this.ButtonTag = ButtonTag;
            this.Description = Description;
        }

        public Parts() { }

        public string ButtonTag { get; set; }
        public string Description { get; set; }
    }
}
