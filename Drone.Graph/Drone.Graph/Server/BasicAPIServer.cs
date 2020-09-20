using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Rigger.Attributes;
using Rigger.Configuration;
using Rigger.ManagedTypes;

namespace Drone.Graph.Server
{
    /// <summary>
    /// Basic API server for the graph using kestrel
    /// </summary>

        [Singleton]
        public class BasicAPIServer
        {
            public BasicAPIServer()
            {

            }

            public BasicAPIServer(IConfiguration configuration)
            {
                // register configuration class with Config provider;


            }

            /// <summary>
            /// Static autowire is necessary since the application UseStartup instantiates a new instance
            /// that is not managed. There may be a better way to do this but for now, these static parameters 
            /// should work. There may be an issue with thread safety, will look into this.
            /// </summary>
            [Autowire] private static IContainer container;

            [Autowire] private static IConfigurationService configuration;

            private IWebHost host;

            /// <summary>
            /// When the APIServer is created as a singleton, startup is ran.
            /// </summary>
            [OnCreate]
            public void Startup()
            {

                // condition doesn't really work for turning off a service, so we'll do this for now.
                if (configuration.Get(ConfigurationKeys.StartWebServer, "false") == "true")
                {
                    host = new WebHostBuilder()
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<BasicAPIServer>()
                        .ConfigureKestrel(serverOptions =>
                        {
                            serverOptions.Listen(IPAddress.IPv6Any, configuration.Get(ConfigurationKeys.Port, 5030),
                                listenOptions =>
                                {
                                    var certFile = configuration.Get(ConfigurationKeys.CertificateFile, "server.pfx");
                                    var certPassword = configuration.Get(ConfigurationKeys.CertificatePassword,
                                        "DontDoIt!");

                                    listenOptions.UseHttps(
                                        certFile,
                                        certPassword
                                    );
                                });
                        })
                        .Build();

                    host.Run();
                }
            }

            /// <summary>
            /// Method that is called when the host is configured.
            /// </summary>
            /// <param name="app"></param>
            public void Configure(IApplicationBuilder app)
            {
                // RequestDelegate needs to be registered by a Configuration or Bootstrap
                // TODO allow for multiple request delegates i.e. middleware

                RequestDelegate rDelegate = container.Get<RequestDelegate>()
                                            ?? throw new Exception(
                                                "Could not find a registered RequestDelegate, please register one.");

                app.Run(rDelegate);
            }

        }
}