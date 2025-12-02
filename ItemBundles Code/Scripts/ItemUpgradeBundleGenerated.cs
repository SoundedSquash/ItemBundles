using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace ItemBundles
{
    public class ItemUpgradeBundleGenerated : MonoBehaviour
    {
        private ItemToggle itemToggle;
        private bool used;

        private PhotonTransformView photonTransformView;
        private PhysGrabObjectImpactDetector impactDetector;

        //What item should we spawn?
        public Item? originalItem;
        public bool isPrefab;

        private void Awake()
        {
            itemToggle = GetComponent<ItemToggle>();
            photonTransformView = GetComponent<PhotonTransformView>();
            impactDetector = GetComponent<PhysGrabObjectImpactDetector>();

            // This specifically prevents the intial prefab from being deleted or falling through the scene
            if ( BundleHelper.SceneIsPrefabStage() )
            {
                hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(this.gameObject);
                impactDetector.destroyDisable = true;
                this.transform.parent = BundleManager.instance.transform;
                var rb = GetComponent<Rigidbody>();
                rb.isKinematic = true;
                isPrefab = true;
            }
            else
            {
                impactDetector.destroyDisable = false;
                var rb = GetComponent<Rigidbody>();
                rb.isKinematic = false;
            }
        }

        private void Start()
        {
            // This specifically prevents the intial prefab from being deleted or falling through the scene
            if ( BundleHelper.SceneIsPrefabStage() )
            {
                StartCoroutine(LateStart(0.1f));
            }

            else
            {
                var rb = GetComponent<Rigidbody>();
                rb.isKinematic = false;
                isPrefab = false;
                photonTransformView.enabled = true;
            }

            UpdateMaterial();
            UpdateLightColor();
        }

        IEnumerator LateStart(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            this.transform.parent = BundleManager.instance.transform;
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.rotation = Quaternion.identity;
            transform.position = new Vector3(0, 500, 0);
            DebugLogger.LogWarning($"Upgrade Bundle pos start: {gameObject.transform.position}", true);
        }

        private void Update()
        {
            if (SemiFunc.RunIsShop())
            {
                return;
            }

            if (!SemiFunc.IsMasterClientOrSingleplayer() || !itemToggle.toggleState || used)
            {
                return;
            }

            if (!used && itemToggle.toggleState)
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
            var playerCount = SemiFunc.PlayerGetAll().Count;
            playerCount = Mathf.Max(playerCount, BundleHelper.GetItemBundleMinItem(originalItem));

            for (int i = 0; i < (playerCount + ItemBundles.Instance.config_debugFakePlayers.Value); i++)
            {
                var boxThicknessOffset = 0.075f;
                var spawnOffset = -transform.forward * boxThicknessOffset * (i - 1);

                if ( !SemiFunc.IsMultiplayer() )
                {
                    var obj = Object.Instantiate(originalItem.prefab.Prefab, base.transform.position + spawnOffset, gameObject.transform.rotation);
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.prefab.prefabName);
                    StatsManager.instance.AddItemsUpgradesPurchased(obj.GetComponent<ItemAttributes>().item.prefab.prefabName);

                }
                if ( SemiFunc.IsMasterClient() )
                {
                    GameObject obj = PhotonNetwork.Instantiate(originalItem.prefab.resourcePath, base.transform.position + spawnOffset, gameObject.transform.rotation);
                    StatsManager.instance.ItemPurchase(obj.GetComponent<ItemAttributes>().item.prefab.prefabName);
                    StatsManager.instance.AddItemsUpgradesPurchased(obj.GetComponent<ItemAttributes>().item.prefab.prefabName);
                }
            }
        }


        public void SetBoxMesh( Mesh mesh )
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
        public Material? GetOriginalBoxMat()
        {
            if (originalItem == null) return null;
            Item? obj = originalItem;
            Material? mat = null;

            var mesh = obj.prefab.Prefab.transform.Find("Mesh");
            var meshR = mesh.GetComponent<MeshRenderer>();
            if (meshR)
            {
                mat = meshR.materials[0];
            }

            if (!mat)
            {
                DebugLogger.LogError($"- GetBoxMat {originalItem.prefab.prefabName} failed, returning NULL", true);
            }
            else
            {
                DebugLogger.LogInfo($"- GetBoxMat returning {mat}", true);
            }

            return mat;
        }

        /// <summary>
        /// Get a light component so we can read and copy it's info
        /// </summary>
        /// <param name="upgradeItemName"></param>
        /// <returns> Light Component on the gameObject "Light - Small Lamp"</returns>
        public Light? GetOriginalLight()
        {
            Item? obj = originalItem;
            if ( obj == null ) return null;

            var lightObj = obj.prefab.Prefab.transform.Find("Light - Small Lamp");
            if ( !lightObj ) return null;

            var light = lightObj.GetComponent<Light>();
            return light;
        }

        public void UpdateMaterial()
        {
            var originalMat = GetOriginalBoxMat();
            if (originalMat == null) return;

            var mesh = transform.Find("Mesh");
            var meshR = mesh.GetComponent<MeshRenderer>();
            if (meshR.materials[0].name.Contains("upgrade"))
            {
                Material[] newMaterials = new Material[2] { originalMat, meshR.materials[1] };
                meshR.materials = newMaterials;
            }
        }

        public void UpdateLightColor()
        {
            var light = gameObject.transform.Find("Light - Small Lamp").GetComponent<Light>();
            var originalLight = GetOriginalLight();
            if (light != null && originalLight != null)
            {
                light.color = originalLight.color;
            }
        }

        public void OnDestroy()
        {
        }
    }
}
