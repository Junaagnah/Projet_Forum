using Projet_Forum.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_Forum.Services.Models
{
    public class CategoryPaginationResponse
    {
        public List<Category> Categories { get; set; }

        public int Count { get; set; }
    }
}
