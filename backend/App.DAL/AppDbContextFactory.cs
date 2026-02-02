using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace App.DAL.EF;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        DotNetEnv.Env.Load("../.env");
        var connection = Environment.GetEnvironmentVariable("PG_DB_CONNECTION");
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connection ?? throw new InvalidOperationException("NO DB CONNECTION: Environment variable 'PG_DB_CONNECTION' is not set."));
        return new AppDbContext(optionsBuilder.Options);
    }
}