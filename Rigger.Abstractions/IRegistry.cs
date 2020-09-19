using System;
using System.Collections.Generic;
using System.Threading;

namespace Rigger.ManagedTypes
{
    public interface IRegistry<TItem, TRegClass> where TRegClass : IRegistration
    {
        TRegClass Register(TItem item);
        TRegClass Find(Func<TRegClass, bool> findPredicate);
        IEnumerable<TRegClass> Where(Func<TRegClass, bool> findPredicate);
    }
}