using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections.Generic;

namespace Company.Function
{
    public class TestCsharp
    {
        private IConfiguration _config;

        public TestCsharp(IConfiguration config)
        {
            _config = config;
        }

        [FunctionName("TestCsharp")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var partitionKey = new PartitionKey("c#");
            var cosmosClient = new CosmosClient(_config.GetConnectionString("cosmos_connection"));
            await cosmosClient
                .GetContainer("Test", "TestContainer")
                .UpsertItemAsync(new TestItem
                {
                    Id = "testId",
                    SomeProperty = 2,
                    Source = "c#"
                }, partitionKey);

            List<TestItem> resultIterator = new List<TestItem>();
            FeedIterator<TestItem> iterator = cosmosClient
                .GetContainer("Test", "TestContainer")
                .GetItemQueryIterator<TestItem>("SELECT * FROM c WHERE c.id =\"testId\"", requestOptions: new QueryRequestOptions
                {
                    PartitionKey = partitionKey
                });

            while (iterator.HasMoreResults)
            {
                FeedResponse<TestItem> response = await iterator.ReadNextAsync();
                resultIterator.AddRange(response.ToList());
            }

            List<TestItem> resultLinq = cosmosClient
                .GetContainer("Test", "TestContainer")
                .GetItemLinqQueryable<TestItem>(allowSynchronousQueryExecution: true, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = partitionKey
                })
                .Where(x => x.Id == "testId")
                .ToList();

            return new OkObjectResult(resultIterator);
        }
    }
}
