/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;

public sealed class AirVRMonoCameraRig : AirVRCameraRig {
    private readonly string CameraAnchorName = "CameraAnchor";

    private Transform _thisTransform;
    private Transform _cameraAnchor;
    private Camera[] _cameras;

    public new Camera camera {
        get {
            return _cameras[0];
        }
    }

    // implements AirVRCameraRig
    protected override void ensureGameObjectIntegrity() {
        if (_thisTransform == null) {
            _thisTransform = transform;
        }

        bool updateCamera = false;
        if (_cameras == null) {
            _cameras = new Camera[1];
            updateCamera = true;
        }

        if (_cameraAnchor == null) {
            _cameraAnchor = getOrCreateGameObject(CameraAnchorName, transform);
        }
        if (_cameraAnchor.GetComponent<Camera>() == null) {
            _cameraAnchor.gameObject.AddComponent<Camera>();
            updateCamera = true;
        }

        if (updateCamera) {
            _cameras[0] = _cameraAnchor.GetComponent<Camera>();
        }
    }

    protected override void setupCamerasOnBound(AirVRClientConfig config) {
        _cameras[0].projectionMatrix = config.GetLeftEyeCameraProjection(_cameras[0].nearClipPlane, _cameras[0].farClipPlane);
    }

    protected override void updateCameraTransforms(AirVRClientConfig config, Vector3 centerEyePosition, Quaternion centerEyeOrientation) {
        _cameraAnchor.localRotation = centerEyeOrientation;
        _cameraAnchor.localPosition = centerEyePosition;
    }

    internal override Matrix4x4 clientSpaceToWorldMatrix {
        get {
            return _thisTransform.localToWorldMatrix;
        }
    }

    internal override Transform headPose {
        get {
            return _cameras != null ? _cameras[0].transform : null;
        }
    }

    internal override Camera[] cameras {
        get {
            return _cameras;
        }
    }
}
