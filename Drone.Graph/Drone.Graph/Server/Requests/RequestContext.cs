using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Drone.Graph.Server.Requests
{
    /// <summary>
    /// Wrapper for the request that is being sent to request handlers. Only exposes a subset of information to the
    /// request handlers for ease of use.
    /// </summary>
    public class RequestContext
    {
        public IDictionary<string, StringValues> Headers { get; set; } 
        public string Path { get; set; }
        public string ContentType { get; set; }
        public string BodyAsString { get; set; }

        public TType ProjectTo<TType>()
        {
            return JsonConvert.DeserializeObject<TType>(BodyAsString);
        }

    }
}