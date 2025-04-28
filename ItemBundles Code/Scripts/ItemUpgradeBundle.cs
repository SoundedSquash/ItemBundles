using UnityEngine;

namespace ItemBundles
{
    public class ItemUpgradeBundle : MonoBehaviour
    {
        public Item originalItem;

        private void Start()
        {
        }

        public void Upgrade()
        {
            if (originalItem == null)
            {
                DebugLogger.LogError($"originalItem is NULL! Either item was not assigned or was not found!");
                return;
            }

            if ( VanillaUpgradesCompat.enabled )
            {
                VanillaUpgradesCompat.CallBundleUpgrade(originalItem.itemAssetName);
            }
            else
            {
                BundleHelper.CallBundleUpgrade(originalItem.itemAssetName);
            }
        }
    }
}
