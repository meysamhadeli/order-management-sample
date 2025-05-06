using Microsoft.AspNetCore.Identity;

namespace OrderManagement.Data.Seed;

public static class InitialData
{
    public static List<IdentityUser> Users { get; }

    static InitialData()
    {
        Users = new List<IdentityUser>
                {
                    new()
                    {
                        Id = Guid.CreateVersion7().ToString(),
                        UserName = "admin",
                        Email = "admin@test.com",
                        SecurityStamp = Guid.NewGuid().ToString(),
                        EmailConfirmed = true
                    },
                    new()
                    {
                        Id = Guid.CreateVersion7().ToString(),
                        UserName = "user",
                        Email = "user@test.com",
                        SecurityStamp = Guid.NewGuid().ToString(),
                        EmailConfirmed = true
                    },
                };
    }
}
