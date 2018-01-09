/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using UnityEngine.Assertions;

public static class AirVRInput {
    public enum Device {
        HeadTracker,
        Touchpad,
        Gamepad,
        TrackedController
    }

    public static class Touchpad {
        public enum Axis2D {
            Position
        }

        public enum Button {
            Touch,
            Back,
            Up,
            Down,
            Left,
            Right
        }

        internal static byte ParseControlID(Axis2D axis) {
            switch (axis) {
                case Axis2D.Position:
                    return (byte)AirVRTouchpadKey.ExtAxis2DPosition;
            }
            Assert.IsTrue(false);
            return 0;
        }

        internal static byte ParseControlID(Button button) {
            switch (button) {
                case Button.Touch:
                    return (byte)AirVRTouchpadKey.ExtButtonTouch;
                case Button.Back:
                    return (byte)AirVRTouchpadKey.ButtonBack;
                case Button.Up:
                    return (byte)AirVRTouchpadKey.ButtonUp;
                case Button.Down:
                    return (byte)AirVRTouchpadKey.ButtonDown;
                case Button.Left:
                    return (byte)AirVRTouchpadKey.ButtonLeft;
                case Button.Right:
                    return (byte)AirVRTouchpadKey.ButtonRight;
            }
            Assert.IsTrue(false);
            return 0;
        }
    }

    public static class Gamepad {
        public enum Axis2D {
            LThumbstick,
            RThumbstick
        }

        public enum Axis {
            LIndexTrigger,
            RIndexTrigger,
        }

        public enum Button {
            A,
            B,
            X,
            Y,
            Start,
            Back,
            LShoulder,
            RShoulder,
            LIndexTrigger,
            RIndexTrigger,
            LThumbstick,
            LThumbstickUp,
            LThumbstickDown,
            LThumbstickLeft,
            LThumbstickRight,
            RThumbstick,
            RThumbstickUp,
            RThumbstickDown,
            RThumbstickLeft,
            RThumbstickRight,
            DpadUp,
            DpadDown,
            DpadLeft,
            DpadRight
        }

        internal static byte ParseControlID(Axis2D axis) {
            switch (axis) {
                case Axis2D.LThumbstick:
                    return (byte)AirVRGamepadKey.Axis2DLThumbstick;
                case Axis2D.RThumbstick:
                    return (byte)AirVRGamepadKey.Axis2DRThumbstick;
            }
            Assert.IsTrue(false);
            return 0;
        }

        internal static byte ParseControlID(Axis axis) {
            switch (axis) {
                case Axis.LIndexTrigger:
                    return (byte)AirVRGamepadKey.AxisLIndexTrigger;
                case Axis.RIndexTrigger:
                    return (byte)AirVRGamepadKey.AxisRIndexTrigger;
            }
            Assert.IsTrue(false);
            return 0;
        }

        internal static byte ParseControlID(Button button) {
            switch (button) {
                case Button.A:
                    return (byte)AirVRGamepadKey.ButtonA;
                case Button.B:
                    return (byte)AirVRGamepadKey.ButtonB;
                case Button.X:
                    return (byte)AirVRGamepadKey.ButtonX;
                case Button.Y:
                    return (byte)AirVRGamepadKey.ButtonY;
                case Button.Start:
                    return (byte)AirVRGamepadKey.ButtonStart;
                case Button.Back:
                    return (byte)AirVRGamepadKey.ButtonBack;
                case Button.LShoulder:
                    return (byte)AirVRGamepadKey.ButtonLShoulder;
                case Button.RShoulder:
                    return (byte)AirVRGamepadKey.ButtonRShoulder;
                case Button.LIndexTrigger:
                    return (byte)AirVRGamepadKey.ExtButtonLIndexTrigger;
                case Button.RIndexTrigger:
                    return (byte)AirVRGamepadKey.ExtButtonRIndexTrigger;
                case Button.LThumbstick:
                    return (byte)AirVRGamepadKey.ButtonLThumbstick;
                case Button.LThumbstickUp:
                    return (byte)AirVRGamepadKey.ExtButtonLThumbstickUp;
                case Button.LThumbstickDown:
                    return (byte)AirVRGamepadKey.ExtButtonLThumbstickDown;
                case Button.LThumbstickLeft:
                    return (byte)AirVRGamepadKey.ExtButtonLThumbstickLeft;
                case Button.LThumbstickRight:
                    return (byte)AirVRGamepadKey.ExtButtonLThumbstickRight;
                case Button.RThumbstick:
                    return (byte)AirVRGamepadKey.ButtonRThumbstick;
                case Button.RThumbstickUp:
                    return (byte)AirVRGamepadKey.ExtButtonRThumbstickUp;
                case Button.RThumbstickDown:
                    return (byte)AirVRGamepadKey.ExtButtonRThumbstickDown;
                case Button.RThumbstickLeft:
                    return (byte)AirVRGamepadKey.ExtButtonRThumbstickLeft;
                case Button.RThumbstickRight:
                    return (byte)AirVRGamepadKey.ExtButtonRThumbstickRight;
                case Button.DpadUp:
                    return (byte)AirVRGamepadKey.ButtonDpadUp;
                case Button.DpadDown:
                    return (byte)AirVRGamepadKey.ButtonDpadDown;
                case Button.DpadLeft:
                    return (byte)AirVRGamepadKey.ButtonDpadLeft;
                case Button.DpadRight:
                    return (byte)AirVRGamepadKey.ButtonDpadRight;
            }
            Assert.IsTrue(false);
            return 0;
        }
    }

    public static class TrackedController {
        public enum Axis2D {
            TouchpadPosition
        }

        public enum Button {
            TouchpadTouch,
            TouchpadClick,
            Back,
            IndexTrigger,
            Up,
            Down,
            Left,
            Right
        }

        internal static byte ParseControlID(Axis2D axis) {
            switch (axis) {
                case Axis2D.TouchpadPosition:
                    return (byte)AirVRTrackedControllerKey.ExtAxis2DTouchPosition;
            }
            Assert.IsTrue(false);
            return 0;
        }

        internal static byte ParseControlID(Button button) {
            switch (button) {
                case Button.TouchpadTouch:
                    return (byte)AirVRTrackedControllerKey.ExtButtonTouch;
                case Button.TouchpadClick:
                    return (byte)AirVRTrackedControllerKey.ButtonTouchpad;
                case Button.Back:
                    return (byte)AirVRTrackedControllerKey.ButtonBack;
                case Button.IndexTrigger:
                    return (byte)AirVRTrackedControllerKey.ButtonIndexTrigger;
                case Button.Up:
                    return (byte)AirVRTrackedControllerKey.ButtonUp;
                case Button.Down:
                    return (byte)AirVRTrackedControllerKey.ButtonDown;
                case Button.Left:
                    return (byte)AirVRTrackedControllerKey.ButtonLeft;
                case Button.Right:
                    return (byte)AirVRTrackedControllerKey.ButtonRight;
            }
            Assert.IsTrue(false);
            return 0;
        }
    }

    private static string deviceName(Device device) {
        switch (device) {
            case Device.HeadTracker:
                return AirVRInputDeviceName.HeadTracker;
            case Device.Touchpad:
                return AirVRInputDeviceName.Touchpad;
            case Device.Gamepad:
                return AirVRInputDeviceName.Gamepad;
            case Device.TrackedController:
                return AirVRInputDeviceName.TrackedController;
        }
        Assert.IsTrue(false);
        return "";
    }

    private static bool isTrackedDevice(Device device) {
        return device == Device.HeadTracker || device == Device.TrackedController;
    }

    private static byte transformControlID(Device device) {
        switch (device) {
            case Device.HeadTracker:
                return (byte)AirVRHeadTrackerKey.Transform;
            case Device.TrackedController:
                return (byte)AirVRTrackedControllerKey.Transform;
        }
        Assert.IsTrue(false);
        return 0;
    }

    private static byte feedbackRaycastHitResultControlID(Device device) {
        switch (device) {
            case Device.HeadTracker:
                return (byte)AirVRHeadTrackerKey.RaycastHitResult;
            case Device.TrackedController:
                return (byte)AirVRTrackedControllerKey.RaycastHitResult;
        }
        Assert.IsTrue(false);
        return 0;
    }

    private static void GetTransform(AirVRCameraRig cameraRig, string deviceName, byte controlID, ref Vector3 worldPosition, ref Quaternion worldOrientation) {
        Vector3 position = Vector3.zero;
        Quaternion orientation = Quaternion.identity;

        cameraRig.inputStream.GetTransform(deviceName, controlID, ref position, ref orientation);

        worldPosition = cameraRig.clientSpaceToWorldMatrix.MultiplyPoint(position);
        worldOrientation = Quaternion.LookRotation(cameraRig.clientSpaceToWorldMatrix.GetColumn(2), cameraRig.clientSpaceToWorldMatrix.GetColumn(1)) * orientation;
    }

    private static Vector2 GetAxis2D(AirVRCameraRig cameraRig, string deviceName, byte controlID) {
        return cameraRig.inputStream.GetAxis2D(deviceName, controlID);
    }

    private static float GetAxis(AirVRCameraRig cameraRig, string deviceName, byte controlID) {
        return cameraRig.inputStream.GetAxis(deviceName, controlID);
    }

    private static bool GetButton(AirVRCameraRig cameraRig, string deviceName, byte controlID) {
        return cameraRig.inputStream.GetButton(deviceName, controlID);
    }

    private static bool GetButtonDown(AirVRCameraRig cameraRig, string deviceName, byte controlID) {
        return cameraRig.inputStream.GetButtonDown(deviceName, controlID);
    }

    private static bool GetButtonUp(AirVRCameraRig cameraRig, string deviceName, byte controlID) {
        return cameraRig.inputStream.GetButtonUp(deviceName, controlID);
    }

    public static bool IsDeviceAvailable(AirVRCameraRig cameraRig, Device device) {
        return cameraRig.inputStream.CheckIfInputDeviceAvailable(deviceName(device));
    }

    public static Vector2 Get(AirVRCameraRig cameraRig, Touchpad.Axis2D axis) {
        return GetAxis2D(cameraRig, AirVRInputDeviceName.Touchpad, Touchpad.ParseControlID(axis));
    }

    public static Vector2 Get(AirVRCameraRig cameraRig, Gamepad.Axis2D axis) {
        return GetAxis2D(cameraRig, AirVRInputDeviceName.Gamepad, Gamepad.ParseControlID(axis));
    }

    public static Vector2 Get(AirVRCameraRig cameraRig, TrackedController.Axis2D axis) {
        return GetAxis2D(cameraRig, AirVRInputDeviceName.TrackedController, TrackedController.ParseControlID(axis));
    }

    public static float Get(AirVRCameraRig cameraRig, Gamepad.Axis axis) {
        return GetAxis(cameraRig, AirVRInputDeviceName.Gamepad, Gamepad.ParseControlID(axis));
    }

    public static bool Get(AirVRCameraRig cameraRig, Touchpad.Button button) {
        return GetButton(cameraRig, AirVRInputDeviceName.Touchpad, Touchpad.ParseControlID(button));
    }

    public static bool Get(AirVRCameraRig cameraRig, Gamepad.Button button) {
        return GetButton(cameraRig, AirVRInputDeviceName.Gamepad, Gamepad.ParseControlID(button));
    }

    public static bool Get(AirVRCameraRig cameraRig, TrackedController.Button button) {
        return GetButton(cameraRig, AirVRInputDeviceName.TrackedController, TrackedController.ParseControlID(button));
    }

    public static bool GetDown(AirVRCameraRig cameraRig, Touchpad.Button button) {
        return GetButtonDown(cameraRig, AirVRInputDeviceName.Touchpad, Touchpad.ParseControlID(button));
    }

    public static bool GetDown(AirVRCameraRig cameraRig, Gamepad.Button button) {
        return GetButtonDown(cameraRig, AirVRInputDeviceName.Gamepad, Gamepad.ParseControlID(button));
    }

    public static bool GetDown(AirVRCameraRig cameraRig, TrackedController.Button button) {
        return GetButtonDown(cameraRig, AirVRInputDeviceName.TrackedController, TrackedController.ParseControlID(button));
    }

    public static bool GetUp(AirVRCameraRig cameraRig, Touchpad.Button button) {
        return GetButtonUp(cameraRig, AirVRInputDeviceName.Touchpad, Touchpad.ParseControlID(button));
    }

    public static bool GetUp(AirVRCameraRig cameraRig, Gamepad.Button button) {
        return GetButtonUp(cameraRig, AirVRInputDeviceName.Gamepad, Gamepad.ParseControlID(button));
    }

    public static bool GetUp(AirVRCameraRig cameraRig, TrackedController.Button button) {
        return GetButtonUp(cameraRig, AirVRInputDeviceName.TrackedController, TrackedController.ParseControlID(button));
    }

    public static bool IsDeviceFeedbackEnabled(AirVRCameraRig cameraRig, Device device) {
        return cameraRig.inputStream.IsDeviceFeedbackEnabled(deviceName(device));
    }

    public static void EnableTrackedDeviceFeedback(AirVRCameraRig cameraRig, Device device, Texture2D cookieTexture, float depthScaleMultiplier) {
        if (isTrackedDevice(device) == false) {
            return;
        }
        
        cameraRig.inputStream.EnableTrackedDeviceFeedback(deviceName(device), cookieTexture, depthScaleMultiplier);
    }

    public static void DisableDeviceFeedback(AirVRCameraRig cameraRig, Device device) {
        cameraRig.inputStream.DisableDeviceFeedback(deviceName(device));
    }

    public static void GetTrackedDevicePositionAndOrientation(AirVRCameraRig cameraRig, Device device, out Vector3 worldPosition, out Quaternion worldOrientation) {
        if (isTrackedDevice(device) == false) {
            worldPosition = cameraRig.headPose.position;
            worldOrientation = cameraRig.headPose.rotation; 
            return;
        }

        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        GetTransform(cameraRig, deviceName(device), transformControlID(device), ref pos, ref rot);

        worldPosition = pos;
        worldOrientation = rot;
    }

    public static void FeedbackTrackedDevice(AirVRCameraRig cameraRig, Device device, Vector3 worldRayOrigin, Vector3 worldHitPosition, Vector3 worldHitNormal) {
        if (isTrackedDevice(device) == false) {
            return;
        }

        cameraRig.inputStream.FeedbackTrackedDevice(deviceName(device), feedbackRaycastHitResultControlID(device),
                                                    cameraRig.clientSpaceToWorldMatrix.inverse.MultiplyPoint(worldRayOrigin),
                                                    cameraRig.clientSpaceToWorldMatrix.inverse.MultiplyPoint(worldHitPosition),
                                                    cameraRig.clientSpaceToWorldMatrix.inverse.MultiplyVector(worldHitNormal));
    }
}
