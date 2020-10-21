# IOC Kata

One of the tools we often take for granted these days is the Inversion of Control (IoC) container. An IoC makes following the Dependency Inversion Principle (the "D" in SOLID) relatively simple, so it's not surprising that developers often confuse the two, but they are not the same thing. The IoC container is just one way of implementing the principle, and is the glue that holds most of our modern work together. Still, it's a black box to most developers, who have no idea how the magic happens under the covers. I've found that the best way to truly understand something is to build one yourself, so in this post we'll walk through creating a fully-functional IoC container that handles the most common cases.

We'll be building an IoC in the form of a coding kata, a practice which grew out of the Software Craftsmanship movement of the mid '90s. Katas are short coding exercises meant to help reinforce and understand solutions to common problems so that when they arise in the real world, you'll recognize them, and already be already familiar with the solution. You write a more or less canned solution through a series of small steps, adding a feature here, refactoring there, and working toward the accepted solution to a common business problem or perhaps implementing a simple game.

For me, the most useful katas were always those that taught me something, or were simply fun on their own. Guy Royse's "Evercraft Kata" (https://github.com/guyroyse/evercraft-kata) is not what I'd call a warm-up exercise, as it would likely take you many hours to complete your first time through. It doesn't result in a finished product that you can leverage in a business scenario, but it _does_ help you develop a way of thinking about the kinds of problems that _do_ come up in daily business such as adding new features to existing entities like users or businesses without bringing the existing system down in the process. If you've never completed it, I recommend working through it some weekend.

I wrote my "Itty Bitty IoC" after reading about some micro-IoC implementations by Oren Eini (http://ayende.com/Blog/archive/2007/10/20/Building-an-IoC-container-in-15-lines-of-code.aspx) and Ken Egozi (http://www.kenegozi.com/Blog/2008/01/17/its-my-turn-to-build-an-ioc-container-in-15-minutes-and-33-lines.aspx). I saw how an IoC works under the covers, and decided to build my own. Since that time, I've evolved it into a coding kata because it really is quite simple to understand and use. I'd like to walk you through that kata now. We'll build it up one feature at a time, following a test-driven development pattern by first writing a test to demonstrate the behavior we want, and then implementing that feature before moving on to the next.

## Step Zero - Setting up the dojo

Clone the GitHub repo at https://github.com/MelGrubb/IocKata, and ensure that you are on the "main" branch. This has an empty class for our IoC, an empty Test class, and a trio of inter-dependent classes called Foo, Bar, and Baz that will be used in the tests. I'll be using NUnit because it's the most universally understood testing library in the .Net space, but you are welcome to use whichever testing library you are most comfortable with. The repository also contains branches for each of the completed steps.

## Step One - Instance Registration

The first, and simplest kind of registration is nothing more than a directory of types or interfaces, and instances _of_ those types. This will simply hand us back an existing instance of a class whenever we ask for it. In essence, when I ask for an IFoo, I want the IoC to hand me back the specific instance of Foo that I registerd.

This first test verifies two things; that an instance can be both registered and resolved, and that subsequent registrations override previous ones (i.e. last-in-wins).

```csharp
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
    }
}
```

To implement this functionality, we're going to need a few things. We'll need a container of some kind to store the dependencies, those are the types we're registering. We can do this with a simple dictionary, using the type as the key, and the instance as the value. We'll also need a method to add instances _to_ this dictionary, and another to retrieve them.

```csharp
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
```

At this point, the test should pass, and we're ready to move on to the next step.

## Step Two - Delegate Registration

The next kind of registration is only slightly more complicated. Rather than providing the IoC with the actual object to return, we'll pass in a function to be executed whenever that dependency is resolved, essentially telling the IoC _how_ to build the type. These functions can even leverage the IoC as part of their logic. The usage should look like the following test.

```csharp
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
```

We can store the function in the existing dictionary since functions can be considered as objects, but we'll need a way to remember whether the entry represents an instance or a function, so we'll alter the dictionary to store a Tuple containing the object or function, and a new enumeration value that says what kind of dependency it is. We'll use the newer tuple syntax introduced in C# 7 to make this more readable later on, even if the declaration is a bit wordy.

```csharp
private enum DependencyType
{
    Instance,
    Delegate,
}

private static readonly Dictionary<Type, (object value, DependencyType dependencyType)> Dependencies
    = new Dictionary<Type, (object value, DependencyType dependencyType)>();
```

 Next, we'll need to modify the existing instance registration method to match, and add the new delegate registration function.

```csharp
public static void Register<T>(T instance)
{
    Dependencies[typeof(T)] = (instance, DependencyType.Instance);
}

public static void Register<T>(Func<object> func)
{
    Dependencies[typeof(T)] = (func, DependencyType.Delegate);
}
```

Finally, we'll expand the Resolve method to cover the new registration type

```csharp
public static T Resolve<T>()
{
    var dependency = Dependencies[typeof(T)];

    if (dependency.dependencyType == DependencyType.Instance)
    {
        return (T) dependency.value;
    }
    else
    {
        return (T) ((Func<object>) dependency.value).Invoke();
    }
}
```

## Step Three - Singletons

With one minor tweak, we can combine the instance and delegate resolution together in order to construct objects as singletons. That is to say we'll create a new instance of the dependency the first time it is asked for, but return that same instance for every subsequent call. This next test illustrates the behavior we want.

```csharp
[Test]
public void Step3_SingletonDelegateRegistration()
{
    IoC.Register<IBaz>(() => new Baz(), isSingleton: false);
    Assert.AreNotSame(IoC.Resolve<IBaz>(), IoC.Resolve<IBaz>());

    IoC.Register<IBaz>(() => new Baz(), isSingleton: true);
    Assert.AreSame(IoC.Resolve<IBaz>(), IoC.Resolve<IBaz>());
}
```

We'll need to update the dictionary again, this time adding a third value to the tuple to indicate whether or not the registration should be a singleton or not, and update the existing Register methods. The isSingleton value won't make any difference for Instance registrations, but I'd consider them singletons by definition, so I'll set it to true in the first Register method.

```csharp
private static readonly Dictionary<Type, (object value, DependencyType dependencyType, bool isSingleton)>
    Dependencies = new Dictionary<Type, (object value, DependencyType dependencyType, bool isSingleton)>();

public static void Register<T>(T instance)
{
    Dependencies[typeof(T)] = (instance, DependencyType.Instance, true);
}

public static void Register<T>(Func<object> func, bool isSingleton = false)
{
    Dependencies[typeof(T)] = (func, DependencyType.Delegate, isSingleton);
}
```

Finally, we'll update the Resolve method to create the dependency instance as usual, but re-register it as an instance when it's supposed to be a singleton.

```csharp
public static T Resolve<T>()
{
    var dependency = Dependencies[typeof(T)];

    if (dependency.dependencyType == DependencyType.Instance)
    {
        return (T) dependency.value;
    }
    else
    {
        var value = (T) ((Func<object>) dependency.value).Invoke();
        if (dependency.isSingleton)
        {
            Dependencies[typeof(T)] = (value, DependencyType.Instance, true);
        }

        return value;
    }
}
```

This is now a usable IoC, except for the fact that you have to tell it explicitly how to build everything. It doesn't know how to figure out anything on its own... yet.

## Step Four - Dynamic Resolution

One of the hallmarks of a real IoC is the ability for it to figure some things out on its own. As long as a type's dependencies are also registered, it shouldn't need to be given a function to do the work. It should just know what to do. Its usage should look like this.

```csharp
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
    Assert.AreNotSame(foo.Bar.Baz, IoC.Resolve<IBaz>());
}
```

You'll notice that in this test, we're no longer providing a delegate function. We're just telling the IoC what concrete class we want when we ask for the registered interface. It's up to the IoC to choose a constructor and invoke it, resolving any of the concrete class's dependencies. It's easier than it sounds. We'll create a third enumeration entry to represent this case, and add a new Register method.

```csharp
private enum DependencyType
{
    Instance,
    Delegate,
    Dynamic,
}

public static void Register<T1, T2>(bool isSingleton = false)
{
    Dependencies[typeof(T1)] = (typeof(T2), DependencyType.Dynamic, isSingleton);
}
```

Because generics are a compile-time thing, we won't be able to use the existing generic Resolve method to build the constructor parameters at runtime. So we'll first need to extract the main logic of the Resolve method into a private, non-generic version that simply returns objects, and call it from the existing generic Resolve method. This will allow the newly-extracted method to call back into itself to resolve the constructor parameters at runtime. I won't go into all the details of the new Resolve branch here since this post is long enough already, and isn't meant to be a tutorial on reflection, but the gist is this. Find the "greediest" constructor, that is the one with the most parameters, resolve those parameters by calling back in to the Resolve method, and then use those parameters to invoke the constructor of the type you were initially trying to build. Here is the completed version.

```csharp
public static T Resolve<T>()
{
    return (T) Resolve(typeof(T));
}

private static object Resolve(Type type)
{
    var dependency = Dependencies[type];

    if (dependency.dependencyType == DependencyType.Instance)
    {
        return dependency.value;
    }
    else if (dependency.dependencyType == DependencyType.Delegate)
    {
        var value = ((Func<object>) dependency.value).Invoke();
        if (dependency.isSingleton)
        {
            Dependencies[type] = (value, DependencyType.Instance, true);
        }

        return value;
    }
    else
    {
        var concreteType = (Type) dependency.value;
        var constructorInfo = concreteType.GetConstructors()
		    .OrderByDescending(o => (o.GetParameters().Length)).First();
        var parameterInfos = constructorInfo.GetParameters();

        if (parameterInfos.Length == 0)
        {
            return Activator.CreateInstance((Type)dependency.value);
        }
        else
        {
            var parameters = new List<object>(parameterInfos.Length);
            foreach (ParameterInfo parameterInfo in parameterInfos)
            {
                parameters.Add(Resolve(parameterInfo.ParameterType));
            }
            var value = constructorInfo.Invoke(parameters.ToArray());

            if (dependency.isSingleton)
            {
                Dependencies[type] = (value, DependencyType.Instance, true);
            }

            return value;
        }
    }
}
```

And that's it. We have a complete IoC in under 100 lines. You can make it even smaller if you forego some readability by removing the "else"s from the Resolve method, and the brackets from the branches with single instructions. You can see why I called it IttyBittyIoC, but there is still more you can do to make it even better.

## Extra credit:
- Add an InjectionConstructorAttribute to manually mark which constructor you want the IoC to use.
- Add convention-based assembly scanning to match up interfaces with similarly-named classes (e.g. IFoo/Foo).

You can look at the Step5 and Step6 branches in the GitHub repo to see how I implemented these features, and Step7 to see the fully-refactored, minimalist, 68-line version.
