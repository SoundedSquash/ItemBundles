using UnityEngine;

namespace ItemBundles
{
    public class ModdedItemUpgradeBundle : MonoBehaviour
    {
        public string upgradeItemName;

        public void FixMaterial()
        {
            var originalMat = MoreUpgradesCompat.GetBoxMat(upgradeItemName);
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
            var originalLight = MoreUpgradesCompat.GetLight(upgradeItemName);
            if  (light != null && originalLight != null)
            {

                light.color = originalLight.color;
            }
        }

        public void Upgrade()
        {
            if ( string.IsNullOrEmpty(upgradeItemName) )
            {
                DebugLogger.LogError("upgradeItemName was empty! Cannot upgrade");
                return;
            }

            var players = SemiFunc.PlayerGetAll();
            foreach (var player in players)
            {
                var steamId = SemiFunc.PlayerGetSteamID(player);
                MoreUpgradesCompat.CallUpgrade(upgradeItemName, steamId, 1);
            }
        }
    }
}
