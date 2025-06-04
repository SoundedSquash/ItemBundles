using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ItemBundles
{
    public class BundleManager : MonoBehaviour
    {
        public static BundleManager instance;

        public void Awake()
        {
            if (instance == null )
            {
                instance = this;
            }
            DebugLogger.LogInfo("======== Bundle Manager Awake");
        }
    }
}
