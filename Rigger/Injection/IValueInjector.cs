using System;
using System.Collections.Generic;
using System.Text;

namespace Rigger.Injection
{

    public interface IValueInjector : IServiceAware
    {

        public T Inject<T>(T obj);
    }
}
