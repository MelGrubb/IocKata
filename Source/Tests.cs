using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace IocKata
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void InstanceRegistrationTest()
        {
            var instance1 = new Foo(new Bar(new Baz()));
            var instance2 = new Foo(new Bar(new Baz()));

            IoC.Register<IFoo>(instance1);
            IoC.Register<IFoo>(instance2);
            var value = IoC.Resolve<IFoo>();
            Assert.AreSame(instance2, value);
        }
    }
}
