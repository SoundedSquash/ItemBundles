using Photon.Pun;
using UnityEngine;
using static SemiFunc;

namespace ItemBundles
{
    public class ItemConsumableBundle : MonoBehaviour
    {
        private ItemToggle itemToggle;

        private PhotonView photonView;
        private PhysGrabObject physGrabObject;
        private PhysGrabObjectImpactDetector impactDetector;
        public ItemBundleShopPrompter shopPrompter;
        
		private Vector3 startPosition;
		private Quaternion startRotation;
        //What item should we spawn?
        public GameObject itemPrefab;
        private bool used;

        private void Start()
        {
            itemToggle = GetComponent<ItemToggle>();
            photonView = GetComponent<PhotonView>();
            physGrabObject = GetComponent<PhysGrabObject>();
            impactDetector = GetComponent<PhysGrabObjectImpactDetector>();

            startPosition = transform.position;
            startRotation = transform.rotation;

            if (!TryGetComponent<ItemBundleShopPrompter>(out shopPrompter))
            {
                shopPrompter = gameObject.AddComponent<ItemBundleShopPrompter>();
            }
        }

        private void Update()
        {
            if (physGrabObject.playerGrabbing.Count == 0 && shopPrompter.shopConfirm == true)
            {
                shopPrompter.shopConfirm = false;
            }

            if ( !itemToggle.toggleState || used )
            {
                return;
            }

            if (!used && itemToggle.toggleState)
            {
                if (SemiFunc.RunIsShop())
                {
                    if (!shopPrompter.shopConfirm)
                    {
                        shopPrompter.shopConfirm = true;
                        itemToggle.ToggleItem(false);
                        return;
                    }
                }

                if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

                SpawnItems();

                if ( !SemiFunc.RunIsShop() )
                {
                    StatsManager.instance.ItemRemove(this.GetComponent<ItemAttributes>().instanceName);
                }

                impactDetector.destroyDisable = false;
                impactDetector.DestroyObject(effects: false);
                used = true;
            }
        }

        public void SpawnItems()
        {
            //TODO: Adjust item spacing more dynamically
            var playerCount = SemiFunc.PlayerGetAll().Count + ItemBundles.Instance.config_debugFakePlayers.Value;

            Item item = GetComponent<ItemAttributes>().item;
            if (item.itemType == itemType.grenade || item.itemType == itemType.mine)
            {
                playerCount = Mathf.Max(playerCount, BundleHelper.GetItemBundleMinItem(BundleHelper.GetItemStringFromBundle(item), item.itemType));
            }

            float offsetMult;
            switch ( item.itemType )
            {
                case itemType.grenade:
                    offsetMult = 0.25f;
                    break;
                case itemType.mine:
                    offsetMult = 0.5f;
                    break;
                default:
                    offsetMult = 0f; 
                    break;
            }

            for (int i = 0; i < playerCount; i++)
            {
                GameObject? obj = null;
                Vector3 randomSpawnOffset = i == 0 ? Vector3.zero : Random.insideUnitSphere * offsetMult;

                if (!SemiFunc.IsMultiplayer())
                {
                    obj = Object.Instantiate(itemPrefab, base.transform.position + randomSpawnOffset, Quaternion.identity);
                }
                else if (SemiFunc.IsMasterClient())
                {
                    obj = PhotonNetwork.Instantiate("Items/" + itemPrefab.name, base.transform.position + randomSpawnOffset, Quaternion.identity, 0);
                }

                if (obj == null) return;

                obj.AddComponent<ItemLateImpulse>();
                var lateStartSet = obj.AddComponent<ItemLateStartSetter>();
                lateStartSet.newStartPosition = startPosition;
                lateStartSet.newStartRotation = startRotation;
                lateStartSet.enable = true;

                if (SemiFunc.IsMasterClient() || !SemiFunc.IsMultiplayer())
                {
                    if (!SemiFunc.RunIsShop())
                        StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.prefab.prefabName);
                }
            }
        }
    }
}
