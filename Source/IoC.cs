using System;
using System.Collections.Generic;

namespace IocKata
{
    public static class IoC
    {
        private static readonly Dictionary<Type, object> Dependencies = new Dictionary<Type, object>();

        public static void Register<T>(T instance)
        {
            Dependencies[typeof(T)] = instance;
        }

        public static T Resolve<T>()
        {
            return (T) Dependencies[typeof(T)];
        }
    }
}