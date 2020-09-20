using System;
using Drone.Graph.Server.Requests;
using Drone.Graph.Services;
using EntityGraphQL;
using Newtonsoft.Json;
using Rigger.Attributes;

namespace Drone.Graph.Server
{
    
    [Managed]
    public class GraphQLIncomingRequestHandler : IIncomingRequestHandler
    {

        [Autowire] private IGraphQueryService graphQueryService;

        public string Handle(RequestContext context)
        {
            QueryRequest request =
                JsonConvert.DeserializeObject<QueryRequest>(context.BodyAsString);

            var result = graphQueryService.MakeResult(request);
            try
            {
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception e)
            {
                return JsonConvert.SerializeObject(new QueryResult()
                {
                    Errors = {new GraphQLError(e.InnerException != null ? e.InnerException.Message : e.Message)}
                });
            }
        }
    }
}