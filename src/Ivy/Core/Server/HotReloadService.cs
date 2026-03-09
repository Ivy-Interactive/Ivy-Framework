[assembly: System.Reflection.Metadata.MetadataUpdateHandlerAttribute(typeof(Ivy.Core.Server.HotReloadService))]
namespace Ivy.Core.Server
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