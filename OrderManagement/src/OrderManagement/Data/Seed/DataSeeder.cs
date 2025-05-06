using BuildingBlocks.EfCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Identities.Constants;

namespace OrderManagement.Data.Seed;

public class DataSeeder: IDataSeeder
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
                await _roleManager.CreateAsync(new IdentityRole { Name = IdentityConstant.Role.Admin });
            }

            if (await _roleManager.RoleExistsAsync(IdentityConstant.Role.User) == false)
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = IdentityConstant.Role.User });
            }
        }
    }

    private async Task SeedUsers()
    {
        if (!await _appDbContext.Users.AnyAsync())
        {
            if (await _userManager.FindByNameAsync("admin") == null)
            {
                var result = await _userManager.CreateAsync(InitialData.Users.First(), "admin@1234");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(InitialData.Users.First(), IdentityConstant.Role.Admin);
                }
            }

            if (await _userManager.FindByNameAsync("user") == null)
            {
                var result = await _userManager.CreateAsync(InitialData.Users.Last(), "user@1234");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(InitialData.Users.Last(), IdentityConstant.Role.User);
                }
            }
        }
    }
}
