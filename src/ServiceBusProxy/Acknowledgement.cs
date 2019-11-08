using System;

namespace ServiceBusProxy
{

    public class Acknowledgement
    {
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public Guid CorrelationId { get; set; }
    }
}