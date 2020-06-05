using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum.Data.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Post { get; set; }

        [Required]
        public string Body { get; set; }
        [Required]
        [ForeignKey("User")]
        public string Author { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
