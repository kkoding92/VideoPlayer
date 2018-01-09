/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using System;

public enum AirVRClientType {
    Monoscopic,
    Stereoscopic
}

[Serializable]
public class AirVRClientConfig {
	public AirVRClientConfig() {
        LeftEyeCameraNearPlane = new float[4];
    }

    [SerializeField] protected string UserID;
    [SerializeField] protected bool Stereoscopy;
    [SerializeField] protected int VideoWidth;
    [SerializeField] protected int VideoHeight;
    [SerializeField] protected float[] LeftEyeCameraNearPlane;
    [SerializeField] protected float FrameRate;
    [SerializeField] protected float InterpupillaryDistance;
    [SerializeField] protected Vector3 EyeCenterPosition;

    private Matrix4x4 makeProjection(float l, float t, float r, float b, float n, float f) {
        Matrix4x4 result = Matrix4x4.zero;
        result[0, 0] = 2 * n / (r - l);
        result[1, 1] = 2 * n / (t - b);
        result[0, 2] = (r + l) / (r - l);
        result[1, 2] = (t + b) / (t - b);
        result[2, 2] = (n + f) / (n - f);
        result[2, 3] = 2 * n * f / (n - f);
        result[3, 2] = -1.0f;

        return result;
    }

    internal Matrix4x4 GetLeftEyeCameraProjection(float near, float far) {
        return makeProjection(LeftEyeCameraNearPlane[0] * near, LeftEyeCameraNearPlane[1] * near, LeftEyeCameraNearPlane[2] * near, LeftEyeCameraNearPlane[3] * near, near, far);
    }

    internal Matrix4x4 GetRightEyeCameraProjection(float near, float far) {
        Matrix4x4 result = GetLeftEyeCameraProjection(near, far);
        result[0, 2] *= -1.0f;
        return result;
    }

    public AirVRClientType type {
        get {
            return Stereoscopy ? AirVRClientType.Stereoscopic : AirVRClientType.Monoscopic;
        }
    }

    public int videoWidth {
        get {
            return VideoWidth;
        }
    }

    public int videoHeight {
        get {
            return VideoHeight;
        }
    }

    public float framerate {
        get {

            return Mathf.Min(FrameRate, AirVRServer.serverParams.maxFrameRate);
        }
    }

    public float fov {
        get {
            float tAngle = Mathf.Atan(Mathf.Abs(LeftEyeCameraNearPlane[1]));
            float bAngle = Mathf.Atan(Mathf.Abs(LeftEyeCameraNearPlane[3]));
            return Mathf.Rad2Deg * (tAngle * Mathf.Sign(LeftEyeCameraNearPlane[1]) - bAngle * Mathf.Sign(LeftEyeCameraNearPlane[3]));
        }
    }

    public Vector3 eyeCenterPosition {
        get {
            return EyeCenterPosition;
        }
    }

    public float ipd {
        get {
            return InterpupillaryDistance;
        }
    }

    public string userID {
        get {
            return UserID;
        }
    }
}
