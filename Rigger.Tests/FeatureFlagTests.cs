using System;
using System.Collections.Generic;
using FluentAssertions;
using TheCommons.Core.Configuration;
using TheCommons.Core.Configuration.Sources;
using TheCommons.Forge.ManagedTypes;
using TheCommons.Forge.ManagedTypes.Resolvers;
using TheCommons.Traits.Attributes;
using Xunit;

namespace TheCommons.Forge.Tests
{
    public class FeatureFlagTests
    {
        public interface ISomeService
        {

        }

        [Singleton]
        [Condition(Expression = "Color == Green")]
        public class GreenService : ISomeService
        {

        }

        [Singleton]
        [Condition(Expression = "Color == Blue")]
        public class BlueService : ISomeService
        {

        }

        [Condition(Expression = "Foo == Bar")]
        public class YellowService : ISomeService
        {

        }

        [Managed]
        public class ManagedService
        {

            [Autowire] public ISomeService service;

        }

        [Configuration]
        public class ConfigurationSetup
        {
            [Autowire]
            public ConfigurationSetup(IContainer container)
            {
                container.Get<IConfigurationService>().AddSource(new MapConfigurationSource
                {
                    BackingMap = new Dictionary<string, object>
                    {
                        {"Color", "Green"}
                    }
                });
            }
        }
        /// <summary>
        /// An application that will use feature flagging to load two different services
        /// </summary>
        class FeatureFlagApplication : ManagedTypes.Crucible
        {
            public FeatureFlagApplication() : base(typeof(FeatureFlagTests))
            {

            }
        }

        [Fact]
        public void TestConditions()
        {
            FeatureFlagApplication app = new FeatureFlagApplication();
            
            app.Get<IConfigurationService>().AddSource(new MapConfigurationSource
            {
                Priority = 500,
                BackingMap = new Dictionary<string, object>
                {
                    {"Color", "Blue"}
                }
            });

            ExpressionTypeResolver resolver = app.Get<IContainer>().Context.Inject(new ExpressionTypeResolver());
            
            resolver.AddType("Color == Green", typeof(GreenService));
            resolver.AddType("Color == Blue", typeof(BlueService));

            resolver.ResolveType().Should().BeAssignableTo<BlueService>();
        }
        [Fact]
        public void TestBlueGreenServices()
        {
            FeatureFlagApplication app = new FeatureFlagApplication();

            var service = app.Get<ManagedService>();
            var service2 = app.Get<ISomeService>();

            (service.service is GreenService).Should().BeTrue();

            // change the configuration, this should update which service is used.

            app.Get<IConfigurationService>().AddSource(new MapConfigurationSource
            {
                Priority = 500,
                BackingMap = new Dictionary<string, object>
                {
                    {"Color", "Blue"},
                    {"Foo", "Bar"}
                }
            });

            var service3 = app.Get<ISomeService>();

            service3.Should().BeAssignableTo<BlueService>();
        }
    }
}