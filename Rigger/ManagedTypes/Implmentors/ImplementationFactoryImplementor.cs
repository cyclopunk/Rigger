using System;
using Rigger.ManagedTypes.Lightweight;

namespace Rigger.ManagedTypes.Implmentors
{
    /// <summary>
    /// DotNetCore DI Uses an implementation factory on its service descriptors. This is an IImplmentor
    /// for that type of instance.
    /// </summary>
    public class ImplementationFactoryImplementor : IImplementor
    {
        private ServiceLifecycle lifecycle;
        private object instance;
        private Func<object[], object> ImplementationFactoryWrapper { get; set; }

        public ImplementationFactoryImplementor(Func<object[],object> factory, ServiceLifecycle type=ServiceLifecycle.Singleton)
        {
            this.lifecycle = type;
            ImplementationFactoryWrapper = factory;
        }

        public object GetInstance(params object[] implementorParams)
        {
            if (lifecycle == ServiceLifecycle.Singleton)
            {
                return instance ??= ImplementationFactoryWrapper.Invoke(implementorParams);
            }

            return ImplementationFactoryWrapper.Invoke(implementorParams);
        }
    }
}