namespace Ivy;

public class NavigationBeaconRegistry : INavigationBeaconRegistry
{
    private readonly Dictionary<Type, object> _beacons = new();

    public void Register<T>(NavigationBeacon<T> beacon)
    {
        if (_beacons.ContainsKey(typeof(T)))
        {
            throw new InvalidOperationException(
                $"A navigation beacon for entity type '{typeof(T).Name}' is already registered. " +
                "Only one app can register a beacon for each entity type.");
        }
        _beacons[typeof(T)] = beacon;
    }

    public bool HasBeacon<T>() => _beacons.ContainsKey(typeof(T));

    public NavigationBeacon<T>? GetBeacon<T>()
    {
        return _beacons.TryGetValue(typeof(T), out var beacon)
            ? (NavigationBeacon<T>)beacon
            : null;
    }
}
