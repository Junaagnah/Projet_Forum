using Microsoft.AspNetCore.Identity;
using Projet_Forum.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projet_Forum
{
    internal static class Initializer
    {
        internal static async Task SeedAsync(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("admin"))
                await roleManager.CreateAsync(new IdentityRole("admin"));

            if (!await roleManager.RoleExistsAsync("moderator"))
                await roleManager.CreateAsync(new IdentityRole("moderator"));

            if (!await roleManager.RoleExistsAsync("standard"))
                await roleManager.CreateAsync(new IdentityRole("standard"));

            var admin = await userManager.FindByNameAsync("Administrator");
            if (admin == null)
            {
                admin = new User {
                    UserName = "Administrator",
                    Email = "coladai18-gni@ccicampus.fr",
                    Role = 3,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "adminGnI2020!");
                await userManager.AddToRoleAsync(admin, "admin");
            }
        }
    }
}
