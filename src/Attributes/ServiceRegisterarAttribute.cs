using Microsoft.Extensions.DependencyInjection;
using System;

namespace Devor.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceRegisterarAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
        //Most Have Resolve Method Like this : public object Resolve() { /*your implementation*/ }
        public Type ImplementationFactoryResolver { get; set; }
        //Most Have Resolve Method Like this : public object Resolve() { /*your implementation*/ }
        public Type ImplementationInstanceResolver { get; set; }
    }
}
