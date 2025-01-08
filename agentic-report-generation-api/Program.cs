using AgenticReportGenerationApi.Models;
using AgenticReportGenerationApi.Services;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using AgenticReportGenerationApi.Prompts;
using AgenticReportGenerationApi.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Asp.Versioning;

namespace AgenticReportGenerationApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            // Load configuration from appsettings.json and appsettings.local.json
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version"));
            })
            .AddMvc() // This is needed for controllers
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddOptions<CosmosDbOptions>()
            .Bind(builder.Configuration.GetSection(CosmosDbOptions.CosmosDb))
            .ValidateDataAnnotations();

            builder.Services.AddOptions<AzureOpenAiOptions>()
            .Bind(builder.Configuration.GetSection(AzureOpenAiOptions.AzureOpenAI))
            .ValidateDataAnnotations();

            builder.Services.AddMemoryCache();

            // Build the service provider
            var serviceProvider = builder.Services.BuildServiceProvider();

            // Access the options instance
            var kernelOptions = serviceProvider.GetRequiredService<IOptions<AzureOpenAiOptions>>().Value;

            builder.Services.AddTransient<Kernel>(s =>
            {
                var builder = Kernel.CreateBuilder();
                builder.AddAzureOpenAIChatCompletion(kernelOptions.DeploymentName, kernelOptions.EndPoint, kernelOptions.ApiKey);
                var memoryCache = s.GetRequiredService<IMemoryCache>();
                var logger = s.GetRequiredService<ILogger<ReportGenerationPlugin>>();
                var reportGenerationPlugin = new ReportGenerationPlugin(memoryCache, logger);
                builder.Plugins.AddFromObject(reportGenerationPlugin, "GenerateReport");
                return builder.Build();
            });

            builder.Services.AddSingleton<IChatCompletionService>(sp =>
                     sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

            builder.Services.AddSingleton<IChatHistoryManager>(sp =>
            {
                var sysPrompt = CorePrompts.GetSystemPrompt();
                return new ChatHistoryManager(sysPrompt);
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}