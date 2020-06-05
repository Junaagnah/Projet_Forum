using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Projet_Forum.Data.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Conversation { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Sender { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;
    }
}
