using System;

namespace Rigger.Implementations.Azure
{
    /**
     * Create a new type based logger that will send log messages to app insights.
     *
     * TODO Extend this with a simple configuration class for eventhub integration as well.
     */
    public class AzureLogging<T> : AbstractAzureLogger
    {
        public AzureLogging()
            :base(typeof(T))
        {
        }
    }

    public class AzureLogging : AbstractAzureLogger
    {
        public AzureLogging()
        {
            
        }

        public AzureLogging(Type type)
            :base(type)
        {
        }
    }
}