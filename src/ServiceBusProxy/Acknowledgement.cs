using System;

namespace ServiceBusProxy
{

    public class Acknowledgement
    {
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public Guid OriginalId { get; set; }
    }

    // Routing Slip approach as opposed to 
    // a persistent saga storage
    public class AcknowledgementEnvelope
    {
        public byte[] Body { get; set; }
        public byte[] Context { get; set; }
    }
}