using MoreUpgrades.Classes;
using Photon.Pun;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using static SemiFunc;

namespace ItemBundles
{
    public class ItemUpgradeBundleGenerated : MonoBehaviour
    {
        private ItemToggle itemToggle;

        private PhotonView photonView;
        private PhysGrabObjectImpactDetector impactDetector;

        //What item should we spawn?
        public Item originalItem;
        private bool used;

        private void Awake()
        {
            itemToggle = GetComponent<ItemToggle>();
            photonView = GetComponent<PhotonView>();
            impactDetector = GetComponent<PhysGrabObjectImpactDetector>();

            // This specifically prevents the intiial prefab from being deleted or falling through the scene
            if ( IsPrefabStage() )
            {
                DebugLogger.LogWarning("========= GENERATED BUNDLE: PREFAB PHASE Awake", true);
                impactDetector.destroyDisable = true;
                this.transform.parent = BundleManager.instance.transform;
                var rb = GetComponent<Rigidbody>();
                rb.isKinematic = true;
            }
            else
            {
                DebugLogger.LogWarning("========= GENERATED BUNDLE: GAME PHASE Awake", true);
                impactDetector.destroyDisable = false;
                var rb = GetComponent<Rigidbody>();
                rb.isKinematic = false;
            }
        }

        private void Start()
        {           
            // This specifically prevents the intiial prefab from being deleted or falling through the scene
            if ( IsPrefabStage() )
            {
                DebugLogger.LogWarning("========= GENERATED BUNDLE: PREFAB PHASE Start", true);
                StartCoroutine(LateStart(0.1f));
            }
            else
            {
                DebugLogger.LogWarning("========= GENERATED BUNDLE: GAME PHASE Start", true);
                var rb = GetComponent<Rigidbody>();
                rb.isKinematic = false;
            }

            FixMaterial();
            FixLight();
        }

        IEnumerator LateStart(float waitTime)
        {
            DebugLogger.LogWarning("========= GENERATED BUNDLE: LateStart start", true);
            yield return new WaitForSeconds(waitTime);
            DebugLogger.LogWarning("========= GENERATED BUNDLE: LateStart end", true);
            this.transform.parent = BundleManager.instance.transform;
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.position = Vector3.zero;
            rb.rotation = Quaternion.identity;
        }

        public void SpawnItems()
        {
            //TODO: Add velocity to items
            //TODO: Adjust item spacing dyanmically
            var playerCount = SemiFunc.PlayerGetAll().Count;
            playerCount = Mathf.Max(playerCount, BundleHelper.GetItemBundleMinItem(originalItem));

            float offsetMult = 0.5f;
            float velMult = 25f;

            for (int i = 0; i < (playerCount + ItemBundles.Instance.config_debugFakePlayers.Value); i++)
            {
                //Makes first item spawn in hand
                var randomSpawnOffset = Vector3.zero;
                if (i != 0)
                {
                    randomSpawnOffset = Random.insideUnitSphere * offsetMult;
                }

                // SP, direct instantiate
                if ( !SemiFunc.IsMultiplayer() )
                {
                    var obj = Object.Instantiate(originalItem.prefab, base.transform.position + randomSpawnOffset, Quaternion.identity);
                    obj.AddComponent<ItemLateImpulse>();
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.itemAssetName);
                }
                // MP, server host network instantiate
                if ( SemiFunc.IsMasterClient() )
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

        public bool IsPrefabStage()
        {
            return (RunManager.instance == null || RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu || RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu);
        }

        /// <summary>
        /// Get a material from a renderer component so we can read and copy it's info
        /// </summary>
        /// <param name="upgradeItemName"></param>
        /// <returns>First material with the string "upgrade" in its name</returns>
        public Material? GetBoxMat()
        {
            Item obj = originalItem;
            if (obj == null)
            {
                return null;
            }

            Material? mat = null;

            var mesh = obj.prefab.transform.Find("Mesh");
            var meshR = mesh.GetComponent<MeshRenderer>();
            if (meshR != null)
            {
                mat = meshR.materials[0];
            }

            if ( mat == null )
            {
                DebugLogger.LogError($"- GetBoxMat {originalItem.itemAssetName} failed, returning NULL", true);
            }

            return mat;
        }

        /// <summary>
        /// Get a light component so we can read and copy it's info
        /// </summary>
        /// <param name="upgradeItemName"></param>
        /// <returns> Light Component on the gameObject "Light - Small Lamp"</returns>
        public Light? GetLight()
        {
            Item obj = originalItem;
            if (obj == null)
            {
                DebugLogger.LogError($"- GetLight {originalItem.itemAssetName} failed", true);
                return null;
            }

            var lightObj = obj.prefab.transform.Find("Light - Small Lamp");
            if (lightObj == null)
            {
                return null;
            }
            else
            {
                var light = lightObj.GetComponent<Light>();
                return light;
            }
        }

        public void FixMaterial()
        {
            var originalMat = GetBoxMat();
            if (originalMat != null)
            {
                var meshFilters = GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter meshF in meshFilters)
                {
                    var meshR = meshF.GetComponent<MeshRenderer>();
                    if (meshR != null)
                    {
                        if (meshR.materials[0].name.Contains("upgrade"))
                        {
                            Material[] newMaterials = new Material[2] { originalMat, meshR.materials[1] };
                            meshR.materials = newMaterials;
                        }
                    }
                }
            }
        }

        public void FixLight()
        {
            var light = gameObject.transform.Find("Light - Small Lamp").GetComponent<Light>();
            var originalLight = GetLight();
            if (light != null && originalLight != null)
            {

                light.color = originalLight.color;
            }
        }

        public void OnDestroy()
        {
            DebugLogger.LogError("======== DESTROYING GENERATED BUNDLE");
        }
    }
}
