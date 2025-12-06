using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ItemBundles
{
    public static class MoreUpgradesCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("bulletbot.moreupgrades");
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static float GetOriginalValueMultiplier(string bundleAssetName)
        {
            var originalItemName = BundleHelper.GetItemStringFromBundle(bundleAssetName);
            var originalItem = StatsManager.instance.itemDictionary[originalItemName];
            return MoreUpgradesAPI.ItemValueMultiplier(originalItem);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static float GetUpgradeValueIncrease(string bundleAssetName)
        {
            var originalItemName = BundleHelper.GetItemStringFromBundle(bundleAssetName);
            var originalItem = StatsManager.instance.itemDictionary[originalItemName];
            return MoreUpgradesAPI.UpgradeValueIncrease(originalItem);
        }
    }
}
