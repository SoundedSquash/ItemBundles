using Photon.Pun;
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

            SpawnItems();
        }

        public void SpawnItems()
        {
            //TODO: Add velocity to items
            //TODO: Adjust item spacing dyanmically
            var playerCount = SemiFunc.PlayerGetAll().Count;
            playerCount = Mathf.Max(playerCount, BundleHelper.GetItemBundleMinItem(originalItem));

            float offsetMult = 0.5f;
            float velMult = 2.5f;

            for (int i = 0; i < (playerCount + ItemBundles.Instance.config_debugFakePlayers.Value); i++)
            {
                //Makes first item spawn in hand
                var randomSpawnOffset = Vector3.zero;
                if (i != 0)
                {
                    randomSpawnOffset = Random.insideUnitSphere * offsetMult;
                }

                // SP, direct instantiate
                if (!SemiFunc.IsMultiplayer())
                {
                    var obj = Object.Instantiate(originalItem.prefab, base.transform.position + randomSpawnOffset, Quaternion.identity);
                    obj.AddComponent<ItemLateImpulse>();
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.itemAssetName);
                }
                // MP, server host network instantiate
                if (SemiFunc.IsMasterClient())
                {
                    GameObject obj = PhotonNetwork.Instantiate("Items/" + originalItem.prefab.name, base.transform.position + randomSpawnOffset, Quaternion.identity, 0);
                    obj.AddComponent<ItemLateImpulse>();
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.itemAssetName);
                }
            }

            //particleScriptExplosion.Spawn(base.transform.position, 0.8f, 50, 100, 4f, onlyParticleEffect: false, disableSound: true);
            //soundExplosion.Play(base.transform.position);
            //soundExplosionGlobal.Play(base.transform.position);
        }
    }
}
