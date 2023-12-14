using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using System.Text;
using System.Text.Json;

// EventSchema
public class EventSchema
{
    public class TradeEvent
    {
        public string EventType { get; set; } = "TradeExecuted";
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTimeOffset EventTimestamp { get; set; } = DateTimeOffset.UtcNow;
        public string Version { get; set; } = "1.0";

        public TradeDetails Details { get; set; }

        // information for additional context
        public TradeEventMetadata Metadata { get; set; }

        public TradeEvent(TradeDetails details, TradeEventMetadata metadata)
        {
            Details = details;
            Metadata = metadata;
        }
    }

    public class TradeDetails
    {
        public Guid TradeId { get; set; }
        public string StockTicker { get; set; }
        public decimal TradePrice { get; set; }
        public decimal NumberOfShares { get; set; }
        public DateTimeOffset TradeTime { get; set; }
        public string BrokerId { get; set; }

        public TradeDetails(Guid tradeId, string stockTicker, decimal tradePrice, decimal numberOfShares, DateTimeOffset tradeTime, string brokerId)
        {
            TradeId = tradeId;
            StockTicker = stockTicker;
            TradePrice = tradePrice;
            NumberOfShares = numberOfShares;
            TradeTime = tradeTime;
            BrokerId = brokerId;
        }
    }

    public class TradeEventMetadata
    {
        // for tracking the events PartitionKey for a specific partition in the EventHub
        public string CorrelationId { get; set; }
        public string PartitionKey { get; set; }

        public TradeEventMetadata(string correlationId, string partitionKey)
        {
            CorrelationId = correlationId;
            PartitionKey = partitionKey;
        }
    }
}

public class EventPubSub
{
    // creating and publishing trade events to the EventHub
    public class TradeService
    {
        private readonly string _connectionString;
        private readonly string _eventHubName;

        public TradeService(string connectionString, string eventHubName)
        {
            _connectionString = connectionString;
            _eventHubName = eventHubName;
        }

        // Method to create and publish a trade event.
        public async Task CreateTradeAsync(string stockTicker, decimal tradePrice, decimal numberOfShares, string brokerId,  string correlationId)
        {
            var tradeId = Guid.NewGuid();
            var timeOfTrade = DateTime.UtcNow;

            var tradeDetails = new EventSchema.TradeDetails(tradeId, stockTicker, tradePrice, numberOfShares, timeOfTrade, brokerId);
            var tradeMetadata = new EventSchema.TradeEventMetadata(correlationId, stockTicker);
            var tradeEvent = new EventSchema.TradeEvent(tradeDetails, tradeMetadata);

            await PublishEventAsync(tradeEvent);
        }

        private async Task PublishEventAsync(EventSchema.TradeEvent tradeEvent)
        {
            await using var producerClient = new EventHubProducerClient(_connectionString, _eventHubName);
            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

            // serliaze
            var eventData = new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tradeEvent)));

            // event size check - should be fine for this simple class
            if (!eventBatch.TryAdd(eventData))
            {
                throw new Exception("Event is too large for the batch and cannot be sent.");
            }

            await producerClient.SendAsync(eventBatch);
        }
    }

    public class TradeEventListener
    {
        private readonly string _connectionString;
        private readonly string _eventHubName;
        private readonly string _blobStorageConnectionString;
        private readonly string _blobContainerName;

        public TradeEventListener(string connectionString, string eventHubName, string blobStorageConnectionString, string blobContainerName)
        {
            _connectionString = connectionString;
            _eventHubName = eventHubName;
            _blobStorageConnectionString = blobStorageConnectionString;
            _blobContainerName = blobContainerName;

            // listening for events
            StartListening();
        }

        // start listening for events
        private void StartListening()
        {
            var processor = new EventProcessorClient(
                new BlobContainerClient(_blobStorageConnectionString, _blobContainerName),
                EventHubConsumerClient.DefaultConsumerGroupName,
                _connectionString,
                _eventHubName);

            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;

            processor.StartProcessingAsync();
        }

        private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            // Deserialize the event data into a TradeEvent object.
            var eventData = eventArgs.Data;
            var eventBody = Encoding.UTF8.GetString(eventData.Body.ToArray());
            var tradeEvent = JsonSerializer.Deserialize<EventSchema.TradeEvent>(eventBody);

            // do soemthing with the event
            Console.WriteLine($"New trade for {tradeEvent.Details.StockTicker} at {tradeEvent.Details.TradePrice}");

            // update checkpointing in Event hub.
            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        // error handler
        private Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            Console.WriteLine($"Error: {eventArgs.Exception.Message}");
            return Task.CompletedTask;
        }
    }
}