using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rigger.Implementations;
using Rigger.Injection;
using Rigger.ManagedTypes;
using Rigger.ManagedTypes.Features;
using Rigger.ManagedTypes.Implementations;
using Rigger.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Rigger.Tests
{
    public class ScopedService : IScopedService, IScopedService2
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

    }

    public interface IScopedService
    {
    }
    public interface IScopedService2
    {
    }
    public class ScopeTests
    {
        private ITestOutputHelper output;
        public ScopeTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        private IServices InitServices()
        {

            /*var logFactory = new Func<Services, Type, object>((services, type) => {
            
                var fac = services.GetService<ILoggerFactory>();
 
                return null;
            });*/

            var services = new Services()
                .Add<IAutowirer, ContainerAutowirer>()
                .Add<ILoggerFactory>(new TestLogger(output).LoggerFactory)
                .Add(typeof(ILogger<>), typeof(Logger<>))
                .Add<IConstructorActivator>(new ManagedConstructorInvoker())
                .Add<IInstanceFactory,AutowireInstanceFactory>()
                .Add<IScopedService, ScopedService>(ServiceLifecycle.Scoped)
                .Add<IScopedService2, ScopedService>(ServiceLifecycle.Scoped);

                services.Add<IServiceScopeFactory, ServiceScopeFactory>();

                return services;
        }

        [Fact]
        public void TestScope()
        {
            var services = InitServices();
            
            IScopedService s1 = null;
            IScopedService s2;
            IScopedService s3;
            IScopedService2 s4;
            IScopedService2 s5;
            
            using (var scope = services.GetService<IServiceScopeFactory>().CreateScope())
            {
                try
                {
                    s1 = scope.ServiceProvider.GetService<IScopedService>();
                }
                catch (Exception e)
                {
                    output.WriteLine(e.ToString());
                }

                s2 = scope.ServiceProvider.GetService<IScopedService>();
                output.WriteLine($"{s1} {s2}");
            }
            using (var scope = services.GetService<IServiceScopeFactory>().CreateScope())
            {
                s3 = scope.ServiceProvider.GetService<IScopedService>();
            }

            s1.Should().BeSameAs(s2);
            s2.Should().NotBeSameAs(s3);

            using (var scope = services.GetService<IServiceScopeFactory>().CreateScope())
            {
                try
                {
                    s1 = scope.ServiceProvider.GetService<IScopedService>();
                }
                catch (Exception e)
                {
                    output.WriteLine(e.ToString());
                }

                s2 = scope.ServiceProvider.GetService<IScopedService>();
                
                using (var scope2 = scope.ServiceProvider.GetService<IServiceScopeFactory>().CreateScope())
                {
                    s3 = scope2.ServiceProvider.GetService<IScopedService>();
                    s4 = scope2.ServiceProvider.GetService<IScopedService2>();
                }   
                
                s5 = scope.ServiceProvider.GetService<IScopedService2>();

                s1.Should().BeSameAs(s2);
                s2.Should().BeSameAs(s3);
                s4.Should().NotBeSameAs(s5);
            }

        }
    }
}