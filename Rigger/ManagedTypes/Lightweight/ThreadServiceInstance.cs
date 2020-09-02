using System;
using System.Threading;
using TheCommons.Forge.ManagedTypes.Implementations;

namespace TheCommons.Forge.ManagedTypes.Lightweight
{
    /// <summary>
    /// Service Instance that uses a ThreadLocal
    /// </summary>
    public class ThreadServiceInstance : IServiceInstance, IServiceAware
    {
        private volatile ManagedConstructorInvoker _invoker; 
        private ThreadLocal<object> _threadLocalFactory;
        public Services Services { get; set; }
        public Type InstanceType { get; set; }

        public object Instance => _threadLocalFactory.Value;


        public object Get()
        {
            _threadLocalFactory = new ThreadLocal<object>(() =>
            {
                var instanceFactory = Services.GetService<IInstanceFactory>();

                return instanceFactory.Make(InstanceType) ?? Activator.CreateInstance(InstanceType);
            });


            return _threadLocalFactory.Value;
        }

        public void Dispose()
        {
            _threadLocalFactory?.Dispose();
            _invoker = null;
        }
    }
}