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
                allUpgrades.Add(item);
            }

            /* Deprecated, replaced with dynamic generation method, to be purged later
            foreach (UpgradeItem item in pluginInstance.upgradeItems)
            {
                var newItem = ItemBundles.Instance.assetBundle.LoadAsset<Item>(item.fullName + " Bundle");
                if ( newItem )
                {
                    REPOLib.Modules.Items.RegisterItem( newItem );

                    DebugLogger.LogInfo($"-- MoreUpgradesCompat found matching bundle item: {newItem}", true);
                    newItem.prefab.GetComponent<ModdedItemUpgradeBundle>().upgradeItemName = item.name;
                    newItem.prefab.GetComponent<ModdedItemUpgradeBundle>().FixMaterial();
                    newItem.prefab.GetComponent<ModdedItemUpgradeBundle>().FixLight();
                }
                else
                {
                    DebugLogger.LogError($"-- MoreUpgradesCompat did not find bundle item", true);
                }
            }
            */
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void CallUpgrade(string upgradeItemName, string steamId, int amount = 1)
        {
            MoreUpgradesManager.instance.Upgrade(upgradeItemName, steamId, amount);
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
        /// <summary>
        /// Get a material from a renderer component so we can read and copy it's info
        /// </summary>
        /// <param name="upgradeItemName"></param>
        /// <returns>First material with the string "upgrade" in its name</returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static Material? GetBoxMat(string upgradeItemName)
        {
            var pluginInstance = MoreUpgrades.Plugin.instance;
            Item obj = pluginInstance.assetBundle.LoadAsset<Item>(upgradeItemName);
            if ( obj == null )
            {
                DebugLogger.LogError($"- GetBoxMat {upgradeItemName} failed", true);
                return null;
            }

            var mat = obj.prefab.GetComponentInChildren<MeshRenderer>().materials[0];
            var meshFilters = obj.prefab.GetComponentsInChildren<MeshFilter>();
            foreach ( MeshFilter meshF in meshFilters)
            {
                var meshR = meshF.GetComponent<MeshRenderer>();
                if (meshR != null)
                {
                    if ( meshR.materials[0].name.Contains("upgrade") )
                    {
                        mat = meshR.materials[0];
                        break;
                    }
                }
            }

            return mat;
        }

        /// <summary>
        /// Get a light component so we can read and copy it's info
        /// </summary>
        /// <param name="upgradeItemName"></param>
        /// <returns> Light Component on the gameObject "Light - Small Lamp"</returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static Light? GetLight(string upgradeItemName)
        {
            var pluginInstance = MoreUpgrades.Plugin.instance;
            Item obj = pluginInstance.assetBundle.LoadAsset<Item>(upgradeItemName);
            if (obj == null)
            {
                DebugLogger.LogError($"- GetLight {upgradeItemName} failed", true);
                return null;
            }

            var lightObj = obj.prefab.transform.Find("Light - Small Lamp");
            if (lightObj == null)
            {
                return null;
            }
            else
            {
                var light = lightObj.GetComponent<Light>();
                return light;
            }
        }
    }
}
