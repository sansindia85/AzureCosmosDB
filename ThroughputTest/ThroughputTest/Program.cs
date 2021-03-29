using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace ThroughputTest
{
    class Program
    {
        static async Task Main()
        {
            await Run();
        }

        private static async Task Run()
        {
            const string endpoint =
                @"AccountEndpoint=https://comsos-course.documents.azure.com:443/;AccountKey=hidwjEgOw8v7YXm1Q2IlYH13TqXADxVZ5shxt2Z1go3kqmOlZmHfZOgEmYJvyYK4z48zAi3vU9WHrRuZwwHsrg==;";
            
            var client = new CosmosClient(endpoint);
            var container = client.GetContainer("Families", "Families");

            dynamic document = new
            {
                id = Guid.NewGuid(),
                familyName = "Sandeep",
                address = new
                {
                    addressLine = "Balagere",
                    city = "Bengaluru",
                    state = "KA",
                    zipCode = "560087"
                },
                parents = new[]
                {
                    "Somaraju",
                    "Rajeswari"
                },
                kids = new[]
                {
                    "Srinesh"
                }
            };

            try
            {
                //.Net SDK will retry multiple times before throwing exception
                var result = await container.CreateItemAsync(document, new PartitionKey(document.address.zipCode));
                var consumedRUs = result.RequestCharge;
                Console.WriteLine($"Cost to create document: {consumedRUs} RUs");
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests) //429
            {
                Console.WriteLine("Can't create document; request was throttled.");
            }
        }
    }
}
