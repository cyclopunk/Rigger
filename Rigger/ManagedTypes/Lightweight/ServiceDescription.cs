﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TheCommons.Forge.ManagedTypes.Lightweight
{
    public class ServiceDescription
    {
        public Type ServiceType { get; set; }

        public Type ImplementationType { get; set; }
        
        public ServiceLifecycle LifeCycle { get; set; }

        public List<Type> ExtraTypes = new List<Type>();

        public IEnumerable<Type> AllTypes()
        {
            return new List<Type> {ImplementationType}.Concat(ExtraTypes);
        }
    }
}