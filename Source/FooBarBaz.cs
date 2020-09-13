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
    }

    public class Baz : IBaz
    {
    }
}