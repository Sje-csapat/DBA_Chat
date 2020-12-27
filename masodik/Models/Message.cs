using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace masodik.Models
{
    public class Message
    {
        public int id { get; set; }
        public string message { get; set; }
        public bool own_message { get; set; }
        public int created_at { get; set; }


        public Message(string message, int created_at, bool own_message = true)
        {
            this.message = message;
            this.own_message = own_message;
            this.created_at = created_at;
        }
    }
}
