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
        public bool isPrefab;

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
                isPrefab = true;
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
                isPrefab = false;
            }

            FixMaterial();
            FixLight();
        }

        public static bool IsPrefabStage()
        {
            if (!ItemBundles.Instance.mainMenuReached || SemiFunc.MenuLevel()) return true;
            else return RunManager.instance == null;
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
            var playerCount = SemiFunc.PlayerGetAll().Count;
            playerCount = Mathf.Max(playerCount, BundleHelper.GetItemBundleMinItem(originalItem));

            for (int i = 0; i < (playerCount + ItemBundles.Instance.config_debugFakePlayers.Value); i++)
            {
                var boxThicknessOffset = 0.075f;
                var spawnOffset = -transform.forward * boxThicknessOffset * (i - 1);

                if ( !SemiFunc.IsMultiplayer() )
                {
                    var obj = Object.Instantiate(originalItem.prefab, base.transform.position + spawnOffset, gameObject.transform.rotation);
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.itemAssetName);
                }
                if ( SemiFunc.IsMasterClient() )
                {
                    GameObject obj = PhotonNetwork.Instantiate("Items/" + originalItem.prefab.name, base.transform.position + spawnOffset, gameObject.transform.rotation);
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.itemAssetName);
                }
            }
        }


        public void SetMesh( Mesh mesh )
        {
            if (!mesh) return;
            var meshObj = gameObject.transform.Find("Mesh");
            var meshF = meshObj.GetComponent<MeshFilter>();
            meshF.mesh = mesh;
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
