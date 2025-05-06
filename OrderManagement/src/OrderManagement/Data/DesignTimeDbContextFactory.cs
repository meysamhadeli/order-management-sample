using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderManagement.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        builder.UseNpgsql( "Server=localhost;Port=5432;Database=order_management_db;User Id=postgres;Password=postgres;Include Error Detail=true");

        return new AppDbContext(builder.Options);
    }
}
