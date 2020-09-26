using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IocKata
{
    public static class IoC
    {
        private enum DependencyType
        {
            Instance,
            Delegate,
            Dynamic,
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

        public static void Register<T1, T2>(bool isSingleton = false)
        {
            Dependencies[typeof(T1)] = (typeof(T2), DependencyType.Dynamic, isSingleton);
        }

        public static T Resolve<T>()
        {
            return (T) Resolve(typeof(T));
        }

        private static object Resolve(Type type)
        {
            var dependency = Dependencies[type];

            if (dependency.dependencyType == DependencyType.Instance)
            {
                return dependency.value;
            }
            else if (dependency.dependencyType == DependencyType.Delegate)
            {
                var value = ((Func<object>) dependency.value).Invoke();
                if (dependency.isSingleton)
                {
                    Dependencies[type] = (value, DependencyType.Instance, false);
                }

                return value;
            }
            else
            {
                var concreteType = (Type) dependency.value;
                var constructorInfo = concreteType.GetConstructors()
                    .OrderByDescending(o => (o.GetCustomAttributes(typeof(InjectionConstructorAttribute), false).Any()))
                    .ThenByDescending(o => (o.GetParameters().Length)).First();
                var parameterInfos = constructorInfo.GetParameters();

                if (parameterInfos.Length == 0)
                {
                    return Activator.CreateInstance((Type)dependency.value);
                }
                else
                {
                    var parameters = new List<object>(parameterInfos.Length);
                    foreach (ParameterInfo parameterInfo in parameterInfos)
                    {
                        parameters.Add(Resolve(parameterInfo.ParameterType));
                    }
                    var value = constructorInfo.Invoke(parameters.ToArray());

                    if (dependency.isSingleton)
                    {
                        Dependencies[type] = (value, DependencyType.Instance, false);
                    }
                
                    return value;
                }
            }
        }
    }
}