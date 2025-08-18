using Microsoft.AspNetCore.Identity;
using NetTestMayapada.Models;

public static class AppDbInitializer
{
    public static async Task SeedAsync(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Administrator"))
            await roleManager.CreateAsync(new IdentityRole("Administrator"));

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole("User"));

        if (await userManager.FindByEmailAsync("admin@mail.com") == null)
        {
            var admin = new Users
            {
                UserName = "admin@mail.com",
                Email = "admin@mail.com",
                FullName = "Super Admin",
                Level = "Administrator",
                PhotoProfile = "/images/default-profile.png",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin1234");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Administrator");
            }
        }

        if (await userManager.FindByEmailAsync("user@mail.com") == null)
        {
            var normalUser = new Users
            {
                UserName = "user@mail.com",
                Email = "user@mail.com",
                FullName = "User Biasa",
                Level = "User",
                PhotoProfile = "/images/default-profile.png",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(normalUser, "User1234");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(normalUser, "User");
            }
        }
    }
}
