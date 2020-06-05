using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.Models
{
    public class ConversationResponse
    {
        public int Id { get; set; }

        public string ContactUsername { get; set; }

        public DateTime LastMessageDate { get; set; }
    }
}
