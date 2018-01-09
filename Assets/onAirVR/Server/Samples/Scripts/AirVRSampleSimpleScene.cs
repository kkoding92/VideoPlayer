/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AirVRSampleSimpleScene : MonoBehaviour, AirVRCameraRigManager.EventHandler {
    private const string PointerSampleSceneName = "B. Event System (experimental)";

    private bool _sceneBeingUnloaded;

    public AirVRCameraRig cameraRig;
    public AudioSource music;

    private IEnumerator loadScene(string sceneName) {
        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, false, 0.5f));
        SceneManager.LoadScene(sceneName);
    }

    void Awake() {
        AirVRCameraRigManager.managerOnCurrentScene.Delegate = this;
    }

    IEnumerator Start() {
        yield return StartCoroutine(AirVRCameraFade.FadeAllCameras(this, true, 0.5f));
    }

    void Update() {
        if (_sceneBeingUnloaded == false && AirVRInput.GetDown(cameraRig, AirVRInput.Touchpad.Button.Back)) {
            _sceneBeingUnloaded = true;
            StartCoroutine(loadScene(PointerSampleSceneName));
        }
    }

    // implements AirVRCameraRigManager.EventHandler
    public void AirVRCameraRigWillBeBound(int clientHandle, AirVRClientConfig config, List<AirVRCameraRig> availables, out AirVRCameraRig selected) {
        selected = availables.Count > 0 ? availables[0] : null;

        if (selected) {
            AirVRSamplePlayer player = selected.GetComponentInParent<AirVRSamplePlayer>();
            player.EnableInteraction(true);

            music.Play();
        }
    }

    public void AirVRCameraRigActivated(AirVRCameraRig cameraRig) {}
    public void AirVRCameraRigDeactivated(AirVRCameraRig cameraRig) {}

    public void AirVRCameraRigHasBeenUnbound(AirVRCameraRig cameraRig) {
        // NOTE : This event occurs in OnDestroy() of AirVRCameraRig during unloading scene.
        //        You should be careful because some objects in the scene might be destroyed already on this event.
        AirVRSamplePlayer player = cameraRig.GetComponentInParent<AirVRSamplePlayer>();
        if (player != null) {
            player.EnableInteraction(false);
        }
        if (music != null) {
            music.Stop();
        }
    }
}
