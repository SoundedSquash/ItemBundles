using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using VanillaUpgrades.Classes;

namespace ItemBundles
{
    public static class VanillaUpgradesCompat
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("bulletbot.vanillaupgrades");
                }
                return (bool)_enabled;
            }
        }

        public static void InitCompat()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static float GetUpgradeValueIncrease(float fallbackMult, string bundleAssetName)
        {
            var itemAssetName = BundleHelper.GetItemStringFromBundle(bundleAssetName);
            return VanillaUpgrades.Plugin.UpgradeValueIncrease(fallbackMult, itemAssetName); ;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void UpgradeViaBundle(ItemToggle itemToggle)
        {
            var itemAssetName = BundleHelper.GetItemStringFromBundle( itemToggle.gameObject.GetComponent<ItemAttributes>().item.itemAssetName );
            var pluginInstance = VanillaUpgrades.Plugin.instance;

            UpgradeItem upgradeItem = pluginInstance.upgradeItems.FirstOrDefault((UpgradeItem x) => x.name == itemAssetName);
            if (upgradeItem != null)
            {
                foreach (PlayerAvatar player in SemiFunc.PlayerGetAll())
                {
                    VanillaUpgradesManager.instance.Upgrade(upgradeItem.name, SemiFunc.PlayerGetSteamID(player), 1);
                }
            }
        }
    }
}
