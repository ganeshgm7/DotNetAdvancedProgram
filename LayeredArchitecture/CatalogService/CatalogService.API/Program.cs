using CatalogService.API.Authorization;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Services;
using CatalogService.Domain.Interfaces;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Messaging;
using CatalogService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CatalogService.API;

/// <summary>
/// Main program class for Catalog Service API
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point
    /// </summary>
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(connectionString));

        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();

        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IProductService, ProductService>();

        builder.Services.Configure<ServiceBusPublisherSettings>(builder.Configuration.GetSection("ServiceBus"));
        builder.Services.AddSingleton<IProductEventPublisher, ServiceBusProductEventPublisher>();

        // Configure JWT Bearer authentication for local JWT tokens
        var jwtSecret = builder.Configuration["JwtSettings:Secret"];
        var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
        var jwtAudience = builder.Configuration["JwtSettings:Audience"];
    
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        // Add Claims Transformation to enrich tokens with permission claims based on roles
        builder.Services.AddSingleton<IClaimsTransformation, PermissionClaimsTransformation>();

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.CanRead, policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Read)));
            options.AddPolicy(Policies.CanCreate, policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Create)));
            options.AddPolicy(Policies.CanUpdate, policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Update)));
            options.AddPolicy(Policies.CanDelete, policy =>
                policy.Requirements.Add(new PermissionRequirement(Permissions.Delete)));
        });

        builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        builder.Services.AddControllers();
        builder.Services.AddRouting(options => options.LowercaseUrls = true);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "Catalog API v1", Version = "v1" });

            // Include XML comments if the XML documentation file is present
            string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (System.IO.File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        WebApplication app = builder.Build();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            CatalogDbContext db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            db.Database.Migrate();
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1");
        });

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
