using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WorkSpace.Infrastructure;

public class WorkSpaceContextFactory : IDesignTimeDbContextFactory<WorkSpaceContext>
{
    public WorkSpaceContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        var builder = new DbContextOptionsBuilder<WorkSpaceContext>();
        builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        return new WorkSpaceContext(builder.Options);
    }
}