using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SubscriberService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient("Service1", client =>
                    {
                        client.BaseAddress = new Uri("https://api.service.fcbt.com/");
                        client.DefaultRequestHeaders.Add("Accept", "application/json");
                        // security stuff
                    });
                    
                    services.AddHostedService<Worker>();
                });
    }
}