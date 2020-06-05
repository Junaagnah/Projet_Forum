using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Projet_Forum.Data.Interfaces;
using Projet_Forum.Data.Models;

namespace Projet_Forum.Data.Repositories
{
    /// <summary>
    /// Classe permettant de gérer les CRUD pour les catégories
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly MyDbContext _context;

        public CategoryRepository(MyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Permet de créer une catégorie
        /// </summary>
        /// <param name="category"></param>
        public async Task CreateCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Permet de supprimer une catégorie
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> DeleteCategory(int id)
        {
            var category = _context.Categories.FirstOrDefault(cat => cat.Id == id);
            if (category != null)
            {
                _context.Categories.Remove(category);

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de récupérer la liste des catégories
        /// </summary>
        /// <param name="role"></param>
        /// <returns>List<Category></returns>
        public List<Category> GetCategories(int role = 0)
        {
            return _context.Categories.Where(cat => cat.Role <= role).ToList();
        }

        /// <summary>
        /// Permet de récupérer une catégorie via son Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Category</returns>
        public Category GetCategory(int id)
        {
            return _context.Categories.FirstOrDefault(cat => cat.Id == id);
        }

        /// <summary>
        /// Permet de mettre à jour une catégorie
        /// </summary>
        /// <param name="category"></param>
        /// <returns>Boolean</returns>
        public async Task<bool> UpdateCategory(Category category)
        {
            var oldCat = _context.Categories.FirstOrDefault(cat => cat.Id == category.Id);
            if (oldCat != null)
            {
                oldCat.Name = category.Name;
                oldCat.Description = category.Description;
                oldCat.Role = category.Role;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Permet de récupérer la liste des catégories avec pagination
        /// </summary>
        /// <param name="page"></param>
        /// <returns>List<Category></returns>
        public List<Category> GetCategoriesWithPagination(int page)
        {
            int catPage = page * 10;

            var categories = _context.Categories.OrderBy(cat => cat.Role).Skip(catPage).Take(10).ToList();

            return categories;
        }

        /// <summary>
        /// Permet de récupérer le nombre de catégories total
        /// </summary>
        /// <returns>Integer</returns>
        public int GetCategoryCount()
        {
            int count = _context.Categories.Count();

            return count;
        }
    }
}
