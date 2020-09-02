using System.Linq;

namespace TheCommons.Forge.ManagedTypes.Lightweight
{
    public static class ServiceDescriptionExtensions
    {
        /// <summary>
        /// Check to see if a service description is the same as another service description.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool IsEqualToService(this ServiceDescription first, ServiceDescription second)
        {

            // TypeName<T> != TypeName<X>, might be unnecessary to check this
            if (first.ServiceType.IsGenericTypeDefinition != second.ServiceType.IsGenericTypeDefinition)
            {
                return false;
            }

            return first.ServiceType == second.ServiceType;
        }
        /// <summary>
        /// This will find something registered as IOption<Test> if IOption<> is also registered
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static ServiceDescription MoreSpecificService(this ServiceDescription first, ServiceDescription second)
        {
            if (first.IsOpenService() && !second.IsOpenService())
            {
                return second;
            }

            // Add more logic here?

            return first;
        }

        public static bool IsValid(this ServiceDescription description)
        {
            var interfaces = description.ImplementationType.GetInterfaces();

            // handle open generics

            if (description.IsOpenService())
            { 
                
                return description.ImplementationType.GetInterfaces().Any(it =>
                    it.IsConstructedGenericType && it.GetGenericTypeDefinition() == description.ServiceType);
            }

            if (!description.IsInterfaceService() && description.ServiceType == description.ImplementationType)
            {
                return true;
            }

            if (!description.ImplementationType.IsSubclassOf(description.ServiceType) && !description.ImplementationType.GetInterfaces().Contains(description.ServiceType))
            {
                return false;
            }

            return true;
        }

        public static bool IsOpenService(this ServiceDescription sd)
        {
            return !sd.ServiceType.IsConstructedGenericType && sd.ServiceType.ContainsGenericParameters;
        }
        public static bool IsInterfaceService(this ServiceDescription sd)
        {
            return sd.ServiceType.IsInterface;
        }

        public static T AddServices<T>(this T sa, Services s) where T : IServiceAware
        {
            sa.Services = s;

            return sa;
        }
    }
}