using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ItemBundles
{
    public class ItemLateImpulse :MonoBehaviour
    {
        PhysGrabObject physGrabObject;
        Rigidbody rb;

        public void Start()
        {
            physGrabObject = GetComponent<PhysGrabObject>();
            rb = GetComponent<Rigidbody>();

            StartCoroutine(LateSpawn());
        }

        private IEnumerator LateSpawn()
        {
            while (!physGrabObject.spawned || rb.isKinematic)
            {
                yield return null;
            }
            Vector3 vector = Random.insideUnitSphere;
            rb.AddForce(vector * Random.Range(1, 5), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(1f, 5f), ForceMode.Impulse);
        }
    }
}
