using LibraryAPI.API.Extensions;
using LibraryAPI.API.Middleware;
using LibraryAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Explicit appsettings path
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(
        "src/API/appsettings.json",
        optional: false,
        reloadOnChange: true)
    .AddEnvironmentVariables();

// ── Register services ────────────────────────────────────────
builder.Services
    .AddDatabase(builder.Configuration)
    .AddRepositories()
    .AddApplicationServices()
    .AddJwtAuthentication(builder.Configuration)
    .AddSwaggerWithJwt()
    .AddCorsPolicy()
    .AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

// ── Auto migrate database ────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

    await db.Database.MigrateAsync();

    // ── Ensure admin seed password is correct ────────────────
    var admin = await db.Users
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(u => u.Email == "admin@library.com");

    if (admin is not null)
    {
        const string defaultPassword = "Admin@123";
        if (!BCrypt.Net.BCrypt.Verify(defaultPassword, admin.PasswordHash))
        {
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword, workFactor: 12);
            db.Users.Update(admin);
            await db.SaveChangesAsync();
            Console.WriteLine("✅ Admin password hash fixed at startup.");
        }
    }
}

// ── Middleware ───────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<RequestLoggingMiddleware>();

// ENABLE SWAGGER ALWAYS
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Library API v1");

    c.RoutePrefix = "swagger";

    c.DocumentTitle = "Library Management API";

    c.DisplayRequestDuration();

    // Persistence script for Swagger UI (runs in default light mode)
    c.InjectJavascript("/swagger-persistence.js");
});

// Serve static files (HTML, CSS, JS, etc.)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        System.IO.Path.Combine(builder.Environment.ContentRootPath, "src", "API", "wwwroot"))
});

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
