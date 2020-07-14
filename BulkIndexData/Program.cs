using Microsoft.Azure.ServiceBus;
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
using System.Text;
using Microsoft.Azure.ServiceBus.Core;
using System.Diagnostics;
using Microsoft.Azure.Amqp.Framing;

namespace BulkIndexingAgent
{
    // code comes from: https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues
    class Program
    {
        static IQueueClient queueClient;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {

            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            SearchServiceClient serviceClient = Search.CreateSearchServiceClient(configuration);

            string indexName = configuration["SearchIndexName"];
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);

            string ServiceBusConnectionString = configuration["ServiceBusConnectionString"];
            string QueueName = configuration["ServiceBusQueueName"];
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            // Receive the messages
            Console.WriteLine("Receiving messages...");

            MessageReceiver receiver = new MessageReceiver(ServiceBusConnectionString, QueueName, ReceiveMode.PeekLock);
            receiver.PrefetchCount = 1000;


            int x = 0;
            // The app wil close after 100 loops
            // To run in perpetuity, covert the condition to: while (true)
            while (x < 100)
            {
                await ReceiveMessagesAsync(receiver, indexClient);
                x++;
            }

            await receiver.CloseAsync();

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit.");
            Console.WriteLine("======================================================");

            Console.ReadKey();
        }

        static async Task ReceiveMessagesAsync(MessageReceiver receiver, ISearchIndexClient indexClient, int timeThreshold = 1000)
        {
            List<ContactInfo> messages = new List<ContactInfo>();

            // Start stopwatch
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var receivedMessages = await receiver.ReceiveAsync(maxMessageCount: 1000);
            while (receivedMessages != null)
            {
                // complete (roundtrips)
                var tokens = receivedMessages.Select(m => m.SystemProperties.LockToken);
                await receiver.CompleteAsync(tokens);

                foreach (var m in receivedMessages)
                {
                    ContactInfo doc = JsonConvert.DeserializeObject<ContactInfo>(Encoding.UTF8.GetString(m.Body));
                    messages.Add(doc);
                }

                if (stopWatch.ElapsedMilliseconds > timeThreshold)
                    break;

                receivedMessages = await receiver.ReceiveAsync(maxMessageCount: 1000);
            }

            if (messages.Count > 0)
            {
                if (messages.Count < 100)
                {
                    Console.WriteLine("======================================================");
                    Console.WriteLine($"Uploading batch with {messages.Count} documents");
                    Console.WriteLine("======================================================");
                    await Search.UploadDocuments(indexClient, messages);
                }
                else
                {
                    for (int i = 0; i < messages.Count; i += 1000)
                    {
                        var batchOfMessages = messages.GetRange(i, Math.Min(1000, messages.Count - i));

                        Console.WriteLine("======================================================");
                        Console.WriteLine($"Uploading batch with {batchOfMessages.Count} documents");
                        Console.WriteLine("======================================================");
                        await Search.UploadDocuments(indexClient, batchOfMessages);
                    }
                }

            }

            stopWatch.Stop();
        }
    }
}
