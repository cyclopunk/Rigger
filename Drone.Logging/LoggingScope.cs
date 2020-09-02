using Rigger.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Serilog.Context;

namespace Drone.Logging
{
    /**
     * A logging scope that will inject properties into the
     * logging stream. This class manages the disposables that is created by Serilog log context based on the local thread. See LogContext.
     *
     * Peter Messina is the primary author of these logging services.
     */
    public static class LoggingScope
    {
        private static readonly AsyncLocal<ImmutableDictionary<string, object>> ScopeItems = new AsyncLocal<ImmutableDictionary<string, object>>();

        private static ImmutableDictionary<string, object> Items
        {
            get => ScopeItems.Value ?? (ScopeItems.Value = ImmutableDictionary<string, object>.Empty);
            set => ScopeItems.Value = value;
        }

        public static object GetValue(string key) => (Items.ContainsKey(key) ? Items[key] : null);

        /**
         * Add a dictionary of logging scopes.
         */
        public static IDisposable BeginCorrelationScopes(Dictionary<string, object> allScopes)
        {
            return new DisposableContainer(allScopes
                .ToList()
                .Select(o => Begin(o.Key, o.Value)));
        }

        public static LoggingContextScope Begin(string key, object value)
        {
            if (Items.ContainsKey(key))
            {
                // TODO Not sure this needs to be an exception, we can pass a NOP disposable instead?
                throw new InvalidOperationException($"{key} is already being correlated!");
            }

            LoggingContextScope scope = new LoggingContextScope(Items, LogContext.PushProperty(key, value));

            Items = Items.Add(key, value);

            return scope;
        }

        public class LoggingContextScope : IDisposable
        {
            private IDisposable _parent;
            private readonly ImmutableDictionary<string, object> _bookmark;
            private readonly IDisposable _logContextPop;
            public LoggingContextScope(IDisposable parent, ImmutableDictionary<string, object> bookmark, IDisposable logContextPop)
            {
                _parent = parent;
                _bookmark = bookmark ?? throw new ArgumentNullException(nameof(bookmark));
                _logContextPop = logContextPop ?? throw new ArgumentNullException(nameof(logContextPop));
            }

            public LoggingContextScope(ImmutableDictionary<string, object> bookmark, IDisposable logContextPop)
            {
                _bookmark = bookmark ?? throw new ArgumentNullException(nameof(bookmark));
                _logContextPop = logContextPop ?? throw new ArgumentNullException(nameof(logContextPop));
            }

            public LoggingContextScope And(string key, object value)
            {

                LoggingContextScope scope = Begin(key, value);
                scope._parent = this;

                return scope;
            }

            public void Dispose()
            {

                _logContextPop.Dispose();

                Items = _bookmark;

                _parent?.Dispose();
            }
        }
    }
}