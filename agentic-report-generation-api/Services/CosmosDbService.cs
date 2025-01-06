using Microsoft.Extensions.Options;
using AgenticReportGenerationApi.Models;
using Microsoft.Azure.Cosmos;
using Azure.Identity;

namespace AgenticReportGenerationApi.Services
{
    public interface ICosmosDbService { }

    public class CosmosDbService : ICosmosDbService
    {
        private readonly Container _container;

        public CosmosDbService(IOptions<CosmosDbOptions> options)
        {
            CosmosClient cosmosClient = new(
               accountEndpoint: options.Value.AccountUri,
               tokenCredential: new DefaultAzureCredential(
                   new DefaultAzureCredentialOptions
                   {
                       TenantId = options.Value.TenantId,
                       ExcludeEnvironmentCredential = true
                   })
           );

           _container = cosmosClient.GetContainer(options.Value.DatabaseName, options.Value.ContainerName);
        }
    }
}