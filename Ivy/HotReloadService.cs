[assembly: System.Reflection.Metadata.MetadataUpdateHandlerAttribute(typeof(Ivy.HotReloadService))]
namespace Ivy
{
    public class HotReloadService
    {
        public static event Action<Type[]?>? UpdateApplicationEvent;

        internal static void ClearCache(Type[]? types) { }
        internal static void UpdateApplication(Type[]? types)
        {
            UpdateApplicationEvent?.Invoke(types);
        }
    }
}