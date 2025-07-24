using System.Collections.Generic;
using UnityEngine;

namespace ItemBundles
{
    public static class BundleHelper
    {
        public static int GetItemBundleChance(Item item)
        {
            var output = ItemBundles.Instance.itemTypeBundleInfos[item.itemType].chanceInShop;
            if (ItemBundles.Instance.itemBundleInfos[item.itemAssetName].chanceInShop >= 0)
            {
                output = ItemBundles.Instance.itemBundleInfos[item.itemAssetName].chanceInShop;
            }

            return output;
        }

        public static int GetItemBundleMax(Item item)
        {
            var output = ItemBundles.Instance.itemTypeBundleInfos[item.itemType].maxInShop;
            if (ItemBundles.Instance.itemBundleInfos[item.itemAssetName].maxInShop >= 0)
            {
                output = ItemBundles.Instance.itemBundleInfos[item.itemAssetName].maxInShop;
            }

            return output;
        }

        public static int GetItemBundleMinItem(Item item)
        {
            var output = ItemBundles.Instance.config_minPerBundle.Value;
            if (ItemBundles.Instance.itemTypeBundleInfos[item.itemType].config_minPerBundle.Value >= 0)
            {
                output = ItemBundles.Instance.itemTypeBundleInfos[item.itemType].config_minPerBundle.Value;
            }
            if ( ItemBundles.Instance.itemBundleInfos[item.itemAssetName].config_minPerBundle.Value >= 0)
            {
                output = ItemBundles.Instance.itemBundleInfos[item.itemAssetName].config_minPerBundle.Value;
            }

            return output;
        }

        public static int GetItemBundleMinItem(string itemString, SemiFunc.itemType itemType)
        {
            var output = ItemBundles.Instance.config_minPerBundle.Value;
            if (ItemBundles.Instance.itemTypeBundleInfos[itemType].config_minPerBundle.Value >= 0)
            {
                output = ItemBundles.Instance.itemTypeBundleInfos[itemType].config_minPerBundle.Value;
            }
            if (ItemBundles.Instance.itemBundleInfos[itemString].config_minPerBundle.Value >= 0)
            {
                output = ItemBundles.Instance.itemBundleInfos[itemString].config_minPerBundle.Value;
            }

            return output;
        }

        public static float GetItemBundlePriceMult(string itemString, SemiFunc.itemType itemType)
        {
            var output = ItemBundles.Instance.config_priceMultiplier.Value;
            if (ItemBundles.Instance.itemTypeBundleInfos[itemType].config_priceMultiplier.Value >= 0)
            {
                output = ItemBundles.Instance.itemTypeBundleInfos[itemType].config_priceMultiplier.Value;
            }
            if (ItemBundles.Instance.itemBundleInfos[itemString].config_priceMultiplier.Value >= 0)
            {
                output = ItemBundles.Instance.itemBundleInfos[itemString].config_priceMultiplier.Value;
            }

            return output;
        }

        public static string GetItemStringFromBundle( Item bundleItem )
        {
            string bundleItemString = bundleItem.itemAssetName;
            return GetItemStringFromBundle( bundleItemString );
        }

        public static string GetItemStringFromBundle(string bundleItemString)
        {
            string bundleString = " Bundle";
            var originalItemString = RemoveString(bundleItemString, bundleString);

            return originalItemString;
        }

        public static string RemoveString(string baseString, string removeString)
        {
            int index = baseString.IndexOf(removeString);
            var newString = (index < 0)
                ? baseString
                : baseString.Remove(index, removeString.Length);

            //CustomLogger.LogInfo($"--- Removing \"{removeString}\" from \"{baseString}\", got  \"{newString}\"", true);
            return newString;
        }

        public static List<PlayerAvatar> PlayerGetAllAlive()
        {
            var playerList = new List<PlayerAvatar>();
            foreach (var player in SemiFunc.PlayerGetAll())
            {
                if ( player.playerHealth.health > 0 )
                {
                    playerList.Add(player);
                }
            }

            return playerList;
        }

        public static bool IsObjectBundlePrefab(GameObject obj)
        {
            var bundleComp = obj.GetComponent<ItemUpgradeBundleGenerated>();
            if (bundleComp)
            {
                return bundleComp.isPrefab;
            }
            else
            {
                return false;
            }
        }

        /* Deprecated, to be purged at a later date
        public static void CallBundleUpgrade( string upgradeName )
        {
            var players = SemiFunc.PlayerGetAll();

            foreach (var player in players)
            {
                switch (upgradeName)
                {
                    case "Item Upgrade Map Player Count":
                        PunManager.instance.UpgradeMapPlayerCount(SemiFunc.PlayerGetSteamID(player));
                        break;
                    case "Item Upgrade Player Energy":
                        PunManager.instance.UpgradePlayerEnergy(SemiFunc.PlayerGetSteamID(player));
                        break;
                    case "Item Upgrade Player Extra Jump":
                        PunManager.instance.UpgradePlayerExtraJump(SemiFunc.PlayerGetSteamID(player));
                        break;
                    case "Item Upgrade Player Grab Range":
                        PunManager.instance.UpgradePlayerGrabRange(SemiFunc.PlayerGetSteamID(player));
                        break;
                    case "Item Upgrade Player Grab Strength":
                        PunManager.instance.UpgradePlayerGrabStrength(SemiFunc.PlayerGetSteamID(player));
                        break;
                    case "Item Upgrade Player Throw Strength":
                        PunManager.instance.UpgradePlayerThrowStrength(SemiFunc.PlayerGetSteamID(player));
                        break;
                    case "Item Upgrade Player Health":
                        PunManager.instance.UpgradePlayerHealth(SemiFunc.PlayerGetSteamID(player));
                        break;
                    case "Item Upgrade Player Sprint Speed":
                        PunManager.instance.UpgradePlayerSprintSpeed(SemiFunc.PlayerGetSteamID(player));
                        break;
                    case "Item Upgrade Player Tumble Launch":
                        PunManager.instance.UpgradePlayerTumbleLaunch(SemiFunc.PlayerGetSteamID(player));
                        break;
                    default:
                        DebugLogger.LogWarning($"{upgradeName} not found in switch statement!");
                        break;
                }
            }
        }
        */
    }
}
