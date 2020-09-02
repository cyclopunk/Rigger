using TheCommons.Traits.Attributes;

namespace TheCommons.Forge.ManagedTypes
{
    /// <summary>
    /// Interface for classes that will autowire an instance. How autowiring
    /// is done is up to the implementor of the class.
    /// </summary>
    public interface IAutowirer
    {
        TRType Inject<TRType>(TRType o);
    }
}