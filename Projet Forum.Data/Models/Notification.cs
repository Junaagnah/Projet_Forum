using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Projet_Forum.Data.Models
{
    public class Notification
    {
        // Enumeration des types de notifications
        public enum Type
        {
            Post,
            Message
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public Type Context { get; set; }

        // Id du post si de type post, id du message si de type message
        [Required]
        public int ContextId { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        // Utile si de type post
        public int CategoryId { get; set; } = 0;

        public bool Read { get; set; } = false;
    }
}
