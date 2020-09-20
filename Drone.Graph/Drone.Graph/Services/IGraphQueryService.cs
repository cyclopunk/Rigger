using EntityGraphQL;

namespace Drone.Graph.Services
{
    // Exposed interface for the graph query service.
    public interface IGraphQueryService
    {
        public QueryResult MakeResult(QueryRequest request);
    }
}