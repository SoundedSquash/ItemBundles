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

                if ( obj == null ) return;

                obj.AddComponent<ItemLateImpulse>();
                StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.prefab.prefabName);
            }
            
            /* Old Code, remove after one revision
            if (!SemiFunc.IsMultiplayer())
            {
                for (int i = 0; i < playerCount; i++)
                {
                    Vector3 randomSpawnOffset = i == 0 ? Vector3.zero : Random.insideUnitSphere * offsetMult;

                    var obj = Object.Instantiate(itemPrefab, base.transform.position + randomSpawnOffset, Quaternion.identity);
                    obj.AddComponent<ItemLateImpulse>();
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.prefab.prefabName);
                }
            }
            else if (SemiFunc.IsMasterClient())
            {
                for (int j = 0; j < playerCount; j++)
                {
                    Vector3 randomSpawnOffset = j == 0 ? Vector3.zero : Random.insideUnitSphere * offsetMult;

                    GameObject obj = PhotonNetwork.Instantiate("Items/" + itemPrefab.name, base.transform.position + randomSpawnOffset, Quaternion.identity, 0);
                    obj.AddComponent<ItemLateImpulse>();
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.prefab.prefabName);
                }
            }
            */
        }
    }
}
