using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Index = Microsoft.Azure.Search.Models.Index;

namespace GenerateData
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            ThreadPool.SetMaxThreads(Int32.MaxValue, Int32.MaxValue);
            ThreadPool.SetMinThreads(5000, 5000);

            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            SearchServiceClient serviceClient = CreateSearchServiceClient(configuration);

            string indexName = configuration["SearchIndexName"];
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);

            Console.WriteLine("{0}", "Deleting index...\n");
            await DeleteIndexIfExists(indexName, serviceClient);

            Console.WriteLine("{0}", "Creating index...\n");
            await CreateIndex(indexName, serviceClient);

            ServiceBus serviceBus = new ServiceBus(configuration);

            // Sends 100 requests per second for 100 seconds
            // This simulates 100 requests per second being sent to a Service Bus
            serviceBus.ControlIndexingSpeeds(100, 100);

            // To make this application generate data in perpetuity, you can uncomment the infinite loop below
            //while (true)
            //{
            //    serviceBus.ControlIndexingSpeeds(100, 30);
            //}

            await serviceBus.CloseClient();


            Console.WriteLine("{0}", "Complete.  Press any key to end application...\n");
            Console.ReadKey();
        }


        // Create the search service client
        private static SearchServiceClient CreateSearchServiceClient(IConfigurationRoot configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            return serviceClient;
        }

        // Delete an existing index to reuse its name
        private static async Task DeleteIndexIfExists(string indexName, SearchServiceClient serviceClient)
        {
            if (serviceClient.Indexes.Exists(indexName))
            {
                await serviceClient.Indexes.DeleteAsync(indexName);
            }
        }

        private static async Task CreateIndex(string indexName, SearchServiceClient searchService)
        {
            // Create a new search index structure that matches the properties of the Hotel class.
            // The Address class is referenced from the Hotel class. The FieldBuilder
            // will enumerate these to create a complex data structure for the index.
            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<ContactInfo>()
            };
            await searchService.Indexes.CreateAsync(definition);
        }

    }
}
