using Photon.Pun;
using UnityEngine;
using static SemiFunc;

namespace ItemBundles
{
    public class ItemConsumableBundle : MonoBehaviour
    {
        private ItemToggle itemToggle;

        private PhotonView photonView;
        private PhysGrabObjectImpactDetector impactDetector;

        //What item should we spawn?
        public GameObject itemPrefab;
        private bool used;

        private void Start()
        {
            itemToggle = GetComponent<ItemToggle>();
            photonView = GetComponent<PhotonView>();
            impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
        }

        private void Update()
        {
            if (SemiFunc.RunIsShop())
            {
                return;
            }

            if (!SemiFunc.IsMasterClientOrSingleplayer() || !itemToggle.toggleState || used )
            {
                return;
            }

            if ( !used && itemToggle.toggleState )
            {
                SpawnItems();

                StatsManager.instance.ItemRemove(this.GetComponent<ItemAttributes>().instanceName);

                impactDetector.destroyDisable = false;
                impactDetector.DestroyObject(effects: false);
                used = true;
            }
        }

        public void SpawnItems()
        {
            //TODO: Adjust item spacing more dynamically
            var playerCount = SemiFunc.PlayerGetAll().Count;

            var item = GetComponent<ItemAttributes>().item;
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

            if (!SemiFunc.IsMultiplayer())
            {
                for (int i = 0; i < (playerCount + ItemBundles.Instance.config_debugFakePlayers.Value); i++)
                {
                    //Makes first item spawn in hand
                    var randomSpawnOffset = Vector3.zero;
                    if (i != 0)
                    {
                        randomSpawnOffset = Random.insideUnitSphere * offsetMult;
                    }

                    var obj = Object.Instantiate(itemPrefab, base.transform.position + randomSpawnOffset, Quaternion.identity);
                    obj.AddComponent<ItemLateImpulse>();
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.itemAssetName);
                }
            }
            else if (SemiFunc.IsMasterClient())
            {
                for (int j = 0; j < (playerCount + ItemBundles.Instance.config_debugFakePlayers.Value); j++)
                {
                    //Makes first item spawn in hand
                    var randomSpawnOffset = Vector3.zero;
                    if ( j != 0 )
                    {
                        randomSpawnOffset = Random.insideUnitSphere * offsetMult;
                    }

                    GameObject obj = PhotonNetwork.Instantiate("Items/" + itemPrefab.name, base.transform.position + randomSpawnOffset, Quaternion.identity, 0);
                    obj.AddComponent<ItemLateImpulse>();
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.itemAssetName);
                }
            }
        }
    }
}
