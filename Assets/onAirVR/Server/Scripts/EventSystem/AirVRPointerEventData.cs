/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;

internal class AirVRPointerEventData : PointerEventData {
    public AirVRPointerEventData(EventSystem eventSystem) : base(eventSystem) { }

    public Ray worldSpaceRay;
}

internal static class PointerEventDataExtension {
    public static bool IsVRPointer(this PointerEventData pointerEventData) {
        return pointerEventData is AirVRPointerEventData;
    }

    public static Ray GetRay(this PointerEventData pointerEventData) {
        return (pointerEventData as AirVRPointerEventData).worldSpaceRay;
    }
}