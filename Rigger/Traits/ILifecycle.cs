namespace Rigger.Attributes
{
    /// <summary>
    /// This is a marker interface for Lifecycle attributes. All lifecycle
    /// attributes are pre-loaded into the type registry and method invokers
    /// are cached for any method marked with a lifecycle attribute. The default
    /// lifecycle attributes that the basic container supports are: OnBuilt, OnRegistered, OnCreate, OnStartup and OnDestroy.
    /// </summary>
    public interface ILifecycle
    {
        
    }
}