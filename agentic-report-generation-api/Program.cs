
using AgenticReportGenerationApi.Models;
using AgenticReportGenerationApi.Services;
using Azure.Identity;

namespace AgenticReportGenerationApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Load configuration from appsettings.json and appsettings.local.json
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

            builder.Services.AddOptionsWithValidateOnStart<CosmosDbOptions>("CosmosDb")
            .BindConfiguration("CosmosDb")
            .ValidateDataAnnotations();

            // Register the concrete implementation of ICosmosDbService
            builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
