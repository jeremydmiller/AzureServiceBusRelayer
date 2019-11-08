using System.Threading.Tasks;

namespace ServiceBusProxy
{
    public interface IMessageCallback<T>
    {
        Task Callback(T state, Acknowledgement acknowledgement);
    }
}