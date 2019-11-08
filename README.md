# AzureServiceBusRelayer

There's meant to be two projects:

1. `ServiceBusProxy` -- a Nuget lib that you would consume to be able to publish messages to Azure Service Bus with a way to request acknowledgements
1. `SubscriberService` -- a Windows service ("Worker" project in this case) that can do the relay from queued Azure Service Bus to HTTP services


The big thing is the `Acknowledgement` class that we can use to send a message back to the original caller about whether a message succeeded or failed
