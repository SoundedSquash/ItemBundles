using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using MoreUpgrades.Classes;
using Photon.Pun;
using Steamworks.Ugc;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace ItemBundles
{
    [BepInPlugin("SeroRonin.ItemBundles", "ItemBundles", "1.4.0")]
    [BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("nickklmao.repoconfig", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("bulletbot.moreupgrades", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("bulletbot.vanillaupgrades", BepInDependency.DependencyFlags.SoftDependency)]
    public class ItemBundles : BaseUnityPlugin
    {
        public static ItemBundles Instance { get; private set; } = null!;
        internal Harmony? Harmony { get; set; }

        public AssetBundle assetBundle;

        public Dictionary<string, Item> itemDictionaryShop = new Dictionary<string, Item>();
        public Dictionary<string, Item> itemDictionaryShopBlacklist = new Dictionary<string, Item>();

        public ConfigEntry<bool> config_disableBundlesSP { get; private set; }
        public ConfigEntry<int> config_chanceBundlesInShop { get; private set; }
        public ConfigEntry<int> config_maxBundlesInShop { get; private set; }
        public ConfigEntry<int> config_minPerBundle { get; private set; }
        public ConfigEntry<float> config_priceMultiplier { get; private set; }
        public ConfigEntry<int> config_debugFakePlayers { get; private set; }
        public ConfigEntry<bool> config_debugLogging { get; private set; }

        public Dictionary<SemiFunc.itemType, BundleShopInfo> itemTypeBundleInfos = new Dictionary<SemiFunc.itemType, BundleShopInfo>();
        public Dictionary<string, BundleShopInfo> itemBundleInfos = new Dictionary<string, BundleShopInfo>();
        public Dictionary<Item, GameObject> generatedBundles = new Dictionary<Item, GameObject>();

        public class BundleShopInfo
        {
            public Item bundleItem;
            public int chanceInShop;
            /// <summary>
            /// Used to track how many are in current shop list
            /// Resets every shop cycle
            /// </summary>
            public int maxInShop;
            public ConfigEntry<int> config_chanceInShop;
            public ConfigEntry<int> config_maxInShop;
            public ConfigEntry<int> config_minPerBundle;
            public ConfigEntry<float> config_priceMultiplier;
        }

        private void Awake()
        {
            // Should not have more than one instance
            if (Instance) return;

            Instance = this;
            DebugLogger.Init(base.Logger);
            
            string pluginFolderPath = Path.GetDirectoryName(Info.Location);
            string assetBundleFilePath = Path.Combine(pluginFolderPath, "itembundles");
            assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            // Prevent the plugin from being deleted
            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            CreateConfigs();

            Patch();

            // Create singleton GO to store each generated bundle 'prefab'
            GameObject manager = new GameObject("Bundle Manager");
            manager.AddComponent<BundleManager>();
            manager.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(manager);

            RegisterItemBundles();

            if ( MoreUpgradesCompat.enabled )
            {
                MoreUpgradesCompat.InitCompat();
                DebugLogger.LogInfo($"MoreUpgradesCompat has loaded!", true);
            }
            if (VanillaUpgradesCompat.enabled)
            {
                VanillaUpgradesCompat.InitCompat();
                DebugLogger.LogInfo($"VanillaUpgradesCompat has loaded!", true);
            }

            DebugLogger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
        }


        public void RegisterItemBundles()
        {
            if (assetBundle == null)
            {
                DebugLogger.LogError($"Assetbundle \"itembundles\" not found! Please make sure that it exists in the same folder as the mod DLL");
                DebugLogger.LogError($"ItemBundles has run into a fatal error! The mod will not work correctly and may cause issues elsewhere!");
                return;
            }

            RegisterBundleItemRepoLib(assetBundle, "Item Upgrade Map Player Count Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Upgrade Player Energy Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Upgrade Player Extra Jump Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Upgrade Player Grab Range Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Upgrade Player Grab Strength Bundle");
            //RegisterBundleItem(assetBundle, "Item Upgrade Player Grab Throw Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Upgrade Player Health Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Upgrade Player Sprint Speed Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Upgrade Player Tumble Launch Bundle");

            RegisterBundleItemRepoLib(assetBundle, "Item Health Pack Small Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Health Pack Medium Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Health Pack Large Bundle");

            RegisterBundleItemRepoLib(assetBundle, "Item Grenade Explosive Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Grenade Shockwave Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Grenade Stun Bundle");

            RegisterBundleItemRepoLib(assetBundle, "Item Mine Explosive Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Mine Shockwave Bundle");
            RegisterBundleItemRepoLib(assetBundle, "Item Mine Stun Bundle");


            var tempBundleItem = assetBundle.LoadAsset<Item>("Item Upgrade Generated Bundle");
            var tempBundlePrefab = tempBundleItem.prefab;

            if (tempBundleItem == null )
            {
                DebugLogger.LogError($"tempBundleItem was null! Unable to generate custom upgrade bundles");
                DebugLogger.LogError($"ItemBundles has run into a fatal error! The mod will not work correctly and may cause issues elsewhere!");
                return;
            }

            if (tempBundlePrefab == null)
            {
                DebugLogger.LogError($"tempBundlePrefab was null! Unable to generate custom upgrade bundles");
                DebugLogger.LogError($"ItemBundles has run into a fatal error! The mod will not work correctly and may cause issues elsewhere!");
                return;
            }

            // Generated Bundles
            foreach (REPOLib.Modules.PlayerUpgrade upgradeEntry in REPOLib.Modules.Upgrades.PlayerUpgrades)
            {
                var upgradeItem = upgradeEntry.Item;
                var upgradePrefab = upgradeItem.prefab;

                // Duplicate template item and prefab
                // var newBundleItem = ScriptableObject.Instantiate(tempBundleItem);
                var newBundleItem = ScriptableObject.CreateInstance<Item>();
                var newBundlePrefab = GameObject.Instantiate(tempBundleItem.prefab);

                // Assign necessary values onto new item
                newBundlePrefab.name = upgradeItem.name + " Bundle";
                newBundleItem.name = upgradeItem.name + " Bundle";
                newBundleItem.itemAssetName = upgradeItem.itemAssetName + " Bundle";
                newBundleItem.itemName = upgradeItem.itemName + "s";
                newBundleItem.itemType = upgradeItem.itemType;
                newBundleItem.emojiIcon = upgradeItem.emojiIcon;
                newBundleItem.itemVolume = upgradeItem.itemVolume;
                newBundleItem.itemSecretShopType = upgradeItem.itemSecretShopType;
                newBundleItem.colorPreset = upgradeItem.colorPreset;
                newBundleItem.prefab = newBundlePrefab;
                newBundleItem.value = upgradeItem.value;
                newBundleItem.maxAmount = upgradeItem.maxAmount;
                newBundleItem.maxAmountInShop = upgradeItem.maxAmountInShop;
                newBundleItem.maxPurchase = upgradeItem.maxPurchase;
                newBundleItem.maxPurchaseAmount = upgradeItem.maxPurchaseAmount;
                newBundleItem.spawnRotationOffset = upgradeItem.spawnRotationOffset;
                newBundleItem.physicalItem = upgradeItem.physicalItem;

                var itemComp = newBundlePrefab.GetComponent<ItemAttributes>();
                itemComp.item = newBundleItem;

                var bundleComp = newBundlePrefab.GetComponent<ItemUpgradeBundleGenerated>();
                bundleComp.originalItem = upgradeItem;

                // Register Item
                REPOLib.Modules.Items.RegisterItem(newBundleItem);
                generatedBundles[newBundleItem] = newBundlePrefab;
            }
        }

        public void InitializeItemBundles()
        {
            if (assetBundle == null)
            {
                DebugLogger.LogError($"Assetbundle \"itembundles\" not found! Please make sure that it exists in the same folder as the mod DLL");
                DebugLogger.LogError($"ItemBundles has run into a fatal error! The mod will not work correctly and may cause issues elsewhere!");
                return;
            }

            InitializeBundle(assetBundle, "Item Upgrade Map Player Count Bundle");
            InitializeBundle(assetBundle, "Item Upgrade Player Energy Bundle");
            InitializeBundle(assetBundle, "Item Upgrade Player Extra Jump Bundle");
            InitializeBundle(assetBundle, "Item Upgrade Player Grab Range Bundle");
            InitializeBundle(assetBundle, "Item Upgrade Player Grab Strength Bundle");
            //RegisterBundleItem(assetBundle, "Item Upgrade Player Grab Throw Bundle");
            InitializeBundle(assetBundle, "Item Upgrade Player Health Bundle");
            InitializeBundle(assetBundle, "Item Upgrade Player Sprint Speed Bundle");
            InitializeBundle(assetBundle, "Item Upgrade Player Tumble Launch Bundle");

            InitializeBundle(assetBundle, "Item Health Pack Small Bundle");
            InitializeBundle(assetBundle, "Item Health Pack Medium Bundle");
            InitializeBundle(assetBundle, "Item Health Pack Large Bundle");

            InitializeBundle(assetBundle, "Item Grenade Explosive Bundle");
            InitializeBundle(assetBundle, "Item Grenade Shockwave Bundle");
            InitializeBundle(assetBundle, "Item Grenade Stun Bundle");

            InitializeBundle(assetBundle, "Item Mine Explosive Bundle");
            InitializeBundle(assetBundle, "Item Mine Shockwave Bundle");
            InitializeBundle(assetBundle, "Item Mine Stun Bundle");

            foreach ( Item item in generatedBundles.Keys )
            {
                InitializeBundle(item, "Generated ");
            }

            if ( MoreUpgradesCompat.enabled )
            {
                InitializeBundle(assetBundle, "Modded Item Upgrade Player Map Enemy Tracker Bundle", "MoreUpgrades ");
                InitializeBundle(assetBundle, "Modded Item Upgrade Player Map Player Tracker Bundle", "MoreUpgrades ");
                InitializeBundle(assetBundle, "Modded Item Upgrade Player Sprint Usage Bundle", "MoreUpgrades ");
                InitializeBundle(assetBundle, "Modded Item Upgrade Player Valuable Count Bundle", "MoreUpgrades ");
            }
        }

        public void CreateConfigs()
        {
            //TODO: Re-add max total bundles once I figure out how to not make it stop on first list.
            config_disableBundlesSP = Config.Bind("General", "Disable Bundles in Singleplayer", true, new ConfigDescription("Whether bundles are disabled when doing a singleplayer run"));
            config_chanceBundlesInShop = Config.Bind("General", "Bundle Chance", 20, new ConfigDescription("Percent chance that an item will be replaced with a bundle variant", new AcceptableValueRange<int>(0, 100)));
            config_maxBundlesInShop = Config.Bind("General", "Maximum Bundles In Shop", -1, new ConfigDescription("Maximum number of bundles that can appear of ANY one type. Setting to -1 makes shop ignore this entry", new AcceptableValueRange<int>(-1, 10)));
            config_minPerBundle = Config.Bind("General", "Mininum consumables per bundle", 0, new ConfigDescription("Minimum amount of items in valid bundles. Price still scales. Default: 0", new AcceptableValueRange<int>(0, 10)));
            config_priceMultiplier = Config.Bind("General", "Bundle Price Multiplier", 66.66f, new ConfigDescription("Multiplier of total item costs that bundles have", new AcceptableValueRange<float>(0f, 200f)));

            string overrideDesc = "Has Priority over General entry. Ignored if set below 0";
            itemTypeBundleInfos[SemiFunc.itemType.mine] = new BundleShopInfo
            {
                config_chanceInShop = Config.Bind("Bundles: Item Type", "Mines: Chance", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 100))),
                config_maxInShop = Config.Bind("Bundles: Item Type", "Mines: Chance", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 10))),
                config_minPerBundle = Config.Bind("Bundles: Item Type", "Mines: Mininum per bundle", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 10))),
                config_priceMultiplier = Config.Bind("Bundles: Item Type", "Mines: Price Multiplier", -1f, new ConfigDescription(overrideDesc, new AcceptableValueRange<float>(-1f, 200f)))
            };
            itemTypeBundleInfos[SemiFunc.itemType.grenade] = new BundleShopInfo
            {
                config_chanceInShop = Config.Bind("Bundles: Item Type", "Grenades: Chance", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 100))),
                config_maxInShop = Config.Bind("Bundles: Item Type", "Grenades: Max", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 10))),
                config_minPerBundle = Config.Bind("Bundles: Item Type", "Grenades: Mininum per bundle", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 10))),
                config_priceMultiplier = Config.Bind("Bundles: Item Type", "Grenades: Price Multiplier", -1f, new ConfigDescription(overrideDesc, new AcceptableValueRange<float>(-1f, 200f)))
            };
            itemTypeBundleInfos[SemiFunc.itemType.healthPack] = new BundleShopInfo
            {
                config_chanceInShop = Config.Bind("Bundles: Item Type", "Health Packs: Chance", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 100))),
                config_maxInShop = Config.Bind("Bundles: Item Type", "Health Packs: Max", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 10))),
                config_priceMultiplier = Config.Bind("Bundles: Item Type", "Health Packs: Price Multiplier", -1f, new ConfigDescription(overrideDesc, new AcceptableValueRange<float>(-1f, 200f)))
            };
            itemTypeBundleInfos[SemiFunc.itemType.item_upgrade] = new BundleShopInfo
            {
                config_chanceInShop = Config.Bind("Bundles: Item Type", "Upgrades: Chance", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 100))),
                config_maxInShop = Config.Bind("Bundles: Item Type", "Upgrades: Max", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 10))),
                config_minPerBundle = Config.Bind("Bundles: Item Type", "Upgrades: Mininum per bundle", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 10))),
                config_priceMultiplier = Config.Bind("Bundles: Item Type", "Upgrades: Price Multiplier", -1f, new ConfigDescription(overrideDesc, new AcceptableValueRange<float>(-1f, 200f)))
            };

            config_debugLogging = Config.Bind("Dev", "Debug Logging", false, new ConfigDescription("Enables debug logging", tags: "HideFromREPOConfig"));
            config_debugFakePlayers = Config.Bind("Dev", "Number of Fake Players", 0, new ConfigDescription("Adds fake players to bundle player calculations", new AcceptableValueRange<int>(0, 10), "HideFromREPOConfig"));
        }

        internal void RegisterBundleItemRepoLib(AssetBundle assetBundle, string itemString)
        {
            Item item = assetBundle.LoadAsset<Item>(itemString);
            if (item == null)
            {
                DebugLogger.LogError($"Item {itemString} not found!");
                return;
            }

            REPOLib.Modules.Items.RegisterItem(item);
        }

        internal void InitializeBundle(AssetBundle assetBundle, string bundleItemString, string configSectionPrefix = "")
        {
            Item bundleItem = assetBundle.LoadAsset<Item>(bundleItemString);
            if (bundleItem == null)
            {
                DebugLogger.LogError($"--- Bundle Item \"{bundleItemString}\" not found!");
                return;
            }

            InitializeBundle(bundleItem, configSectionPrefix);
        }

        internal void InitializeBundle( Item item, string configSectionPrefix = "")
        {
            Item bundleItem = item;
            if (bundleItem == null)
            {
                DebugLogger.LogError($"--- Bundle Item not found!");
                return;
            }

            var bundleString = " Bundle";
            if (!item.itemAssetName.Contains(bundleString))
            {
                DebugLogger.LogError($"--- Item {item.itemAssetName} is not a bundle! Add \" Bundle\" to item name (WITH THE SPACE)");
                return;
            }

            var originalItemString = BundleHelper.GetItemStringFromBundle(bundleItem);
            var originalItem = StatsManager.instance.itemDictionary[originalItemString];

            if (!originalItem)
            {
                DebugLogger.LogError($"--- Didn't find {originalItemString}! Make sure itemAssetName of bundle Item and bundle Prefab is {originalItemString + bundleString}");
                return;
            }

            if (itemBundleInfos.ContainsKey(originalItemString))
            {
                DebugLogger.LogWarning($"--- bundleStringPairs {originalItemString} already has an entry {itemBundleInfos[originalItemString]}, we are overriding something!");
            }

            itemDictionaryShopBlacklist.Add(bundleItem.itemAssetName, bundleItem);

            // FAILSAFE: Update bundle upgrade prices to match original upgrades
            if (bundleItem.itemType == SemiFunc.itemType.item_upgrade)
            {
                bundleItem.value = ScriptableObject.CreateInstance<Value>();
                bundleItem.value.valueMin = originalItem.value.valueMin;
                bundleItem.value.valueMax = originalItem.value.valueMax;
            }

            string overrideDesc = "Has Priority over Item Type entry. Ignored if set below 0";
            var bundleInfo = new BundleShopInfo
            {
                bundleItem = bundleItem,
                config_chanceInShop = Config.Bind($"{configSectionPrefix}Bundles: Item", $"{originalItem.itemName}: Chance", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 100))),
                config_maxInShop = Config.Bind($"{configSectionPrefix}Bundles: Item", $"{originalItem.itemName}: Max", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 10))),
                config_priceMultiplier = Config.Bind($"{configSectionPrefix}Bundles: Item", $"{originalItem.itemName}: Price Multiplier", -1f, new ConfigDescription(overrideDesc, new AcceptableValueRange<float>(-1f, 200f)))
            };

            if (bundleItem.itemType == SemiFunc.itemType.grenade || bundleItem.itemType == SemiFunc.itemType.mine || bundleItem.itemType == SemiFunc.itemType.item_upgrade)
            {
                bundleInfo.config_minPerBundle = Config.Bind($"{configSectionPrefix}Bundles: Item", $"{originalItem.itemName}: Mininum per bundle", -1, new ConfigDescription(overrideDesc, new AcceptableValueRange<int>(-1, 10)));
            }

            itemBundleInfos[originalItemString] = bundleInfo;
            DebugLogger.LogInfo($"--- Added bundleInfo {{ {originalItemString} | {bundleItem.itemAssetName} }}", true);

        }

        internal void Patch()
        {
            Harmony ??= new Harmony(Info.Metadata.GUID);
            Harmony.PatchAll();
        }

        internal void Unpatch()
        {
            Harmony?.UnpatchSelf();
        }

        private void Update()
        {
            // Code that runs every frame goes here
        }
    }
}