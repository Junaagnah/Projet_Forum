using Projet_Forum.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.Models
{
    public class PostPaginationResponse
    {
        public int Count { get; set; }

        public List<Post> Posts { get; set; }
    }
}
