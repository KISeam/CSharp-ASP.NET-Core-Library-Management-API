using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LibraryAPI.Infrastructure.Data;

public class LibraryDbContextFactory
    : IDesignTimeDbContextFactory<LibraryDbContext>
{
    public LibraryDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        IConfigurationRoot configuration =
            new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(
                    "src/API/appsettings.json",
                    optional: false,
                    reloadOnChange: true)
                .Build();

        var connectionString =
            configuration.GetConnectionString("Default")
            ?? "Data Source=library.db";

        var optionsBuilder =
            new DbContextOptionsBuilder<LibraryDbContext>();

        optionsBuilder.UseSqlite(connectionString);

        return new LibraryDbContext(
            optionsBuilder.Options);
    }
}
