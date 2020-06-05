using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum.Data.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [Required]
        public string Path { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        [ForeignKey("Post")]
        public int? PostId { get; set; }
    }
}
