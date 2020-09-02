namespace TheCommons.Forge.ManagedTypes.Implmentors
{
    public interface IImplementor<TOpen>
    {
        TClosed GetInstance <TClosed>(params object[] implementorParams);
    }

    public interface IImplementor
    {
        object GetInstance(params object[] implementorParams);
    }
}