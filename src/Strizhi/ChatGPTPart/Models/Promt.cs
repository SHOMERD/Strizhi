using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strizhi.ChatGPTPart.Models
{
    public class Promt
    {
        public int Namber { get; set; }
        public string PromtText { get; set; }

        public List<string> Files { get; set; }
        public List<string> Hashtags { get; set; }

    }
}
