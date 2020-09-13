using System;
using System.Collections.Generic;

namespace IocKata
{
    public static class IoC
    {
        private static readonly Dictionary<Type, Func<object>> Delegates = new Dictionary<Type, Func<object>>();
        private static readonly Dictionary<Type, object> Instances = new Dictionary<Type, object>();

        public static void Register<T>(T instance)
        {
            Instances[typeof(T)] = instance;
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
            var result = default(T);

            if (Instances.ContainsKey(typeof(T)))
                result = (T) Instances[typeof(T)];

            if (Delegates.ContainsKey(typeof(T)))
                result = (T) Delegates[typeof(T)].Invoke();

            return result;
        }
    }
}