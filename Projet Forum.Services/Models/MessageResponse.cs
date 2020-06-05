using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.Models
{
    public class MessageResponse
    {
        public int Id { get; set; }

        public int Conversation { get; set; }

        public string Content { get; set; }

        public DateTime Date { get; set; }

        public string Sender { get; set; }

        public string Receiver { get; set; }

        public ProfileResponse SenderProfile { get; set; }
    }
}
