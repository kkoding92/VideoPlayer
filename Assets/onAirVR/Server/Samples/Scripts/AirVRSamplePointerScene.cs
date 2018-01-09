/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AirVRSamplePointerScene : MonoBehaviour {
    private const string BasicSampleSceneName = "A. Basic";

    private bool _loadingBasicScene;

    private IEnumerator loadScene(string sceneName) {
        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, false, 0.5f));
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator Start() {
        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, true, 0.5f));
    }

    public void GoToBasicScene() {
        if (_loadingBasicScene == false) {
            _loadingBasicScene = true;

            StartCoroutine(loadScene(BasicSampleSceneName));
        }
    }
}
