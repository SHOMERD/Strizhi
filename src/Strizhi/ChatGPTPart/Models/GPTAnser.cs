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
        public Parts(string ButtonTag, string Description, string BriefDescription)
        {
            this.ButtonTag = ButtonTag;
            this.Description = Description;
            this.BriefDescription = BriefDescription;
        }

        public Parts() { }

        public string ButtonTag { get; set; }
        public string Description { get; set; }
        public string BriefDescription { get; set; }

    }
}
