using BuildingBlocks.EfCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Customers.Models;
using OrderManagement.Identities.Constants;

namespace OrderManagement.Data.Seed;

public class DataSeeder : IDataSeeder
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _appDbContext;

    public DataSeeder(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext appDbContext
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _appDbContext = appDbContext;
    }

    public async Task SeedAllAsync()
    {
        var pendingMigrations = await _appDbContext.Database.GetPendingMigrationsAsync();

        if (!pendingMigrations.Any())
        {
            await SeedRoles();
            await SeedUsers();
        }
    }

    private async Task SeedRoles()
    {
        if (!await _appDbContext.Roles.AnyAsync())
        {
            if (await _roleManager.RoleExistsAsync(IdentityConstant.Role.Admin) == false)
            {
                await _roleManager.CreateAsync(
                    new IdentityRole {Name = IdentityConstant.Role.Admin});
            }

            if (await _roleManager.RoleExistsAsync(IdentityConstant.Role.User) == false)
            {
                await _roleManager.CreateAsync(
                    new IdentityRole {Name = IdentityConstant.Role.User});
            }
        }
    }

    private async Task SeedUsers()
    {
        if (!await _appDbContext.Users.AnyAsync())
        {
            if (await _userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = InitialData.Users.Find(x => x.UserName == "admin")!;

                var result = await _userManager.CreateAsync(
                                 adminUser,
                                 "admin@1234");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(
                        adminUser,
                        IdentityConstant.Role.Admin);
                }

                var customer = Customer.Create(
                    Guid.CreateVersion7(),
                    "Admin",
                    "Admin",
                    "admin@test.com",
                    InitialData.Users.First(),
                    100);

                _appDbContext.Customers.Add(customer);
                await _appDbContext.SaveChangesAsync();
            }

            if (await _userManager.FindByNameAsync("user") == null)
            {
                var user = InitialData.Users.Find(x => x.UserName == "user")!;

                var result = await _userManager.CreateAsync(user, "user@1234");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(
                        user,
                        IdentityConstant.Role.User);
                }
            }

            if (await _userManager.FindByNameAsync("test1") == null)
            {
                var test1 = InitialData.Users.Find(x => x.UserName == "test1")!;

                await _userManager.CreateAsync(test1, "test1@1234");
            }

            if (await _userManager.FindByNameAsync("test2") == null)
            {
                var test2 = InitialData.Users.Find(x => x.UserName == "test2")!;

                await _userManager.CreateAsync(test2, "test2@1234");
            }

            if (await _userManager.FindByNameAsync("test3") == null)
            {
                var test3 = InitialData.Users.Find(x => x.UserName == "test3")!;

                await _userManager.CreateAsync(test3, "test3@1234");
            }
        }
    }
}
