using System.Runtime.CompilerServices;

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
                    //_enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("bulletbot.vanillaupgrades");
                    _enabled = false;
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static float GetUpgradeValueIncrease(float fallbackMult, string bundleAssetName)
        {
            var itemAssetName = BundleHelper.GetItemStringFromBundle(bundleAssetName);
            return VanillaUpgrades.Plugin.UpgradeValueIncrease(fallbackMult, itemAssetName); ;
        }
    }
}
