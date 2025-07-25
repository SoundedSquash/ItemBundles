using MoreUpgrades.Classes;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

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

        public static List<Item> allUpgrades = new List<Item>();

        public static void InitCompat()
        {
            var pluginInstance = MoreUpgrades.Plugin.instance;

            allUpgrades.Clear();

            foreach (UpgradeItem upgradeItem in pluginInstance.upgradeItems)
            {
                var item = pluginInstance.assetBundle.LoadAsset<Item>(upgradeItem.name);
                DebugLogger.LogInfo($"MoreUpgradesCompat: Init {upgradeItem}, {item}");

                if (!item) continue;
                if (!item.prefab) continue;

                allUpgrades.Add(item);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static float GetItemValueMultiplier( float fallbackMult, string bundleAssetName)
        {
            var itemAssetName = BundleHelper.GetItemStringFromBundle(bundleAssetName);
            return MoreUpgrades.Plugin.ItemValueMultiplier(fallbackMult, itemAssetName);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static float GetUpgradeValueIncrease(float fallbackMult, string bundleAssetName)
        {
            var itemAssetName = BundleHelper.GetItemStringFromBundle(bundleAssetName);
            return MoreUpgrades.Plugin.UpgradeValueIncrease(fallbackMult, itemAssetName); ;
        }
    }
}
