/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;

public abstract class AirVRDeviceFeedback : AirVRInputSender {}

public abstract class AirVRTrackedDeviceFeedback : AirVRDeviceFeedback {
    public AirVRTrackedDeviceFeedback(Texture2D cookieTexture, float cookieDepthScaleMultiplier) {
#if UNITY_2017_1_OR_NEWER
        this.cookieTexture = ImageConversion.EncodeToPNG(cookieTexture);
#else
        this.cookieTexture = cookieTexture.EncodeToPNG();
#endif
        this.cookieDepthScaleMultiplier = cookieDepthScaleMultiplier;
    }

    private Vector3 _rayOrigin;
    private Vector3 _hitPosition;
    private Vector3 _hitNormal;

    protected abstract byte raycastHitResultKey { get; }

    public byte[] cookieTexture             { get; private set; }
    public float cookieDepthScaleMultiplier { get; private set; }

    public void SetRaycastResult(Vector3 rayOrigin, Vector3 hitPosition, Vector3 hitNormal) {
        _rayOrigin = rayOrigin;
        _hitPosition = hitPosition;
        _hitNormal = hitNormal;
    }

    // implements AirVRInputDeviceFeedback
    public override void PendInputsPerFrame(AirVRInputStream inputStream) {
        inputStream.PendTrackedDeviceFeedback(this, raycastHitResultKey, _rayOrigin, _hitPosition, _hitNormal);
    }
}

internal class AirVRHeadTrackerDeviceFeedback : AirVRTrackedDeviceFeedback {
    public AirVRHeadTrackerDeviceFeedback(Texture2D cookieTexture, float cookieDepthScaleMultiplier)
        : base(cookieTexture, cookieDepthScaleMultiplier) {}

    // implements AirVRPointerInputDeviceFeedback
    protected override byte raycastHitResultKey {
        get {
            return (byte)AirVRHeadTrackerKey.RaycastHitResult;
        }
    }

    public override string name {
        get {
            return AirVRInputDeviceName.HeadTracker;
        }
    }
}

public class AirVRTrackedControllerDeviceFeedback : AirVRTrackedDeviceFeedback {
    public AirVRTrackedControllerDeviceFeedback(Texture2D cookieTexture, float cookieDepthScaleMultiplier)
        : base(cookieTexture, cookieDepthScaleMultiplier) {}

    // implements AirVRPointerInputDeviceFeedback
    protected override byte raycastHitResultKey {
        get {
            return (byte)AirVRTrackedControllerKey.RaycastHitResult;
        }
    }

    public override string name {
        get {
            return AirVRInputDeviceName.TrackedController;
        }
    }
}
