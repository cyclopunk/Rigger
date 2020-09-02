﻿using System;
using TheCommons.Forge.ManagedTypes.Features;

namespace TheCommons.Forge.ManagedTypes.Lightweight
{
    public class AutowireInstanceFactory : IInstanceFactory, IServiceAware
    {
        public Services Services { get; set; }

        public object Make(Type type)
        {
            IConstructorInvoker invoker = Services.GetService<IConstructorInvoker>();
            IAutowirer autowire = Services.GetService<IAutowirer>();

            var instance = invoker?.Construct(type, new object[] {});

            autowire?.Inject(instance);

            return instance;
        }
    }
}