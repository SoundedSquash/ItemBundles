using System.Collections;
using UnityEngine;

namespace ItemBundles
{
    public class ItemLateStartSetter : MonoBehaviour
    {
        PhysGrabObject physGrabObject;
        Rigidbody rb;

        public Vector3 newStartPosition = Vector3.zero;
        public Quaternion newStartRotation = Quaternion.identity;

        public bool enable;

        public void Start()
        {
            physGrabObject = GetComponent<PhysGrabObject>();
            rb = GetComponent<Rigidbody>();

            StartCoroutine(LateSpawn());
        }

        private IEnumerator LateSpawn()
        {
            while (!physGrabObject.spawned || rb.isKinematic || !enable)
            {
                yield return null;
            }

            var grenadeComponent = GetComponent<ItemGrenade>();
            if (grenadeComponent != null)
            {
                grenadeComponent.grenadeStartPosition = newStartPosition;
                grenadeComponent.grenadeStartRotation = newStartRotation;
            }

            var mineComponent = GetComponent<ItemMine>();
            if (mineComponent != null)
            {
                mineComponent.startPosition = newStartPosition;
                mineComponent.startRotation = newStartRotation;
            }
        }
    }
}
