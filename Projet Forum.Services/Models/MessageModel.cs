using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.Models
{
    public class MessageModel
    {
        public string Username { get; set; }

        public int UserRole { get; set; }

        public string Message { get; set; }

        public DateTime Date { get; set; }
    }
}
