/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;

public class AirVRTouchpadInputDevice : AirVRInputDevice {
    private const float DragMultiplier = 5.0f;

    // implements AirVRInputDevice
    protected override string deviceName {
        get {
            return AirVRInputDeviceName.Touchpad;
        }
    }

    protected override void MakeControlList() {
        AddControlTouch((byte)AirVRTouchpadKey.Touchpad);
        AddControlButton((byte)AirVRTouchpadKey.ButtonBack);
        AddControlButton((byte)AirVRTouchpadKey.ButtonUp);
        AddControlButton((byte)AirVRTouchpadKey.ButtonDown);
        AddControlButton((byte)AirVRTouchpadKey.ButtonLeft);
        AddControlButton((byte)AirVRTouchpadKey.ButtonRight);

        AddExtControlAxis2D((byte)AirVRTouchpadKey.ExtAxis2DPosition);
        AddExtControlButton((byte)AirVRTouchpadKey.ExtButtonTouch);
    }

    protected override void UpdateExtendedControls() {
        Vector2 position = Vector2.zero;
        bool touch = false;

        if (GetTouch((byte)AirVRTouchpadKey.Touchpad, ref position, ref touch)) {
            SetExtControlAxis2D((byte)AirVRTouchpadKey.ExtAxis2DPosition, position);
            SetExtControlButton((byte)AirVRTouchpadKey.ExtButtonTouch, touch ? 1.0f : 0.0f);
        }
        else {
            SetExtControlAxis2D((byte)AirVRTouchpadKey.ExtAxis2DPosition, Vector2.zero);
            SetExtControlButton((byte)AirVRTouchpadKey.ExtButtonTouch, 0.0f);
        }
    }
}
