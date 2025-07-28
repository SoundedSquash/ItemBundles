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

        Vector3 direction = Vector3.zero;
        float forceMin = 1f;
        float forceMax = 5f;
        float forceMult = 1f;

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
            Vector3 vector = direction == Vector3.zero ? Random.insideUnitSphere : direction;
            rb.AddForce(vector * forceMult * Random.Range(forceMin, forceMax), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * forceMult * Random.Range(forceMin, forceMax), ForceMode.Impulse);
        }
    }
}
