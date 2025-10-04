using Ivy.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ivy.NavigationBacon
{
    public static class NavigationBeaconLookup
    {
        private static readonly Dictionary<Type, object> _beacons = new();
        private static bool _initialized = false;

        private static void InitListBeacons()
        {
            if (_initialized) return;

            // Scan all loaded assemblies for [NavigationBeacon] attributes
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttributes(typeof(NavigationBeaconAttribute), false).Any());

            foreach (var type in types)
            {
                var attrs = (NavigationBeaconAttribute[])type.GetCustomAttributes(typeof(NavigationBeaconAttribute), false);

                foreach (var attr in attrs)
                {
                    // Find the static factory method
                    var method = type.GetMethod(attr.FactoryMethodName, BindingFlags.Public | BindingFlags.Static);
                    if (method == null)
                        throw new InvalidOperationException($"Factory method {attr.FactoryMethodName} not found on {type.Name}");

                    var beacon = method.Invoke(null, null);
                    if (beacon != null)
                    {
                        _beacons[attr.EntityType] = beacon;
                    }
                }
            }

            _initialized = true;
        }

        public static NavigateArgs? GetNavigationArgsFor<T>(T Entity)
        {
            InitListBeacons();

            if (_beacons.ContainsKey(typeof(T)) && _beacons[typeof(T)] is NavigationBeacon<T> beacon)
            {
                var navigationArgs = beacon.ArgsBuilder.Invoke(Entity);
                return navigationArgs;
            }
            return null;
        }

        public static bool HasBeaconFor<T>()
        {
            InitListBeacons();
            return _beacons.ContainsKey(typeof(T));
        }
    }
}
