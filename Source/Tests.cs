using NUnit.Framework;

namespace IocKata
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Step1_InstanceRegistration()
        {
            var instance1 = new Foo(new Bar(new Baz()));
            IoC.Register<IFoo>(instance1);

            var instance2 = new Foo(new Bar(new Baz()));
            IoC.Register<IFoo>(instance2);

            IoC.Register<IFoo>(instance1);
            IoC.Register<IFoo>(instance2);
            var value = IoC.Resolve<IFoo>();
            Assert.AreSame(instance2, value);
        }

        [Test]
        public void DelegateRegistrationTest()
        {
            IoC.Reset();
            IoC.Register<IFoo>(() => new Foo(new Bar(new Baz())));
        }
    }
}