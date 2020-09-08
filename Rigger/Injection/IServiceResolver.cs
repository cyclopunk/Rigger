namespace Rigger.Injection
{
    public interface IServiceResolver : IServiceAware
    {
        public object Resolve();
    }
}