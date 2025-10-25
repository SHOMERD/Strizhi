using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strizhi.TelegramPart.Models
{
    public class UserFile
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Offer { get; set; }
        public long UserID { get; set; }
        public string FileName { get; set; }

        public string СlientName { get; set; }



    }
}
