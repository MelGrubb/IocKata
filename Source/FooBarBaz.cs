namespace IocKata
{
    public interface IFoo
    {
        IBar Bar { get; set; }
    }

    public class Foo : IFoo
    {
        public Foo(IBar bar)
        {
            Bar = bar;
        }

        public IBar Bar { get; set; }
    }

    public interface IBar
    {
        IBaz Baz { get; set; }
    }

    public class Bar : IBar
    {
        public Bar(IBaz baz)
        {
            Baz = baz;
        }

        public IBaz Baz { get; set; }
    }

    public interface IBaz
    {
        bool ExtraParameterWasSupplied { get; }
    }

    public class Baz : IBaz
    {
        public bool ExtraParameterWasSupplied { get; }

        [InjectionConstructor]
        public Baz()
        {
        }

        public Baz(object extraParameter)
        {
            ExtraParameterWasSupplied = true;
        }
    }
}