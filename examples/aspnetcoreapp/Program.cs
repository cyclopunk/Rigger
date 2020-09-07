using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rigger.Core;

namespace aspnetcoreapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHostBuilder builder = CreateHostBuilder(args);
            
            IHost host = builder.Build();
            
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new RiggedServiceProviderFactory("Drone"))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    
                    webBuilder
                        .UseIISIntegration() 
                        .UseSetting("detailedErrors", "true")
                        .UseStartup<Startup>();
                });
    }
}
