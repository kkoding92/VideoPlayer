/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine.EventSystems;

public class AirVREventSystem : EventSystem {
    protected override void OnApplicationFocus(bool hasFocus) {
        // do nothing to prevents from being paused when lose focus
    }
}
