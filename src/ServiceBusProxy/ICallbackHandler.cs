using System.Threading.Tasks;

namespace ServiceBusProxy
{
    public interface ICallbackHandler
    {
        Task Handle(object state, Acknowledgement acknowledgement);
    }
}