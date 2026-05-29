using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using LibraryAPI.Application.Services;
using LibraryAPI.Domain.Interfaces.Repositories;
using LibraryAPI.Domain.Interfaces.Services;
using LibraryAPI.Infrastructure.Data;
using LibraryAPI.Infrastructure.Repositories;
using LibraryAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Collections.Generic;

namespace LibraryAPI.API.Extensions;

public static class ServiceExtensions
{
    // ── Database ─────────────────────────────────────────────
    public static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<LibraryDbContext>(opts =>
            opts.UseSqlite(
                config.GetConnectionString("Default") ?? "Data Source=library.db",
                b => b.MigrationsAssembly("LibraryAPI")));
        return services;
    }

    // ── Repository + Unit of Work ─────────────────────────────
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    // ── Application services ──────────────────────────────────
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtService,     JwtService>();
        services.AddScoped<IAuthService,    AuthService>();
        services.AddScoped<IBookService,    BookService>();
        services.AddScoped<IAuthorService,  AuthorService>();
        services.AddScoped<IMemberService,  MemberService>();
        services.AddScoped<IBorrowService,  BorrowService>();
        return services;
    }

    // ── JWT Authentication ────────────────────────────────────
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration config)
    {
        var secret = config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                                                  Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer           = true,
                    ValidIssuer              = config["Jwt:Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = config["Jwt:Audience"],
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero   // no grace period
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        if (ctx.Exception is SecurityTokenExpiredException)
                            ctx.Response.Headers.Append("Token-Expired", "true");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly",      p => p.RequireRole("Admin"))
            .AddPolicy("StaffOnly",      p => p.RequireRole("Admin", "Librarian"))
            .AddPolicy("Authenticated",  p => p.RequireAuthenticatedUser());

        return services;
    }

    // ── Swagger with JWT support ──────────────────────────────
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "Library Management API",
                Version     = "v1",
                Description = "Role-based library system: Admin | Librarian | Member",
                Contact     = new OpenApiContact { Name = "Library Dev Team" }
            });

            // Add the "Authorize" button to Swagger UI
            var jwtScheme = new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "Paste your JWT: Bearer {token}",
                Reference    = new OpenApiReference
                {
                    Id   = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);
            
            // Only add security requirement to endpoints that require authorization
            c.OperationFilter<AuthorizeCheckOperationFilter>();
        });

        return services;
    }

    // ── CORS ──────────────────────────────────────────────────
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(opts =>
            opts.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        return services;
    }
}

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if there is an [Authorize] attribute on the controller or action method
        var hasAuthorize = (context.MethodInfo.DeclaringType != null &&
                            context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any())
                           || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        if (hasAuthorize)
        {
            // Also check if there is an [AllowAnonymous] attribute on the action method
            var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

            if (!hasAllowAnonymous)
            {
                operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

                var jwtBearerScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [ jwtBearerScheme ] = Array.Empty<string>()
                    }
                };
            }
        }
    }
}
