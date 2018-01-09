/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class AirVRSampleAdvancedScene : MonoBehaviour, AirVRCameraRigManager.EventHandler {
    [SerializeField]
    private AirVRCameraRig _primaryCameraRig;

    void Awake() {
        AirVRCameraRigManager.managerOnCurrentScene.Delegate = this;
    }

    // implements AirVRCameraRigMananger.EventHandler
    public void AirVRCameraRigWillBeBound(int clientHandle, AirVRClientConfig config, List<AirVRCameraRig> availables, out AirVRCameraRig selected) {
        if (availables.Contains(_primaryCameraRig)) {
            selected = _primaryCameraRig;
        }
        else if (availables.Count > 0) {
            selected = availables[0];
        }
        else {
            selected = null;
        }
    }

    public void AirVRCameraRigActivated(AirVRCameraRig cameraRig) {}
    public void AirVRCameraRigDeactivated(AirVRCameraRig cameraRig) {}
    public void AirVRCameraRigHasBeenUnbound(AirVRCameraRig cameraRig) {}
}
