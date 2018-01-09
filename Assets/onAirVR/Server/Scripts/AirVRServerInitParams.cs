/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;

public class AirVRServerInitParams : MonoBehaviour {
    public string licenseFilePath       = AirVRServerParams.DefaultLicenseFilePath;
    public int maxClientCount           = AirVRServerParams.DefaultMaxClientCount;
    public int port                     = AirVRServerParams.DefaultPort;
    public int videoBitrate             = AirVRServerParams.DefaultVideoBitrate;
    public float maxFrameRate           = AirVRServerParams.DefaultMaxFrameRate;
    public float defaultFrameRate       = AirVRServerParams.DefaultDefaultFrameRate;
    
    void Start() {
        Destroy(gameObject);
    }
}
