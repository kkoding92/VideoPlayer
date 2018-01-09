/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]

public class AirVRCameraFade : MonoBehaviour {
    private static List<AirVRCameraFade> _cameraFades = new List<AirVRCameraFade>();

    public static IEnumerator FadeAllCameras(MonoBehaviour caller, bool fadeIn, float duration) {
        foreach (AirVRCameraFade cameraFade in _cameraFades) {
            caller.StartCoroutine(cameraFade.Fade(fadeIn, duration));
        }

        for (bool anyCameraIsFading = true; anyCameraIsFading;) {
            anyCameraIsFading = false;
            foreach (AirVRCameraFade cameraFade in _cameraFades) {
                anyCameraIsFading = anyCameraIsFading || cameraFade.isFading;
                if (anyCameraIsFading) {
                    break;
                }
            }
            if (anyCameraIsFading) {
                yield return null;
            }
        }
    }

    public static void FadeAllCamerasImmediately(bool fadeIn) {
        foreach (AirVRCameraFade cameraFade in _cameraFades) {
            cameraFade.FadeImmediately(fadeIn);
        }
    }

    private Material _fadeMaterial;
    private Color _startFadeColor;
    private Color _endFadeColor;
    private float _startTimeToFade;

    [SerializeField]
    internal Color fadeOutColor = Color.black;

    private void Awake() {
        _fadeMaterial = new Material(Shader.Find("onAirVR/Unlit transparent color"));
        _fadeMaterial.color = Color.clear;

        _cameraFades.Add(this);
    }

    private void OnDestroy() {
        _cameraFades.Remove(this);
    }

    private void OnPostRender() {
        if (_fadeMaterial.color != Color.clear) {
            _fadeMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Color(_fadeMaterial.color);
            GL.Begin(GL.QUADS);
            GL.Vertex3(0.0f, 0.0f, -1.0f);
            GL.Vertex3(0.0f, 1.0f, -1.0f);
            GL.Vertex3(1.0f, 1.0f, -1.0f);
            GL.Vertex3(1.0f, 0.0f, -1.0f);
            GL.End();
            GL.PopMatrix();
        }
    }

    public bool isFading { get; private set; }

    public IEnumerator Fade(bool fadeIn, float duration) {
        _startFadeColor = isFading ? _fadeMaterial.color : (fadeIn ? fadeOutColor : Color.clear);
        _endFadeColor = fadeIn ? Color.clear : fadeOutColor;

        _startTimeToFade = Time.realtimeSinceStartup;

        if (isFading == false) {
            isFading = true;
            _fadeMaterial.color = _startFadeColor;
            while (_fadeMaterial.color != _endFadeColor) {
                _fadeMaterial.color = Color.Lerp(_startFadeColor, _endFadeColor, (Time.realtimeSinceStartup - _startTimeToFade) / duration);
                yield return null;
            }
            isFading = false;
        }
    }

    public void FadeImmediately(bool fadeIn) {
        _startFadeColor = fadeIn ? fadeOutColor : Color.clear;
        _endFadeColor = fadeIn ? Color.clear : fadeOutColor;

        _fadeMaterial.color = _endFadeColor;
    }
}
