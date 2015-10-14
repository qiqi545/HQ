using System;

namespace container
{
    public interface IDependencyRegistrar : IDisposable
    {
        void Register<T>(Func<T> builder, Lifetime lifetime) where T : class;
        void Register<T>(string name, Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class;
        void Register<T>(Func<IDependencyResolver, T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class;
        void Register<T>(string name, Func<IDependencyResolver, T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class;
        void Register(Type type, Func<IDependencyResolver, object> builder, Lifetime lifetime = Lifetime.AlwaysNew);
        void Register(Type interfaceType, Type implementationType, Lifetime lifetime = Lifetime.AlwaysNew);
        void Register<T>(T instance);
    }
}