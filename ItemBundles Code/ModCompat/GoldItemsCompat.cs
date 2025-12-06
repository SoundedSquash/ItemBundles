using System.Runtime.CompilerServices;
using UnityEngine;

namespace ItemBundles
{
    public static class GoldItemsCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("WorthyOtter.GoldItems");
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TryAddGoldMarker( GameObject baseItem, ref GameObject itemPrefab )
        {
            if (baseItem.GetComponent<GoldItems.GoldItemMarker>())
            {
                DebugLogger.LogInfo("|----  GoldItem shop item found!");
                if (!itemPrefab.TryGetComponent<GoldItems.GoldItemMarker>(out _))
                {
                    itemPrefab.AddComponent<GoldItems.GoldItemMarker>();
                }
            }
        }
    }
}
