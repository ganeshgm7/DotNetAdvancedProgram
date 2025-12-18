using IdentityService.API.Authorization;
using IdentityService.API.Configuration;
using IdentityService.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace IdentityService.API;

/// <summary>
/// Main program class for Identity Service API
/// </summary>
public class Program
{
    /// <summary>
    /// Application entry point
    /// </summary>
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Configure Azure AD Settings
        builder.Services.Configure<AzureAdSettings>(builder.Configuration.GetSection("AzureAd"));
        
        // Configure JWT Settings (still needed for service-to-service communication)
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

        // Add Microsoft Identity Web for Azure AD authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

        // Add Claims Transformation to enrich tokens with permission claims
        builder.Services.AddSingleton<IClaimsTransformation, PermissionClaimsTransformation>();

        builder.Services.AddAuthorization();

        // ? USE AZURE AD TOKEN SERVICE - Learning with real Azure AD
        builder.Services.AddScoped<ITokenService, AzureAdTokenService>();

        builder.Services.AddControllers();
        builder.Services.AddRouting(options => options.LowercaseUrls = true);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "Identity Service API - Azure Entra ID (Azure AD)",
                Version = "v1",
                Description = "Identity Management System with Azure Entra ID integration for learning and production use"
            });

            // Include XML comments if the XML documentation file is present
            string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (System.IO.File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        });

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1 - Azure AD");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "Identity Service API - Azure Entra ID Integration";
                c.DisplayRequestDuration();
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
