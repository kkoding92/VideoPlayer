/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

public class AirVRServerInputStream : AirVRInputStream {
    [DllImport(AirVRServerPlugin.Name)]
    private static extern byte onairvr_RegisterTrackedDeviceFeedbackAsInputSender(int playerID, string name, IntPtr cookieTexture, int cookieTextureSize, float cookieDepthScaleMultiplier);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_UnregisterInputSender(int playerID, byte id);


    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_PendTrackedDeviceFeedback(int playerID, byte deviceID, byte controlID,
                                                                 float worldRayOriginX, float worldRayOriginY, float worldRayOriginZ,
                                                                 float worldHitPositionX, float worldHitPositionY, float worldHitPositionZ,
                                                                 float worldHitNormalX, float worldHitNormalY, float worldHitNormalZ, byte policy);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool onairvr_GetInputTouch(int playerID, byte deviceID, byte controlID, ref float posX, ref float posY, ref float touch);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool onairvr_GetInputTransform(int playerID, byte deviceID, byte controlID,
                                                         ref float posX, ref float posY, ref float posZ, 
                                                         ref float rotX, ref float rotY, ref float rotZ, ref float retW);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool onairvr_GetInputFloat4(int playerID, byte deviceID, byte controlID, ref float x, ref float y, ref float z, ref float w);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool onairvr_GetInputFloat3(int playerID, byte deviceID, byte controlID, ref float x, ref float y, ref float z);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool onairvr_GetInputFloat2(int playerID, byte deviceID, byte controlID, ref float x, ref float y);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool onairvr_GetInputFloat(int playerID, byte deviceID, byte controlID, ref float value);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_SendPendingInputs(int playerID);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_ResetInput(int playerID);

    public AirVRServerInputStream() {
        addInputDevice(new AirVRHeadTrackerInputDevice());
        addInputDevice(new AirVRTouchpadInputDevice());
        addInputDevice(new AirVRGamepadInputDevice());
        addInputDevice(new AirVRTrackedControllerInputDevice());
    }

    public AirVRCameraRig owner { get; set; }

    private void addInputDevice(AirVRInputDevice device) {
        receivers.Add(device.name, device);
    }

    private void addDeviceFeedback(AirVRDeviceFeedback feedback) {
        senders.Add(feedback.name, feedback);
    }

    private AirVRTrackedDeviceFeedback createTrackedDeviceFeedback(string deviceName, Texture2D cookieTexture, float cookieDepthScaleMultiplier) {
        if (deviceName.Equals(AirVRInputDeviceName.HeadTracker)) {
            return new AirVRHeadTrackerDeviceFeedback(cookieTexture, cookieDepthScaleMultiplier);
        }
        else if (deviceName.Equals(AirVRInputDeviceName.TrackedController)) {
            return new AirVRTrackedControllerDeviceFeedback(cookieTexture, cookieDepthScaleMultiplier);
        }
        return null;
    }

    private void registerTrackedDeviceFeedback(AirVRTrackedDeviceFeedback feedback) {
        int cookieTextureSize = Marshal.SizeOf(feedback.cookieTexture[0]) * feedback.cookieTexture.Length;
        IntPtr ptr = Marshal.AllocHGlobal(cookieTextureSize);

        Marshal.Copy(feedback.cookieTexture, 0, ptr, feedback.cookieTexture.Length);
        feedback.OnRegistered(onairvr_RegisterTrackedDeviceFeedbackAsInputSender(owner.playerID, feedback.name, ptr, cookieTextureSize, feedback.cookieDepthScaleMultiplier));

        Marshal.FreeHGlobal(ptr);
    }

    public override void Init() {
        Assert.IsTrue(owner != null && owner.isBoundToClient);

        foreach (var key in senders.Keys) {
            AirVRTrackedDeviceFeedback feedback = senders[key] as AirVRTrackedDeviceFeedback;
            Assert.IsNotNull(feedback);

            registerTrackedDeviceFeedback(feedback);
        }

        base.Init();
    }

    public bool GetTransform(string deviceName, byte controlID, ref Vector3 position, ref Quaternion orientation) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetTransform(controlID, ref position, ref orientation);
        }
        return false;
    }

    public Quaternion GetOrientation(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetOrientation(controlID);
        }
        return Quaternion.identity;
    }

    public Vector2 GetAxis2D(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetAxis2D(controlID);
        }
        return Vector2.zero;
    }

    public float GetAxis(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetAxis(controlID);
        }
        return 0.0f;
    }

    public float GetButtonRaw(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetButtonRaw(controlID);
        }
        return 0.0f;
    }

    public bool GetButton(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetButton(controlID);
        }
        return false;
    }

    public bool GetButtonDown(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetButtonDown(controlID);
        }
        return false;
    }

    public bool GetButtonUp(string deviceName, byte controlID) {
        if (receivers.ContainsKey(deviceName)) {
            return (receivers[deviceName] as AirVRInputDevice).GetButtonUp(controlID);
        }
        return false;
    }

    public bool CheckIfInputDeviceAvailable(string deviceName) {
        return receivers.ContainsKey(deviceName) && receivers[deviceName].isRegistered;
    }

    public bool IsDeviceFeedbackEnabled(string deviceName) {
        return senders.ContainsKey(deviceName) && senders[deviceName].isRegistered;
    }

    public void EnableTrackedDeviceFeedback(string deviceName, Texture2D cookieTexture, float cookieDepthScaleMultiplier) {
        if (senders.ContainsKey(deviceName) == false) {
            AirVRTrackedDeviceFeedback feedback = createTrackedDeviceFeedback(deviceName, cookieTexture, cookieDepthScaleMultiplier);
            if (feedback != null) {
                addDeviceFeedback(feedback);
                if (owner != null && owner.isBoundToClient) {
                    registerTrackedDeviceFeedback(feedback);
                }
            }
        }
    }

    public void DisableDeviceFeedback(string deviceName) {
        if (senders.ContainsKey(deviceName)) {
            AirVRDeviceFeedback feedback = senders[deviceName] as AirVRDeviceFeedback;
            Assert.IsNotNull(feedback);

            if (owner != null && owner.isBoundToClient && feedback.isRegistered) {
                onairvr_UnregisterInputSender(owner.playerID, (byte)feedback.deviceID);
                feedback.OnUnregistered();
            }
            senders.Remove(deviceName);
        }
    }

    public void DisableAllDeviceFeedbacks() {
        foreach (var key in senders.Keys) {
            AirVRDeviceFeedback feedback = senders[key] as AirVRDeviceFeedback;
            Assert.IsNotNull(feedback);

            if (owner != null && owner.isBoundToClient && feedback.isRegistered) {
                onairvr_UnregisterInputSender(owner.playerID, (byte)feedback.deviceID);
                feedback.OnUnregistered();
            }
        }
        senders.Clear();
    }

    public void FeedbackTrackedDevice(string deviceName, byte controlID, Vector3 rayOrigin, Vector3 hitPosition, Vector3 hitNormal) {
        if (senders.ContainsKey(deviceName)) {
            AirVRTrackedDeviceFeedback feedback = senders[deviceName] as AirVRTrackedDeviceFeedback;
            Assert.IsNotNull(feedback);

            feedback.SetRaycastResult(rayOrigin, hitPosition, hitNormal);
        }
    }

    // implements AirVRInputStreaming
    protected override float sendingRatePerSec {
        get {
            return 90.0f;
        }
    }

    protected override void UnregisterInputSenderImpl(byte id) {
        Assert.IsTrue(owner != null && owner.isBoundToClient);

        onairvr_UnregisterInputSender(owner.playerID, id);
    }

    protected override void PendInputTouchImpl(byte deviceID, byte controlID, Vector2 position, float touch, byte policy) {
        Assert.IsTrue(false);
    }

    protected override void PendInputTransformImpl(byte deviceID, byte controlID, Vector3 position, Quaternion orientation, byte policy) {
        Assert.IsTrue(false);
    }

    protected override void PendTrackedDeviceFeedbackImpl(byte deviceID, byte controlID, Vector3 worldRayOrigin, Vector3 worldHitPosition, Vector3 worldHitNormal, byte policy) {
        Assert.IsTrue(owner != null && owner.isBoundToClient);

        onairvr_PendTrackedDeviceFeedback(owner.playerID, deviceID, controlID, worldRayOrigin.x, worldRayOrigin.y, worldRayOrigin.z,
                                          worldHitPosition.x, worldHitPosition.y, worldHitPosition.z, worldHitNormal.x, worldHitNormal.y, worldHitNormal.z, policy);
    }

    protected override void PendInputFloat4Impl(byte deviceID, byte controlID, Vector4 value, byte policy) {
        Assert.IsTrue(false);
    }

    protected override void PendInputFloat3Impl(byte deviceID, byte controlID, Vector3 value, byte policy) {
        Assert.IsTrue(false);
    }

    protected override void PendInputFloat2Impl(byte deviceID, byte controlID, Vector2 value, byte policy) {
        Assert.IsTrue(false);
    }

    protected override void PendInputFloatImpl(byte deviceID, byte controlID, float value, byte policy) {
        Assert.IsTrue(false);
    }

    protected override bool GetInputTouchImpl(byte deviceID, byte controlID, ref Vector2 position, ref float touch) {
        Assert.IsNotNull(owner);
        return onairvr_GetInputTouch(owner.playerID, deviceID, controlID, ref position.x, ref position.y, ref touch);
    }

    protected override bool GetInputTransformImpl(byte deviceID, byte controlID, ref Vector3 position, ref Quaternion orientation) {
        Assert.IsNotNull(owner);
        return onairvr_GetInputTransform(owner.playerID, deviceID, controlID, ref position.x, ref position.y, ref position.z, ref orientation.x, ref orientation.y, ref orientation.z, ref orientation.w);
    }

    protected override bool GetTrackedDeviceFeedbackImpl(byte deviceID, byte controlID, ref Vector3 worldRayOrigin, ref Vector3 worldHitPosition, ref Vector3 worldHitNormal) {
        Assert.IsTrue(false);
        return false;
    }

    protected override bool GetInputFloat4Impl(byte deviceID, byte controlID, ref Vector4 value) {
        Assert.IsNotNull(owner);
        return onairvr_GetInputFloat4(owner.playerID, deviceID, controlID, ref value.x, ref value.y, ref value.z, ref value.w);
    }

    protected override bool GetInputFloat3Impl(byte deviceID, byte controlID, ref Vector3 value) {
        Assert.IsNotNull(owner);
        return onairvr_GetInputFloat3(owner.playerID, deviceID, controlID, ref value.x, ref value.y, ref value.z);
    }

    protected override bool GetInputFloat2Impl(byte deviceID, byte controlID, ref Vector2 value) {
        Assert.IsNotNull(owner);
        return onairvr_GetInputFloat2(owner.playerID, deviceID, controlID, ref value.x, ref value.y);
    }

    protected override bool GetInputFloatImpl(byte deviceID, byte controlID, ref float value) {
        Assert.IsNotNull(owner);
        return onairvr_GetInputFloat(owner.playerID, deviceID, controlID, ref value);
    }

    protected override void SendPendingInputEventsImpl() {
        Assert.IsTrue(owner != null && owner.isBoundToClient);
        onairvr_SendPendingInputs(owner.playerID);
    }

    protected override void ResetInputImpl() {
        Assert.IsTrue(owner != null && owner.isBoundToClient);
        onairvr_ResetInput(owner.playerID);
    }
}
