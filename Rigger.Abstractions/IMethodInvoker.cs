namespace Rigger.Reflection
{
    /// <summary>
    /// Interface for a method invoker. A method invoker will invoke a method using
    /// reflection.
    /// </summary>
    public interface IMethodInvoker
    {
        string MethodName { get; set; }
        object Invoke(object destination, params object[] values);
    }
}