/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using UnityEngine.Assertions;

public sealed class AirVRStereoCameraRig : AirVRCameraRig, IAirVRTrackingModelContext {
    private readonly string TrackingSpaceName = "TrackingSpace";
    private readonly string LeftEyeAnchorName = "LeftEyeAnchor";
    private readonly string RightEyeAnchorName = "RightEyeAnchor";
    private readonly string CenterEyeAnchorName = "CenterEyeAnchor";
    private readonly int CameraLeftIndex = 0;
    private readonly int CameraRightIndex = 1;

    public enum TrackingModel {
        Head,
        InterpupillaryDistanceOnly,
        ExternalTracker,
        NoPositionTracking
    }

    private Matrix4x4 _worldToHMDSpaceMatrix;
    private Transform _trackingSpace;
    private Transform _leftEyeAnchor;
    private Transform _centerEyeAnchor;
    private Transform _rightEyeAnchor;

    private Camera[] _cameras;

    private AirVRTrackingModel _trackingModelObject;

    internal Transform trackingSpace {
        get {
            return _trackingSpace;
        }
    }

    public Camera leftEyeCamera {
        get {
            return _cameras[CameraLeftIndex];
        }
    }

    public Camera rightEyeCamera {
        get {
            return _cameras[CameraRightIndex];
        }
    }

    public Transform leftEyeAnchor {
        get {
            return _leftEyeAnchor;
        }
    }

    public Transform centerEyeAnchor {
        get {
            return _centerEyeAnchor;
        }
    }

    public Transform rightEyeAnchor {
        get {
            return _rightEyeAnchor;
        }
    }

    public TrackingModel trackingModel;
    public Transform externalTrackingOrigin;
    public Transform externalTracker;

    private TrackingModel trackingModelOf(AirVRTrackingModel trackingModelObject) {
        return trackingModelObject.GetType() == typeof(AirVRHeadTrackingModel)             ? TrackingModel.Head :
               trackingModelObject.GetType() == typeof(AirVRIPDOnlyTrackingModel)          ? TrackingModel.InterpupillaryDistanceOnly :
               trackingModelObject.GetType() == typeof(AirVRExternalTrackerTrackingModel)  ? TrackingModel.ExternalTracker :
               trackingModelObject.GetType() == typeof(AirVRNoPotisionTrackingModel)       ? TrackingModel.NoPositionTracking : TrackingModel.Head;
    }

    private AirVRTrackingModel createTrackingModelObject(TrackingModel model) {
        return model == TrackingModel.InterpupillaryDistanceOnly ?  new AirVRIPDOnlyTrackingModel(this, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor) :
               model == TrackingModel.ExternalTracker            ?  new AirVRExternalTrackerTrackingModel(this, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor, externalTrackingOrigin, externalTracker) :
               model == TrackingModel.NoPositionTracking         ?  new AirVRNoPotisionTrackingModel(this, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor) :
                                                                    new AirVRHeadTrackingModel(this, leftEyeAnchor, centerEyeAnchor, rightEyeAnchor) as AirVRTrackingModel;
    }

    private void updateTrackingModel() {
        if (_trackingModelObject == null || trackingModelOf(_trackingModelObject) != trackingModel) {
            _trackingModelObject = createTrackingModelObject(trackingModel);
        }
        if (trackingModelOf(_trackingModelObject) == TrackingModel.ExternalTracker) {
            AirVRExternalTrackerTrackingModel model = _trackingModelObject as AirVRExternalTrackerTrackingModel;
            model.trackingOrigin = externalTrackingOrigin;
            model.tracker = externalTracker;
        }
    }

    // implements AirVRCameraRig
    private bool ensureCameraObjectIntegrity(Transform xform) {
        if (xform.gameObject.GetComponent<Camera>() == null) {
            xform.gameObject.AddComponent<Camera>();
            return false;
        }
        return true;
    }

    protected override void ensureGameObjectIntegrity() {
        if (_trackingSpace == null) {
            _trackingSpace = getOrCreateGameObject(TrackingSpaceName, transform);
        }
        if (_leftEyeAnchor == null) {
            _leftEyeAnchor = getOrCreateGameObject(LeftEyeAnchorName, _trackingSpace);
        }
        if (_centerEyeAnchor == null) {
            _centerEyeAnchor = getOrCreateGameObject(CenterEyeAnchorName, _trackingSpace);
        }
        if (_rightEyeAnchor == null) {
            _rightEyeAnchor = getOrCreateGameObject(RightEyeAnchorName, _trackingSpace);
        }

        bool updateCamera = false;
        if (_cameras == null) {
            _cameras = new Camera[2];
            updateCamera = true;
        }

        if (ensureCameraObjectIntegrity(_leftEyeAnchor) == false || updateCamera) {
            _cameras[CameraLeftIndex] = _leftEyeAnchor.GetComponent<Camera>();
        }
        if (ensureCameraObjectIntegrity(_rightEyeAnchor) == false || updateCamera) {
            _cameras[CameraRightIndex] = _rightEyeAnchor.GetComponent<Camera>();
        }
    }

    protected override void init() {
        if (_trackingModelObject == null) {
            _trackingModelObject = createTrackingModelObject(trackingModel);
        }
    }

    protected override void setupCamerasOnBound(AirVRClientConfig config) {
        leftEyeCamera.projectionMatrix = config.GetLeftEyeCameraProjection(leftEyeCamera.nearClipPlane, leftEyeCamera.farClipPlane);
        rightEyeCamera.projectionMatrix = config.GetRightEyeCameraProjection(rightEyeCamera.nearClipPlane, rightEyeCamera.farClipPlane);
    }

    protected override void onStartRender() {
        updateTrackingModel();
        _trackingModelObject.StartTracking();
    }

    protected override void onStopRender() {
        updateTrackingModel();
        _trackingModelObject.StopTracking();
    }

    protected override void updateCameraTransforms(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation) {
        updateTrackingModel();
        _trackingModelObject.UpdateEyePose(config, centerEyePosition, centerEyeOrientation);
    }

    internal override Matrix4x4 clientSpaceToWorldMatrix {
        get {
            Assert.IsNotNull(_trackingModelObject);
            return _trackingModelObject.HMDSpaceToWorldMatrix;
        }
    }

    internal override Transform headPose {
        get {
            return centerEyeAnchor;
        }
    }

    internal override Camera[] cameras {
        get {
            return _cameras;
        }
    }

    // implements IAirVRTrackingModelContext
    void IAirVRTrackingModelContext.RecenterCameraRigPose() {
        RecenterPose();
    }
}
