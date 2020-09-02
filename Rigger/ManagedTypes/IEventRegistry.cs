using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rigger.ManagedTypes
{
    public interface IEventRegistry
    {
        IEnumerable<EventReceiver> Register(object instance);
        void Fire(object eventToFire);
        Task FireAsync(object eventToFire);
    }
}