using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace masodik.Models
{
    public class Home
    {
        public string username { get; set; }
        public string password { get; set; }
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }
        
    }
}
