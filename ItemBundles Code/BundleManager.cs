using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ItemBundles
{
    /// <summary>
    /// Currently just Simple Singleton Object that holds prefabs and prevents them from getting cleared during scene changes
    /// </summary>
    public class BundleManager : MonoBehaviour
    {
        public static BundleManager instance;

        public void Awake()
        {
            if (!instance)
            {
                instance = this;
            }
        }
    }
}
