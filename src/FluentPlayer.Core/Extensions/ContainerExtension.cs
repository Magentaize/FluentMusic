using System;
using System.Linq;
using DryIoc;

namespace Magentaize.FluentPlayer.Core.Extensions
{
    public static class ContainerExtension
    {
        public static Container RegisterInstance(this Container container, Type type, object instance)
        {
            container.UseInstance(type, instance);
            return container;
        }

        public static Container RegisterInstance(this Container container, Type type, object instance, string name)
        {
            container.UseInstance(type, instance, serviceKey: name);
            return container;
        }

        public static Container RegisterSingleton<T>(this Container container)
        {
            container.Register<T>(reuse: Reuse.Singleton);
            return container;
        }

        public static Container RegisterSingleton(this Container container, Type from, Type to)
        {
            container.Register(from, to, Reuse.Singleton);
            return container;
        }

        public static Container RegisterSingleton(this Container container, Type from, Type to, string name)
        {
            container.Register(from, to, Reuse.Singleton, serviceKey: name);
            return container;
        }

        public static T Resolve<T>(this Container container, Type type, params (Type Type, object Instance)[] parameters)
        {
            return (T) container.Resolve(type, args: parameters.Select(p => p.Instance).ToArray());
        }

        public static T Resolve<T>(this Container container, Type type, string name, params (Type Type, object Instance)[] parameters)
        {
            return (T) container.Resolve(type, name, args: parameters.Select(p => p.Instance).ToArray());
        }
    }
}