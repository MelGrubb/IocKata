using System;
using System.Collections.Generic;

namespace IocKata
{
    public static class IoC
    {
        private enum DependencyType
        {
            Instance,
            Delegate,
        }

        private static readonly Dictionary<Type, (object value, DependencyType dependencyType, bool isSingleton)> Dependencies = new Dictionary<Type, (object value, DependencyType dependencyType, bool isSingleton)>();

        public static void Register<T>(T instance)
        {
            Dependencies[typeof(T)] = (instance, DependencyType.Instance, true);
        }

        public static void Register<T>(Func<object> func, bool isSingleton = false)
        {
            Dependencies[typeof(T)] = (func, DependencyType.Delegate, isSingleton);
        }

        public static T Resolve<T>()
        {
            var dependency = Dependencies[typeof(T)];

            if (dependency.dependencyType == DependencyType.Instance)
            {
                return (T) dependency.value;
            }
            else
            {
                var value = (T) ((Func<object>) dependency.value).Invoke();
                if (dependency.isSingleton)
                {
                    Dependencies[typeof(T)] = (value, DependencyType.Instance, true);
                }

                return value;
            }
        }
    }
}