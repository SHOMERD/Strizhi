using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Strizhi.TelegramPart.Models
{
    public class TheUser
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public long UserID { get; set; }
        public string Username { get; set; }

        public string? PhoneNamber { get; set; }

    }
}
