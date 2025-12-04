using Steamworks.Ugc;
using System.Collections.Generic;
using UnityEngine;
using static SemiFunc;

namespace ItemBundles
{
    public static class BundleHelper
    {
        public static int GetItemBundleChance(Item item)
        {
            var itemTypeChecked = ValidateItemType(item);
            var output = ItemBundles.Instance.itemTypeBundleInfos[itemTypeChecked].chanceInShop;
            if (ItemBundles.Instance.itemBundleInfos[item.prefab.prefabName].chanceInShop >= 0)
            {
                output = ItemBundles.Instance.itemBundleInfos[item.prefab.prefabName].chanceInShop;
            }

            return output;
        }

        public static int GetItemBundleMax(Item item)
        {
            var itemTypeChecked = ValidateItemType(item);
            var output = ItemBundles.Instance.itemTypeBundleInfos[itemTypeChecked].maxInShop;
            if (ItemBundles.Instance.itemBundleInfos[item.prefab.prefabName].maxInShop >= 0)
            {
                output = ItemBundles.Instance.itemBundleInfos[item.prefab.prefabName].maxInShop;
            }

            return output;
        }

        public static int GetItemBundleMinItem(Item item)
        {
            var itemTypeChecked = ValidateItemType(item);
            var output = ItemBundles.Instance.config_minPerBundle.Value;
            if (ItemBundles.Instance.itemTypeBundleInfos[itemTypeChecked].config_minPerBundle.Value >= 0)
            {
                output = ItemBundles.Instance.itemTypeBundleInfos[itemTypeChecked].config_minPerBundle.Value;
            }
            if ( ItemBundles.Instance.itemBundleInfos[item.prefab.prefabName].config_minPerBundle.Value >= 0)
            {
                output = ItemBundles.Instance.itemBundleInfos[item.prefab.prefabName].config_minPerBundle.Value;
            }

            return output;
        }

        public static int GetItemBundleMinItem(string itemString, SemiFunc.itemType itemType)
        {
            var itemTypeChecked = ValidateItemType(itemType);
            var output = ItemBundles.Instance.config_minPerBundle.Value;
            if (ItemBundles.Instance.itemTypeBundleInfos[itemTypeChecked].config_minPerBundle.Value >= 0)
            {
                output = ItemBundles.Instance.itemTypeBundleInfos[itemTypeChecked].config_minPerBundle.Value;
            }
            if (ItemBundles.Instance.itemBundleInfos[itemString].config_minPerBundle.Value >= 0)
            {
                output = ItemBundles.Instance.itemBundleInfos[itemString].config_minPerBundle.Value;
            }

            return output;
        }

        public static float GetItemBundlePriceMult(string itemString, SemiFunc.itemType itemType)
        {
            var itemTypeChecked = ValidateItemType(itemType);
            var output = ItemBundles.Instance.config_priceMultiplier.Value;
            if (ItemBundles.Instance.itemTypeBundleInfos[itemTypeChecked].config_priceMultiplier.Value >= 0)
            {
                output = ItemBundles.Instance.itemTypeBundleInfos[itemTypeChecked].config_priceMultiplier.Value;
            }
            if (ItemBundles.Instance.itemBundleInfos[itemString].config_priceMultiplier.Value >= 0)
            {
                output = ItemBundles.Instance.itemBundleInfos[itemString].config_priceMultiplier.Value;
            }

            return output;
        }

        /// <summary>
        /// Failsafe patch that overrides any player_upgrade itemTypes to item_upgrade.
        /// As far as I know, player_upgrade isn't actually used anywhere
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static itemType ValidateItemType(Item item)
        {
            return item.itemType == itemType.player_upgrade ? itemType.item_upgrade : item.itemType;
        }

        public static itemType ValidateItemType(itemType itemType)
        {
            return itemType == itemType.player_upgrade ? itemType.item_upgrade : itemType;
        }

        public static string GetItemStringFromBundle( Item bundleItem )
        {
            string bundleItemString = bundleItem.prefab.prefabName;
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

        public static bool SceneIsPrefabStage()
        {
            if (!ItemBundles.Instance.mainMenuReached || SemiFunc.MenuLevel()) return true;
            else return RunManager.instance == null;
        }
    }
}
