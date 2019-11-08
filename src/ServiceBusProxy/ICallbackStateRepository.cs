using System;
using System.Threading.Tasks;

namespace ServiceBusProxy
{
    public interface ICallbackStateRepository
    {
        Task Store(Guid correlationId, object state);
        Task<object> Find(Guid correlationId);

        Task Delete(Guid correlationId);
    }
}