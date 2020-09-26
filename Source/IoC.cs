using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IocKata
{
    public static class IoC
    {
        private static readonly Dictionary<Type, (object value, DependencyType dependencyType, bool isSingleton)> Dependencies = new Dictionary<Type, (object value, DependencyType dependencyType, bool isSingleton)>();

        public static void Register<T>(T instance) => Dependencies[typeof(T)] = (instance, DependencyType.Instance, true);

        public static void Register<T>(Func<object> func, bool isSingleton = false) => Dependencies[typeof(T)] = (func, DependencyType.Delegate, isSingleton);

        public static void Register<T1, T2>(bool isSingleton = false) => Dependencies[typeof(T1)] = (typeof(T2), DependencyType.Dynamic, isSingleton);

        public static void Register(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t => t.GetInterfaces().Any(i => i.Name == $"I{t.Name}")))
                Dependencies[type.GetInterface($"I{type.Name}")] = (type, DependencyType.Dynamic, false);
        }

        public static T Resolve<T>() => (T) Resolve(typeof(T));

        private static object Resolve(Type type)
        {
            var dependency = Dependencies[type];
            object value = null;

            if (dependency.dependencyType == DependencyType.Instance) return dependency.value;

            if (dependency.dependencyType == DependencyType.Delegate)
            {
                value = ((Func<object>) dependency.value).Invoke();
                if (dependency.isSingleton) Dependencies[type] = (value, DependencyType.Instance, false);

                return value;
            }

            var concreteType = (Type) dependency.value;
            var constructorInfo = concreteType.GetConstructors()
                .OrderByDescending(o => o.GetCustomAttributes(typeof(InjectionConstructorAttribute), false).Any())
                .ThenByDescending(o => o.GetParameters().Length).First();
            var parameterInfos = constructorInfo.GetParameters();

            if (parameterInfos.Length == 0)
                return Activator.CreateInstance((Type) dependency.value);

            var parameters = new List<object>(parameterInfos.Length);
            foreach (var parameterInfo in parameterInfos) parameters.Add(Resolve(parameterInfo.ParameterType));
            value = constructorInfo.Invoke(parameters.ToArray());

            if (dependency.isSingleton) Dependencies[type] = (value, DependencyType.Instance, false);

            return value;
        }

        public static void Reset() => Dependencies.Clear();

        private enum DependencyType
        {
            Instance,
            Delegate,
            Dynamic
        }
    }
}