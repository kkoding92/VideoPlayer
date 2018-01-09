/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

public static class AirVRInputDeviceName {
    public const string HeadTracker = "HeadTracker";
    public const string Touchpad = "Touchpad";
    public const string Gamepad = "Gamepad";
    public const string TrackedController = "TrackedController";
}

public enum AirVRHeadTrackerKey {
    Transform = 0,
    RaycastHitResult,

    // ADD ADDITIONAL KEYS HERE

    Max
}

public enum AirVRTouchpadKey {
    Touchpad = 0,

    ButtonBack,
    ButtonUp,
    ButtonDown,
    ButtonLeft,
    ButtonRight,

    // ADD ADDITIONAL KEYS HERE

    ExtAxis2DPosition,
    ExtButtonTouch,

    Max
}

public enum AirVRGamepadKey {
    Axis2DLThumbstick = 0,
    Axis2DRThumbstick,
    AxisLIndexTrigger,
    AxisRIndexTrigger,

    ButtonA,
    ButtonB,
    ButtonX,
    ButtonY,
    ButtonStart,
    ButtonBack,
    ButtonLShoulder,
    ButtonRShoulder,
    ButtonLThumbstick,
    ButtonRThumbstick,
    ButtonDpadUp,
    ButtonDpadDown,
    ButtonDpadLeft,
    ButtonDpadRight,

    // ADD ADDITIONAL KEYS HERE

    ExtButtonLIndexTrigger,
    ExtButtonRIndexTrigger,
    ExtButtonLThumbstickUp,
    ExtButtonLThumbstickDown,
    ExtButtonLThumbstickLeft,
    ExtButtonLThumbstickRight,
    ExtButtonRThumbstickUp,
    ExtButtonRThumbstickDown,
    ExtButtonRThumbstickLeft,
    ExtButtonRThumbstickRight,

    Max
}

public enum AirVRTrackedControllerKey {
    Touchpad = 0,
    Transform,
    RaycastHitResult,

    ButtonTouchpad,
    ButtonBack,
    ButtonIndexTrigger,
    ButtonUp,
    ButtonDown,
    ButtonLeft,
    ButtonRight,

    // ADD ADDITIONAL KEYS HERE

    ExtAxis2DTouchPosition,
    ExtButtonTouch,

    Max
}

