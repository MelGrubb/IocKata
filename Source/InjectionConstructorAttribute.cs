using System;

namespace IocKata
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class InjectionConstructorAttribute : Attribute
    {
    }
}