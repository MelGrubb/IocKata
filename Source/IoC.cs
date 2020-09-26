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

        private static readonly Dictionary<Type, (object value, DependencyType dependencyType)> Dependencies = new Dictionary<Type, (object value, DependencyType dependencyType)>();

        public static void Register<T>(T instance)
        {
            Dependencies[typeof(T)] = (instance, DependencyType.Instance);
        }

        public static void Register<T>(Func<object> func)
        {
            Dependencies[typeof(T)] = (func, DependencyType.Delegate);
        }

        public static T Resolve<T>()
        {
            var dependency = Dependencies[typeof(T)];

            if (dependency.dependencyType == DependencyType.Instance)
            {
                return (T)dependency.value;
            }
            else
            {
                return (T)((Func<object>)dependency.value).Invoke();
            }
        }
    }
}