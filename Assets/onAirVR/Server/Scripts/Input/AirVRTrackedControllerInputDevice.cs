/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;

public class AirVRTrackedControllerInputDevice : AirVRInputDevice {

    // implements AirVRInputDevice
    protected override string deviceName {
        get {
            return AirVRInputDeviceName.TrackedController;
        }
    }

    protected override void MakeControlList() {
        AddControlTouch((byte)AirVRTrackedControllerKey.Touchpad);
        AddControlTransform((byte)AirVRTrackedControllerKey.Transform);
        AddControlButton((byte)AirVRTrackedControllerKey.ButtonTouchpad);
        AddControlButton((byte)AirVRTrackedControllerKey.ButtonBack);
        AddControlButton((byte)AirVRTrackedControllerKey.ButtonIndexTrigger);
        AddControlButton((byte)AirVRTrackedControllerKey.ButtonUp);
        AddControlButton((byte)AirVRTrackedControllerKey.ButtonDown);
        AddControlButton((byte)AirVRTrackedControllerKey.ButtonLeft);
        AddControlButton((byte)AirVRTrackedControllerKey.ButtonRight);

        AddExtControlAxis2D((byte)AirVRTrackedControllerKey.ExtAxis2DTouchPosition);
        AddExtControlButton((byte)AirVRTrackedControllerKey.ExtButtonTouch);
    }

    protected override void UpdateExtendedControls() {
        Vector2 position = Vector2.zero;
        bool touch = false;

        if (GetTouch((byte)AirVRTrackedControllerKey.Touchpad, ref position, ref touch)) {
            SetExtControlAxis2D((byte)AirVRTrackedControllerKey.ExtAxis2DTouchPosition, position);
            SetExtControlButton((byte)AirVRTrackedControllerKey.ExtButtonTouch, touch ? 1.0f : 0.0f);
        }
        else {
            SetExtControlAxis2D((byte)AirVRTrackedControllerKey.ExtAxis2DTouchPosition, Vector2.zero);
            SetExtControlButton((byte)AirVRTrackedControllerKey.ExtButtonTouch, 0.0f);
        }
    }
}
