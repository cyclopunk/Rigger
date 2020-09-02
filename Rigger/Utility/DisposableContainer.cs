using System;
using System.Collections.Generic;

namespace Rigger.Utility
{
    /**
     * Container for disposing multiple disposables at once.
     */
    public class DisposableContainer : IDisposable
    {
        private readonly IEnumerable<IDisposable> _disposables;
        public DisposableContainer(IEnumerable<IDisposable> scopes)
        {
            _disposables = scopes;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}