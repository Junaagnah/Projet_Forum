using System.Threading.Tasks;
using Projet_Forum.Services.Models;
using Projet_Forum.Data.Models;
using Projet_Forum.Services.ViewModels;

namespace Projet_Forum.Services.Interfaces
{
    public interface ICategoryService
    {
        public MyResponse GetCategories(int role = 0);

        public Task<MyResponse> GetCategoriesAndPosts(bool isIndex = false, int role = 0);

        public Task<MyResponse> CreateCategory(CategoryViewModel category, int role);

        public MyResponse GetCategory(int id);

        public Task<MyResponse> UpdateCategory(CategoryViewModel category, int role);

        public Task<MyResponse> DeleteCategory(int id, int role);

        public MyResponse GetCategoriesWithPagination(int role, int page = 0);
    }
}
