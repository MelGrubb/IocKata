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

            var value = IoC.Resolve<IFoo>();
            Assert.AreSame(instance2, value);
        }

        [Test]
        public void Step2_DelegateRegistration()
        {
            IoC.Register<IBaz>(() => new Baz());
            IoC.Register<IBar>(() => new Bar(IoC.Resolve<IBaz>()));
            IoC.Register<IFoo>(() => new Foo(IoC.Resolve<IBar>()));

            var value = IoC.Resolve<IFoo>();
            Assert.IsInstanceOf<Foo>(value);
            Assert.IsInstanceOf<Bar>(value.Bar);
            Assert.IsInstanceOf<Baz>(value.Bar.Baz);
        }

        [Test]
        public void Step3_SingletonDelegateRegistration()
        {
            IoC.Register<IBaz>(() => new Baz(), isSingleton: false);
            Assert.AreNotSame(IoC.Resolve<IBaz>(), IoC.Resolve<IBaz>());

            IoC.Register<IBaz>(() => new Baz(), isSingleton: true);
            Assert.AreSame(IoC.Resolve<IBaz>(), IoC.Resolve<IBaz>());
        }

        [Test]
        public void Step4_AutomaticResolution()
        {
            IoC.Register<IBaz, Baz>();
            IoC.Register<IBar, Bar>(isSingleton: true);
            IoC.Register<IFoo, Foo>();

            var foo = IoC.Resolve<IFoo>();
            Assert.IsInstanceOf<Foo>(foo);
            Assert.IsInstanceOf<Bar>(foo.Bar);
            Assert.IsInstanceOf<Baz>(foo.Bar.Baz);

            Assert.AreNotSame(foo, IoC.Resolve<IFoo>());
            Assert.AreSame(foo.Bar, IoC.Resolve<IBar>());
            Assert.AreSame(foo.Bar, IoC.Resolve<IBar>());
            Assert.AreNotSame(foo.Bar.Baz, IoC.Resolve<IBaz>());
        }

        [Test]
        public void Step5_InjectionConstructorAttribute()
        {
            IoC.Register<IBaz, Baz>();

            var baz = IoC.Resolve<IBaz>();
            Assert.IsFalse(baz.ExtraParameterWasSupplied);
        }
    }
}