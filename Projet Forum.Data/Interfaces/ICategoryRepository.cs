using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Data.Interfaces
{
    public interface ICategoryRepository
    {
        public List<Category> GetCategories(int role = 0);

        public Category GetCategory(int id);

        public Task CreateCategory(Category category);

        public Task<bool> UpdateCategory(Category category);

        public Task<bool> DeleteCategory(int id);

        public List<Category> GetCategoriesWithPagination(int page);

        public int GetCategoryCount();
    }
}
