using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rigger.Attributes;
using Rigger.Extensions;
using Rigger.Implementations;
using Rigger.Injection;
using Rigger.ManagedTypes;
using Rigger.ManagedTypes.ComponentHandlers;
using Rigger.ManagedTypes.ComponentScanners;
using Rigger.ManagedTypes.Features;
using Rigger.ManagedTypes.Implementations;
using Rigger.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Rigger.Tests
{
    [Singleton]
    public class SingletonTest
    {

    }

    [Managed]
    public class TransientPassService
    {
        [Autowire] public SingletonTest singleton;
    }
    public class ComponentScanTests
    {

        private ITestOutputHelper output;
        public ComponentScanTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        
        private IServices InitServices()
        {

            return new Services()
                .Add<IAutowirer>(new ContainerAutowirer())
                .Add<IComponentHandler<SingletonAttribute>, SingletonComponentHandler>()
                .Add<IComponentHandler<ManagedAttribute>, ManagedComponentHandler>()
                .Add<IComponentScanner, SingletonComponentScanner>()
                .Add<IComponentScanner, ManagedComponentScanner>()
                .Add<ILoggerFactory>(new TestLogger(output).LoggerFactory)
                .Add(typeof(ILogger<>), typeof(Logger<>))
                .Add<IConstructorActivator>(new ManagedConstructorInvoker())
                .Add<IInstanceFactory>(new AutowireInstanceFactory());
        }
        
        [Fact]
        public void TestSingleton()
        {
            var services = InitServices();
            
            var scanners = services.GetService<IEnumerable<IComponentScanner>>();

            scanners.ForEach(o =>
            {
                try
                {
                    o.ComponentScan(typeof(ComponentScanTests).Assembly);
                }
                catch (Exception e)
                {
                    services.GetService<ILogger<ComponentScanTests>>().LogError(e, "Cannot scan");
                    throw;
                }
            });

            //output.WriteLine($"Got types: {types}");
            
            services.GetService<SingletonTest>().Should().NotBeNull();
            services.GetService<TransientPassService>().Should().NotBeNull();
        }
    }
}