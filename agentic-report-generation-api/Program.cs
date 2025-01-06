using AgenticReportGenerationApi.Models;
using AgenticReportGenerationApi.Services;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using AgenticReportGenerationApi.Prompts;
using EntertainmentChatApi.Services;

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

            builder.Services.AddOptions<CosmosDbOptions>()
            .Bind(builder.Configuration.GetSection(CosmosDbOptions.CosmosDb))
            .ValidateDataAnnotations();

            builder.Services.AddOptions<AzureOpenAiOptions>()
            .Bind(builder.Configuration.GetSection(AzureOpenAiOptions.AzureOpenAI))
            .ValidateDataAnnotations();

            // Build the service provider
            var serviceProvider = builder.Services.BuildServiceProvider();

            // Access the options instance
            var kernelOptions = serviceProvider.GetRequiredService<IOptions<AzureOpenAiOptions>>().Value;

            builder.Services.AddTransient<Kernel>(s =>
            {
                var builder = Kernel.CreateBuilder();
                builder.AddAzureOpenAIChatCompletion(kernelOptions.DeploymentName, kernelOptions.EndPoint, kernelOptions.ApiKey);
                return builder.Build();
            });

            builder.Services.AddSingleton<IChatCompletionService>(sp =>
                     sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());

            builder.Services.AddSingleton<IChatHistoryManager>(sp =>
            {
                var sysPrompt = CorePrompts.GetSystemPrompt();
                return new ChatHistoryManager(sysPrompt);
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