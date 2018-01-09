/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;

public class AirVRGamepadInputDevice : AirVRInputDevice {
    private const float AxisAsButtonThreshold = 0.5f;   // refer OVRInput.cs in Oculus Utilities

    // implements AirVRInputDevice
    protected override string deviceName {
        get {
            return AirVRInputDeviceName.Gamepad;
        }
    }

    protected override void MakeControlList() {
        AddControlAxis2D((byte)AirVRGamepadKey.Axis2DLThumbstick);
        AddControlAxis2D((byte)AirVRGamepadKey.Axis2DRThumbstick);
        AddControlAxis((byte)AirVRGamepadKey.AxisLIndexTrigger);
        AddControlAxis((byte)AirVRGamepadKey.AxisRIndexTrigger);

        AddControlButton((byte)AirVRGamepadKey.ButtonA);
        AddControlButton((byte)AirVRGamepadKey.ButtonB);
        AddControlButton((byte)AirVRGamepadKey.ButtonX);
        AddControlButton((byte)AirVRGamepadKey.ButtonY);
        AddControlButton((byte)AirVRGamepadKey.ButtonStart);
        AddControlButton((byte)AirVRGamepadKey.ButtonBack);
        AddControlButton((byte)AirVRGamepadKey.ButtonLShoulder);
        AddControlButton((byte)AirVRGamepadKey.ButtonRShoulder);
        AddControlButton((byte)AirVRGamepadKey.ButtonLThumbstick);
        AddControlButton((byte)AirVRGamepadKey.ButtonRThumbstick);
        AddControlButton((byte)AirVRGamepadKey.ButtonDpadUp);
        AddControlButton((byte)AirVRGamepadKey.ButtonDpadDown);
        AddControlButton((byte)AirVRGamepadKey.ButtonDpadLeft);
        AddControlButton((byte)AirVRGamepadKey.ButtonDpadRight);

        AddExtControlButton((byte)AirVRGamepadKey.ExtButtonLIndexTrigger);
        AddExtControlButton((byte)AirVRGamepadKey.ExtButtonRIndexTrigger);
        AddExtControlButton((byte)AirVRGamepadKey.ExtButtonLThumbstickUp);
        AddExtControlButton((byte)AirVRGamepadKey.ExtButtonLThumbstickDown);
        AddExtControlButton((byte)AirVRGamepadKey.ExtButtonLThumbstickLeft);
        AddExtControlButton((byte)AirVRGamepadKey.ExtButtonLThumbstickRight);
        AddExtControlButton((byte)AirVRGamepadKey.ExtButtonRThumbstickUp);
        AddExtControlButton((byte)AirVRGamepadKey.ExtButtonRThumbstickDown);
        AddExtControlButton((byte)AirVRGamepadKey.ExtButtonRThumbstickLeft);
        AddExtControlButton((byte)AirVRGamepadKey.ExtButtonRThumbstickRight);
    }

    protected override void UpdateExtendedControls() {
        SetExtControlButton((byte)AirVRGamepadKey.ExtButtonLIndexTrigger, GetAxis((byte)AirVRGamepadKey.AxisLIndexTrigger) >= AxisAsButtonThreshold ? 1.0f : 0.0f);
        SetExtControlButton((byte)AirVRGamepadKey.ExtButtonRIndexTrigger, GetAxis((byte)AirVRGamepadKey.AxisRIndexTrigger) >= AxisAsButtonThreshold ? 1.0f : 0.0f);

        Vector2 axis = GetAxis2D((byte)AirVRGamepadKey.Axis2DLThumbstick);
        SetExtControlButton((byte)AirVRGamepadKey.ExtButtonLThumbstickLeft, axis.x <= -AxisAsButtonThreshold ? 1.0f : 0.0f);
        SetExtControlButton((byte)AirVRGamepadKey.ExtButtonLThumbstickRight, axis.x >= AxisAsButtonThreshold ? 1.0f : 0.0f);
        SetExtControlButton((byte)AirVRGamepadKey.ExtButtonLThumbstickDown, axis.y <= -AxisAsButtonThreshold ? 1.0f : 0.0f);
        SetExtControlButton((byte)AirVRGamepadKey.ExtButtonLThumbstickUp, axis.y >= AxisAsButtonThreshold ? 1.0f : 0.0f);

        axis = GetAxis2D((byte)AirVRGamepadKey.Axis2DRThumbstick);
        SetExtControlButton((byte)AirVRGamepadKey.ExtButtonRThumbstickLeft, axis.x <= -AxisAsButtonThreshold ? 1.0f : 0.0f);
        SetExtControlButton((byte)AirVRGamepadKey.ExtButtonRThumbstickRight, axis.x >= AxisAsButtonThreshold ? 1.0f : 0.0f);
        SetExtControlButton((byte)AirVRGamepadKey.ExtButtonRThumbstickDown, axis.y <= -AxisAsButtonThreshold ? 1.0f : 0.0f);
        SetExtControlButton((byte)AirVRGamepadKey.ExtButtonRThumbstickUp, axis.y >= AxisAsButtonThreshold ? 1.0f : 0.0f);
    }
}
