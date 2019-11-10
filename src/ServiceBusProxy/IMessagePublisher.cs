using System.Threading.Tasks;

namespace ServiceBusProxy
{
    public interface IMessagePublisher
    {
        Task PublishExpectingAcknowledgement<TMessage, TContext>(TMessage message, TContext context);
        Task Publish<TMessage>(TMessage message);
    }

    public class ApplicationPublished
    {
        public string ApplicationId { get; set; }
    }

    // This is resolved through the IoC container
    public class ApplicationPublishedCallback : IMessageCallback<ApplicationPublished>
    {
        public Task Callback(ApplicationPublished state, Acknowledgement acknowledgement)
        {
            throw new System.NotImplementedException();
        }
    }
}