using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace ServiceBusProxy
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly AzureServiceBusSettings _settings;
        private readonly ICallbackStateRepository _repository;

        public MessagePublisher(AzureServiceBusSettings settings, ICallbackStateRepository repository)
        {
            _settings = settings;
            _repository = repository;
        }

        public async Task PublishExpectingAcknowledgement<TMessage, TState>(TMessage message, TState context)
        {
            // It's actually important here to use sequential Guids
            var correlationId = CombGuidIdGeneration.NewGuid();

            await _repository.Store(correlationId, context);
            
            var serializer = new JsonSerializer();

            var stream = new MemoryStream();
            serializer.Serialize(new JsonTextWriter(new StreamWriter(stream)), message);
            stream.Position = 0;

            var asbMessage = new Message
            {
                MessageId = correlationId.ToString(),
                Body = stream.ToArray(),
                Label = typeof(TMessage).Name
            };

            // The current application should be listening for acknowledgements at exactly
            // this topic, so there'd need to be a dedicated topic client for it
            asbMessage.ReplyTo = _settings.AcknowledgementTopic;
            
            // TODO -- gotta tell the downstream subscriber service 
            // that you want an acknowledgement back
            asbMessage.UserProperties.Add("AckRequested", true);

        }

        public Task Publish<TMessage>(TMessage message)
        {
            // TODO -- later
            throw new System.NotImplementedException();
        }
    }
}