
using CartService.API.BusinessLogic;
using CartService.API.BusinessLogic.Interfaces;
using CartService.API.DataAccess;
using System.Reflection;

namespace CartService.API;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<ICartRepository>(sp => new CartRepository("cart.db"));
        builder.Services.AddScoped<CartManager>();

        builder.Services.AddControllers();

        // Add API versioning
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "Cart API v1", Version = "v1" });
            options.SwaggerDoc("v2", new() { Title = "Cart API v2", Version = "v2" });

            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddRouting(options => options.LowercaseUrls = true);

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart API v1");
                options.SwaggerEndpoint("/swagger/v2/swagger.json", "Cart API v2");
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
