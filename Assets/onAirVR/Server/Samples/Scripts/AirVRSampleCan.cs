/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]

public class AirVRSampleCan : MonoBehaviour {
    public float lifetime;

    private IEnumerator DestroySelf() {
        yield return new WaitForSeconds(lifetime);

        Destroy(gameObject);
    }

    void Start() {
        StartCoroutine(DestroySelf());
    }

    public void Throw(Vector3 velocity, Vector3 torque) {
        Rigidbody rigid = GetComponent<Rigidbody>();
        rigid.AddForce(velocity, ForceMode.VelocityChange);
        rigid.AddTorque(torque, ForceMode.VelocityChange);
    }
}
