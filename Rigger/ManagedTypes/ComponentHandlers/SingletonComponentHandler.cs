﻿using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Rigger.Extensions;
using Rigger.ManagedTypes;
using Rigger.Attributes;
using Rigger.Injection;

namespace Rigger.ManagedTypes.ComponentHandlers
{
    public class SingletonComponentHandler : IComponentHandler<SingletonAttribute>
    {
        public IServices Services { get; set; }

        [Autowire] private ILogger _logger;
        public void HandleComponent(Type type)
        {
            /*
            var attribute = type.GetCustomAttribute<SingletonAttribute>();
            var lookupType = attribute.LookupType ?? type.GetInterfaces().FirstOr(type);
            var typeRegistration = _registry.Register(RegistrationType.Singleton, 
                lookupType, type);

            _logger?.LogDebug($"Created [Singleton] registration for {type} using Lookup Type {lookupType}");*/
        }
    }
}