using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.TeamsDashboard
{
    public interface IEventHubListener
    {
        Task ListenEvents();
    }
    public class EventHubListener : IEventHubListener
    {
        private readonly EventProcessorClient _processor;
        private readonly BlobContainerClient _storageClient;
        private readonly ITeamsDataCollectorManager _teamsDataCollectorManager;
        private readonly ILogger<TeamsDataCollectorManager> _logger;
        private readonly UniSettings _uniSettings;
        private readonly IConfiguration _configuration;

        public EventHubListener(IOptions<UniSettings> uniCfg
            , ILogger<TeamsDataCollectorManager> logger
            , IConfiguration configuration
            , ITeamsDataCollectorManager teamsDataCollectorManager)
        {
            _uniSettings = uniCfg.Value;
            _logger = logger;
            _configuration = configuration;
            _teamsDataCollectorManager = teamsDataCollectorManager;
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            // Create a blob container client that the event processor will use 
            _storageClient = new BlobContainerClient(configuration.GetConnectionString("BlobStorage"), _uniSettings.StorageEventContainer);

            // Create an event processor client to process events in the event hub
            _processor = new EventProcessorClient(_storageClient, consumerGroup, configuration.GetConnectionString("EventHubNamespace"), _uniSettings.EventHubName);
        }
        public async Task ListenEvents()
        {
            // Read from the default consumer group: $Default


            // Register handlers for processing events and handling errors
            _processor.ProcessEventAsync += ProcessEventHandler;
            _processor.ProcessErrorAsync += ProcessErrorHandler;

            // Start the processing
            await _processor.StartProcessingAsync();

            while (_processor.IsRunning)
            {
                // Wait for 30 seconds for the events to be processed
                await Task.Delay(TimeSpan.FromSeconds(10));
            }


            // Stop the processing
            await _processor.StopProcessingAsync();
        }


        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                Console.WriteLine("\tReceived event: {0}", Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));
                var res = await _teamsDataCollectorManager.ProcessNotification(eventArgs.Data.EventBody.ToStream());
            }
            finally
            {
                semaphoreSlim.Release();
            }

            // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            // Write details about the error to the console window
            Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(eventArgs.Exception.Message);
            return Task.CompletedTask;
        }
    }
}
