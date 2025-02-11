﻿using Microsoft.Extensions.Options;
using AgenticReportGenerationApi.Models;
using Microsoft.Azure.Cosmos;
using Azure.Identity;

namespace AgenticReportGenerationApi.Services
{
    public interface ICosmosDbService 
    {
        Task AddAsync(Company item);
        Task<Company> GetAsync(string id, string companyName);
        Task<Company?> GetCompanyByNameAsync(string companyName);
        Task<Company?> GetCompanyByIdAsync(string companyId);
        Task<List<string>> GetCompanyNamesAsync();
        Task<List<Company>> GetAllCompaniesAsync();
        Task<Dictionary<string, string>> GetCompanyIdAndNameAsync();
    }

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

        public async Task AddAsync(Company item)
        {
            var queryDefinition = new QueryDefinition("SELECT TOP 1 c.id FROM c");
            var queryRequestOptions = new QueryRequestOptions
            {
                // Only query this specific partition
                PartitionKey = new PartitionKey(item.company_name),
                MaxItemCount = 1
            };

            var iterator = _container.GetItemQueryIterator<dynamic>(queryDefinition, requestOptions: queryRequestOptions);

            if (iterator.HasMoreResults)
            {
                var firstBatch = await iterator.ReadNextAsync();
                if (firstBatch.Any())
                {
                    // That means at least one document is already in this partition
                    throw new InvalidOperationException(
                        $"A document already exists in the '{item.company_name}' partition. Duplicates are not allowed.");
                }
            }

            // If no items found in that partition, create a new one
            await _container.CreateItemAsync(item, new PartitionKey(item.company_name));
        }

        public async Task<Company?> GetAsync(string id, string companyName)
        {
            try
            {
                var response = await _container.ReadItemAsync<Company>(id, new PartitionKey(companyName));
                return response.Resource;
            }
            catch (CosmosException) //For handling item not found and other exceptions
            {
                return null;
            }
        }

        public async Task<Company?> GetCompanyByNameAsync(string companyName)
        {
            var queryDefinition = new QueryDefinition(
                "SELECT TOP 1 * FROM c WHERE c.CompanyName = @companyName"
            ).WithParameter("@companyName", companyName);

            var queryRequestOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(companyName),
                MaxItemCount = 1
            };

            using FeedIterator<Company> feedIterator = _container
                .GetItemQueryIterator<Company>(queryDefinition, requestOptions: queryRequestOptions);

            if (feedIterator.HasMoreResults)
            {
                FeedResponse<Company> response = await feedIterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return null;
        }

        public async Task<Company?> GetCompanyByIdAsync(string companyId)
        {
            var query = $"SELECT TOP 1 * FROM c WHERE c.CompanyId = '{@companyId}'";

            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<Company> feedIterator = _container.GetItemQueryIterator<Company>(queryDefinition);

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<Company> response = await feedIterator.ReadNextAsync();
                return response.FirstOrDefault();
            }

            return null;
        }

        public async Task<List<string>> GetCompanyNamesAsync()
        {
            var query = "SELECT DISTINCT VALUE c.CompanyName FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<string> feedIterator = _container.GetItemQueryIterator<string>(queryDefinition);
            List<string> uniqueCompanyNames = new List<string>();

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<string> currentResultSet = await feedIterator.ReadNextAsync();
                uniqueCompanyNames.AddRange(currentResultSet.Resource);
            }

            return uniqueCompanyNames;
        }

        public async Task<Dictionary<string, string>> GetCompanyIdAndNameAsync()
        {
            var query = "SELECT c.CompanyId, c.CompanyName FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<dynamic> feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
            Dictionary<string, string> companyIdNameDict = new Dictionary<string, string>();

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<dynamic> currentResultSet = await feedIterator.ReadNextAsync();
                foreach (var item in currentResultSet)
                {
                    var companyId = item.CompanyId.ToString();
                    var companyName = item.CompanyName.ToString();

                    if (!string.IsNullOrEmpty(companyId) && !string.IsNullOrEmpty(companyName))
                    {
                        companyIdNameDict.Add(companyId, companyName);
                    }
                }
            }

            return companyIdNameDict;
        }

        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            var query = "SELECT * FROM c";
            QueryDefinition queryDefinition = new QueryDefinition(query);
            FeedIterator<Company> feedIterator = _container.GetItemQueryIterator<Company>(queryDefinition);
            List<Company> companies = new List<Company>();

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<Company> currentResultSet = await feedIterator.ReadNextAsync();
                companies.AddRange(currentResultSet.Resource);
            }

            return companies;
        }
    }
}