using System.Threading.Tasks;

namespace ServiceBusProxy
{
    /// <summary>
    /// Sleight of hand just do deal with relaying object
    /// to the right strong typed handler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CallbackHandler<T> : ICallbackHandler
    {
        private readonly IMessageCallback<T> _callback;

        public CallbackHandler(IMessageCallback<T> callback)
        {
            _callback = callback;
        }

        public Task Handle(object state, Acknowledgement acknowledgement)
        {
            return _callback.Callback((T) state, acknowledgement);
        }
    }
}