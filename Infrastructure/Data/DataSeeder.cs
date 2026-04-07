using Application.Interfaces.Repositories;
using Domain.Entities.Identity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context, IUserRepository userRepository, IRoleRepository roleRepository)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Admin Role
            if (!await roleRepository.ExistsAsync("Admin"))
            {
                var adminRole = AppRole.Create("Admin", "Quản trị viên - Toàn quyền hệ thống");
                await context.Roles.AddAsync(adminRole);
                await context.SaveChangesAsync();
            }

            // Seed Customer Role
            if (!await roleRepository.ExistsAsync("Customer"))
            {
                var customerRole = AppRole.Create("Customer", "Khách hàng");
                await context.Roles.AddAsync(customerRole);
                await context.SaveChangesAsync();
            }

            // Seed Technician Role
            if (!await roleRepository.ExistsAsync("Technician"))
            {
                var techRole = AppRole.Create("Technician", "Kỹ thuật viên");
                await context.Roles.AddAsync(techRole);
                await context.SaveChangesAsync();
            }

            // Seed Admin User
            if (!await userRepository.ExistsAsync("admin"))
            {
                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                
                // Hash password: admin123
                string passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                
                var adminUser = AppUser.Create(
                    "admin",
                    "admin@smarthome.com",
                    "Administrator",
                    passwordHash,
                    "bcrypt",
                    null
                );

                if (adminRole != null)
                {
                    adminUser.AssignRole(adminRole);
                }

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }
    }
}
