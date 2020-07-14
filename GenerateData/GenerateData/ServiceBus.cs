using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenerateData
{
    class ServiceBus
    {
        private string ServiceBusConnectionString;
        private string QueueName;
        static IQueueClient queueClient;

        public ServiceBus(IConfigurationRoot configuration)
        {
            ServiceBusConnectionString = configuration["ServiceBusConnectionString"];
            QueueName = configuration["ServiceBusQueueName"];
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);            
        }


        // Code from https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues
        public async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            DataGenerator dataGenerator = new DataGenerator();
            List<ContactInfo> documents = dataGenerator.GetDocuments(numberOfMessagesToSend);

            try
            {
                foreach (ContactInfo d in documents)
                {
                    // Create a new message to send to the queue.
                    string messageBody = JsonConvert.SerializeObject(d);
                    Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        public async Task SendMessageAsync(ContactInfo document)
        {
            try
            {
                // Create a new message to send to the queue.
                string messageBody = JsonConvert.SerializeObject(document);
                Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

                // Write the body of the message to the console.
                Console.WriteLine($"Sending message: {messageBody}");
                Console.WriteLine("");

                // Send the message to the queue.
                await queueClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        public void ControlIndexingSpeeds(int targetIPS = 5, int targetExecutionTime = 30)
        {
            var rnd = new Random();
            var dataGenerator = new DataGenerator();
            var lastBatchstartTime = DateTime.Now;

            int taskcount = 0;
            int batchCounter = 0;

            var taskList = new List<Task>();

            var data = dataGenerator.GetDocuments(targetIPS * targetExecutionTime);//.ToArray();

            foreach (ContactInfo c in data)
            {
                int tmpTaskCount = taskcount;
                taskList.Add(Task.Factory.StartNew(() => SendMessageAsync(c)));
                taskcount++;

                batchCounter++;
                if (batchCounter == targetIPS)
                {
                    Task.WaitAll(taskList.ToArray());
                    while (DateTime.Now.Subtract(lastBatchstartTime).TotalMilliseconds < 1000)
                    {
                        Thread.Sleep(100);
                    }
                    Console.WriteLine("Documents Submitted: {0}", taskcount);
                    lastBatchstartTime = DateTime.Now;

                    batchCounter = 0;
                }

            }

            Task.WaitAll(taskList.ToArray());
        }

        public async Task CloseClient()
        {
            await queueClient.CloseAsync();
        }
    }
}
