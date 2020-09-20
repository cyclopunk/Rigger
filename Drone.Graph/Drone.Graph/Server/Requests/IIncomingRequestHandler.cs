namespace Drone.Graph.Server.Requests
{
    /// <summary>
    /// Interface for incoming requests to the BasicAPI server.
    /// </summary>
    public interface IIncomingRequestHandler
    {
        string Handle(RequestContext context);
    }
}