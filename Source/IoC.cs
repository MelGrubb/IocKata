using System;
using System.Collections.Generic;

namespace IocKata
{
    public static class IoC
    {
        private static readonly Dictionary<Type, object> Instances = new Dictionary<Type, object>();

        public static void Register<T>(T instance)
        {
            Dependencies[typeof(T)] = instance;
        }

        public static void Register<T>(Func<object> func)
        {
            Delegates[typeof(T)] = func;
        }

        public static void Reset()
        {
            Instances.Clear();
            Delegates.Clear();
        }

        public static T Resolve<T>()
        {
            return (T) Dependencies[typeof(T)];
        }
    }
}