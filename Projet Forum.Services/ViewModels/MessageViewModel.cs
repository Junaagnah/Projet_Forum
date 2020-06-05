using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.ViewModels
{
    public class MessageViewModel
    {
        public int Conversation { get; set; }

        public string Content { get; set; }

        public string ReceiverId { get; set; }
    }
}
