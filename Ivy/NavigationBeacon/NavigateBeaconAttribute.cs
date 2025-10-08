using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivy.NavigationBeacon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NavigationBeaconAttribute : Attribute
    {
        public Type EntityType { get; }
        public string FactoryMethodName { get; }

        public NavigationBeaconAttribute(Type entityType, string factoryMethodName)
        {
            EntityType = entityType;
            FactoryMethodName = factoryMethodName;
        }
    }
}
