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

    // Dark mode theme for Swagger UI
    c.InjectStylesheet("/swagger-ui-dark.css");
    c.InjectJavascript("/swagger-persistence.js");
});

// Serve static files (HTML, CSS, JS, etc.)
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
