/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;

public interface IAirVRTrackingModelContext {
    void RecenterCameraRigPose();
}

public abstract class AirVRTrackingModel {
    public AirVRTrackingModel(IAirVRTrackingModelContext context, Transform leftEyeAnchor, Transform centerEyeAnchor, Transform rightEyeAnchor) {
        this.context = context;
        this.leftEyeAnchor = leftEyeAnchor;
        this.centerEyeAnchor = centerEyeAnchor;
        this.rightEyeAnchor = rightEyeAnchor;

        HMDSpaceToWorldMatrix = centerEyeAnchor.parent.localToWorldMatrix;
    }

    protected IAirVRTrackingModelContext context    { get; private set; }
    protected Transform leftEyeAnchor               { get; private set; }
    protected Transform centerEyeAnchor             { get; private set; }
    protected Transform rightEyeAnchor              { get; private set; }

    protected virtual Quaternion HMDTrackingRootRotation {
        get {
            return centerEyeAnchor.parent.rotation;
        }
    }

    protected abstract void OnUpdateEyePose(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation);

    public Matrix4x4 HMDSpaceToWorldMatrix  { get; private set; }

    public virtual void StartTracking() {}

    public virtual void StopTracking() {
        HMDSpaceToWorldMatrix = centerEyeAnchor.parent.localToWorldMatrix;
    }

    public void UpdateEyePose(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation) {
        OnUpdateEyePose(config, centerEyePosition, centerEyeOrientation);

        Transform trackingRoot = centerEyeAnchor.parent;
        HMDSpaceToWorldMatrix = Matrix4x4.TRS(trackingRoot.localToWorldMatrix.MultiplyPoint(centerEyeAnchor.localPosition - centerEyePosition), 
                                              HMDTrackingRootRotation, 
                                              trackingRoot.lossyScale);
    }
}

public class AirVRHeadTrackingModel : AirVRTrackingModel {
    public AirVRHeadTrackingModel(IAirVRTrackingModelContext context, Transform leftEyeAnchor, Transform centerEyeAnchor, Transform rightEyeAnchor)
        : base(context, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor) {}

    // implements AirVRTrackingModel
    protected override void OnUpdateEyePose(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation) {
        centerEyeAnchor.localRotation = leftEyeAnchor.localRotation = rightEyeAnchor.localRotation = centerEyeOrientation;

        Vector3 left = config.eyeCenterPosition + Vector3.left * 0.5f * config.ipd;
        Vector3 right = config.eyeCenterPosition + Vector3.right * 0.5f * config.ipd;
        Vector3 adjustEyeHeightToZero = config.eyeCenterPosition.y * Vector3.down;

        leftEyeAnchor.localPosition = centerEyePosition + centerEyeOrientation * (Vector3.left * 0.5f * config.ipd);
        centerEyeAnchor.localPosition = centerEyePosition;
        rightEyeAnchor.localPosition = centerEyePosition + centerEyeOrientation * (Vector3.right * 0.5f * config.ipd);
    }
}

public class AirVRIPDOnlyTrackingModel : AirVRTrackingModel {
    public AirVRIPDOnlyTrackingModel(IAirVRTrackingModelContext context, Transform leftEyeAnchor, Transform centerEyeAnchor, Transform rightEyeAnchor)
        : base(context, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor) {}

    // implements AirVRTrackingModel
    protected override void OnUpdateEyePose(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation) {
        centerEyeAnchor.localRotation = leftEyeAnchor.localRotation = rightEyeAnchor.localRotation = centerEyeOrientation;

        leftEyeAnchor.localPosition = centerEyeOrientation * (Vector3.left * 0.5f * config.ipd);
        centerEyeAnchor.localPosition = Vector3.zero;
        rightEyeAnchor.localPosition = centerEyeOrientation * (Vector3.right * 0.5f * config.ipd);
    }
}

public class AirVRExternalTrackerTrackingModel : AirVRTrackingModel {
    public AirVRExternalTrackerTrackingModel(IAirVRTrackingModelContext context, Transform leftEyeAnchor, Transform centerEyeAnchor, Transform rightEyeAnchor, Transform trackingOrigin, Transform tracker)
        : base(context, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor) {
        _trackingSpaceChanged = false;
        _trackingOrigin = trackingOrigin;
        _tracker = tracker;

        _localTrackerRotationOnIdentityHeadOrientation = Quaternion.identity;
    }

    private bool _trackingSpaceChanged;
    private Quaternion _localTrackerRotationOnIdentityHeadOrientation;
    private Transform _trackingOrigin;
    private Transform _tracker;

    private Quaternion trackingOriginRotation {
        get {
            return _trackingOrigin != null ? _trackingOrigin.rotation : Quaternion.identity;
        }
    }

    private bool needToUpdateTrackingSpace() {
        return _trackingSpaceChanged;
    }

    private void updateTrackingSpace() {
        if (_tracker != null) {
            context.RecenterCameraRigPose();
            _localTrackerRotationOnIdentityHeadOrientation = Quaternion.Euler(0.0f, (_tracker.rotation * (_trackingOrigin != null ? Quaternion.Inverse(_trackingOrigin.rotation) : Quaternion.identity)).eulerAngles.y, 0.0f);
        }
        _trackingSpaceChanged = false;
    }

    public Transform trackingOrigin {
        set {
            if (_trackingOrigin != value) {
                _trackingOrigin = value;
                _trackingSpaceChanged = true;
            }
        }
    }

    public Transform tracker {
        set {
            if (_tracker != value) {
                _tracker = value;
                _trackingSpaceChanged = true;
            }
        }
    }

    // implements AirVRTrackingModel

    protected override Quaternion HMDTrackingRootRotation {
        get {
            return trackingOriginRotation * _localTrackerRotationOnIdentityHeadOrientation;
        }
    }

    protected override void OnUpdateEyePose(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation) {
        if (needToUpdateTrackingSpace()) {
            updateTrackingSpace();
        }

        if (_tracker != null) {
            Quaternion worldHeadOrientation = HMDTrackingRootRotation * centerEyeOrientation;
            centerEyeAnchor.rotation = leftEyeAnchor.rotation = rightEyeAnchor.rotation = worldHeadOrientation;

            Vector3 cameraRigScale = centerEyeAnchor.parent.lossyScale;
            leftEyeAnchor.position = _tracker.position + worldHeadOrientation * (Vector3.Scale(Vector3.left, cameraRigScale) * 0.5f * config.ipd);
            centerEyeAnchor.position = _tracker.position;
            rightEyeAnchor.position = _tracker.position + worldHeadOrientation * (Vector3.Scale(Vector3.right, cameraRigScale) * 0.5f * config.ipd);
        }
        else {
            centerEyeAnchor.localRotation = leftEyeAnchor.localRotation = rightEyeAnchor.localRotation = centerEyeOrientation;

            leftEyeAnchor.localPosition = centerEyeOrientation * (Vector3.left * 0.5f * config.ipd);
            centerEyeAnchor.localPosition = Vector3.zero;
            rightEyeAnchor.localPosition = centerEyeOrientation * (Vector3.right * 0.5f * config.ipd);
        }
    }

    public override void StartTracking() {
        updateTrackingSpace();
    }
}

public class AirVRNoPotisionTrackingModel : AirVRTrackingModel {
    public AirVRNoPotisionTrackingModel(IAirVRTrackingModelContext context, Transform leftEyeAnchor, Transform centerEyeAnchor, Transform rightEyeAnchor)
        : base(context, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor) {}

    // implements AirVRTrackingModel
    protected override void OnUpdateEyePose(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation) {
        centerEyeAnchor.localRotation = leftEyeAnchor.localRotation = rightEyeAnchor.localRotation = centerEyeOrientation;
    }
}
