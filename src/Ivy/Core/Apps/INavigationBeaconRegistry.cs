namespace Ivy;

public interface INavigationBeaconRegistry
{
    /// <summary>Returns true if any app has registered a beacon for entity type T.</summary>
    bool HasBeacon<T>();

    /// <summary>Gets the beacon for entity type T, or null if none registered.</summary>
    NavigationBeacon<T>? GetBeacon<T>();
}
