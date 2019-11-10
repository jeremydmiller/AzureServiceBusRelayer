using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceBusProxy;

namespace SubscriberService
{
    // TODO -- set up the Azure Service Bus listener here
    
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ListenerSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IList<TopicSubscriber> _subscribers = new List<TopicSubscriber>();

        public Worker(ILogger<Worker> logger, ListenerSettings settings, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _settings = settings;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var subscription in _settings.Subscriptions)
            {
                var subscriber = new TopicSubscriber(_settings, subscription, _httpClientFactory, _logger);
                
                _subscribers.Add(subscriber);
            }
        }
    }

    public class ListenerSettings
    {
        public string AzureServiceBusConnectionString { get; set; }
        
        
        public Subscription[] Subscriptions { get; set; }
    }

    public class TopicSubscriber
    {
        private readonly ListenerSettings _settings;
        private readonly Subscription _subscription;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Worker> _logger;
        private SubscriptionClient _client;

        public TopicSubscriber(ListenerSettings settings, Subscription subscription,
            IHttpClientFactory httpClientFactory, ILogger<Worker> logger)
        {
            _settings = settings;
            _subscription = subscription;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _client = new SubscriptionClient(settings.AzureServiceBusConnectionString, subscription.Topic, subscription.SubscriptionName);
            
            var options = new MessageHandlerOptions(args => Task.CompletedTask);
            
            _client.RegisterMessageHandler(ProcessMessage, options);
            
        }

        public async Task ProcessMessage(Message message, CancellationToken cancellation)
        {
            int responseCode = 500;
            string description = "";

            bool ackRequested = false;
            if (message.UserProperties.TryGetValue("AckRequested", out object header))
            {
                ackRequested = (bool)header;
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                
                // Probably more to this, but for now
                var request = new HttpRequestMessage(_subscription.HttpMethod, _subscription.Url)
                {
                    Content = new ByteArrayContent(message.Body)
                };


                var response = await httpClient.SendAsync(request, cancellation);

                responseCode = (int) response.StatusCode;
                description = response.ReasonPhrase;
            }
            catch (Exception e)
            {
                description = e.Message;
                _logger.LogError(e, $"Failed when trying to relay message on topic '{_subscription.Topic}'");
                
                // Send the message to the dead letter queue
                
            }

            if (ackRequested)
            {
                await SendAcknowledgement(message, responseCode, description);
            }
        }

        private async Task SendAcknowledgement(Message message, int responseCode, string description)
        {
            var ack = new Acknowledgement
            {
                OriginalId = Guid.Parse(message.CorrelationId),
                StatusCode = responseCode,
                StatusDescription = description
            };

            var serializer = new JsonSerializer();
            var stream = new MemoryStream();

            serializer.Serialize(new JsonTextWriter(new StreamWriter(stream)), ack);
            stream.Position = 0;

            var ackMessage = new Message
            {
                Body = stream.ToArray(),
                ContentType = "application/json"
            };

            var client = new TopicClient(_settings.AzureServiceBusConnectionString, message.ReplyTo);
            await client.SendAsync(ackMessage);
        }
    }

    public class Subscription
    {
        public string Topic { get; set; }
        public string SubscriptionName { get; set; }
        public string Url { get; set; }
        public HttpMethod HttpMethod { get; set; }
        
        // There would be more later
    }
}