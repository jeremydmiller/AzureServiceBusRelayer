using System.Threading.Tasks;
using StructureMap;

namespace ServiceBusProxy
{
    public class AcknowledgementHandler
    {
        private readonly IContainer _container;
        private readonly ICallbackStateRepository _repository;

        public AcknowledgementHandler(IContainer container, ICallbackStateRepository repository)
        {
            _container = container;
            _repository = repository;
        }

        public async Task Handle(Acknowledgement acknowledgement)
        {
            var state = await _repository.Find(acknowledgement.CorrelationId);
            if (state == null)
            {
                // log that it's a miss on correlation id
            }
            else
            {
                // Just getting fancy w/ generics as a way to attach the right handler
                // for the acknowledgement state
                var handlerType = typeof(CallbackHandler<>).MakeGenericType(state.GetType());
                var handler = (ICallbackHandler)_container.GetInstance(handlerType);

                // TODO -- you'd wrap this in try/catch w/ retry mechanics too
                await handler.Handle(state, acknowledgement);
            }
        }
    }
}