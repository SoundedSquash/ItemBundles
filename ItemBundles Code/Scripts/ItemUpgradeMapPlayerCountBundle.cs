using UnityEngine;

namespace ItemBundles
{
    public class ItemUpgradeMapPlayerCountBundle : MonoBehaviour
    {
        private ItemToggle itemToggle;

        private void Start()
        {
            itemToggle = GetComponent<ItemToggle>();
        }

        public void Upgrade()
        {
            if (itemToggle != null && VanillaUpgradesCompat.enabled)
            {
                VanillaUpgradesCompat.UpgradeViaBundle(itemToggle);
            }
            else
            {
                var players = SemiFunc.PlayerGetAll();

                foreach (var player in players)
                {
                    PunManager.instance.UpgradeMapPlayerCount(SemiFunc.PlayerGetSteamID(player));
                }
            }
        }
    }
}
