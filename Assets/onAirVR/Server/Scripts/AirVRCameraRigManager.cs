/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Runtime.InteropServices;

public class AirVRCameraRigManager : MonoBehaviour {
    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_AcceptPlayer(int playerID);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_Update();

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_Disconnect(int playerID);

    public interface EventHandler {
        void AirVRCameraRigWillBeBound(int clientHandle, AirVRClientConfig config, List<AirVRCameraRig> availables, out AirVRCameraRig selected);
        void AirVRCameraRigActivated(AirVRCameraRig cameraRig);
        void AirVRCameraRigDeactivated(AirVRCameraRig cameraRig);
        void AirVRCameraRigHasBeenUnbound(AirVRCameraRig cameraRig);
    }

    private static AirVRCameraRigManager _instanceOnCurrentScene;

    internal static void LoadOncePerScene() {
        if (_instanceOnCurrentScene == null) {
            _instanceOnCurrentScene = FindObjectOfType<AirVRCameraRigManager>();
            if (_instanceOnCurrentScene == null) {
                GameObject go = new GameObject("AirVRCameraRigManager");
                go.AddComponent<AirVRCameraRigManager>();
                Assert.IsTrue(_instanceOnCurrentScene != null);
            }
        }
    }

    internal static void UnloadOncePerScene() {
        if (_instanceOnCurrentScene != null) {
            _instanceOnCurrentScene = null;
        }
    }

    internal static bool CheckIfExistManagerOnCurrentScene() {
        return _instanceOnCurrentScene != null;
    }

    public static AirVRCameraRigManager managerOnCurrentScene {
        get {
            LoadOncePerScene();
            return _instanceOnCurrentScene;
        }
    }

    private AirVRCameraRigList _cameraRigList;
    private AirVRServerEventDispatcher _eventDispatcher;

    private AirVRCameraRig notifyCameraRigWillBeBound(int playerID) {
        AirVRClientConfig config = AirVRServerPlugin.GetConfig(playerID);

        List<AirVRCameraRig> cameraRigs = new List<AirVRCameraRig>();
        _cameraRigList.GetAvailableCameraRigs(config.type, cameraRigs);

        AirVRCameraRig selected = null;
        if (Delegate != null) {
            Delegate.AirVRCameraRigWillBeBound(playerID, config, cameraRigs, out selected);
            AirVRServerPlugin.SetConfig(playerID, config);
        }
        else if (cameraRigs.Count > 0) {
            selected = cameraRigs[0];
        }
        return selected;
    }

    private void unregisterAllCameraRigs(bool applicationQuit) {
        List<AirVRCameraRig> cameraRigs = new List<AirVRCameraRig>();
        _cameraRigList.GetAllRetainedCameraRigs(cameraRigs);

        foreach (var cameraRig in cameraRigs) {
            UnregisterCameraRig(cameraRig, applicationQuit);
        }
    }

    void Awake() {
        if (_instanceOnCurrentScene != null) {
            new UnityException("[ONAIRVR] ERROR: There must exist only one AirVRCameraRigManager at a time.");
        }
        _instanceOnCurrentScene = this;

        _cameraRigList = new AirVRCameraRigList();
        _eventDispatcher = new AirVRServerEventDispatcher();
    }

    void Start() {
        List<AirVRServerStreamHandover.Streams> streams = new List<AirVRServerStreamHandover.Streams>();
        AirVRServerStreamHandover.TakeAllStreamsHandedOverInPrevScene(streams);
        foreach (var item in streams) {
            AirVRCameraRig selected = notifyCameraRigWillBeBound(item.playerID);
            if (selected != null) {
                _cameraRigList.RetainCameraRig(selected);
                selected.BindPlayer(item.playerID, item.mediaStream, item.inputStream);

                if (selected.isStreaming && Delegate != null) {
                    Delegate.AirVRCameraRigActivated(selected);
                }
            }
            else {
                onairvr_Disconnect(item.playerID);
            }
        }

        _eventDispatcher.MessageReceived += onAirVRMessageReceived;
    }

    void Update() {
        onairvr_Update();

        _eventDispatcher.DispatchEvent();
        List<AirVRCameraRig> cameraRigs = new List<AirVRCameraRig>();
        _cameraRigList.GetAllCameraRigs(cameraRigs);
        foreach (var cameraRig in cameraRigs) {
            cameraRig.OnUpdate();
        }
    }

    void LateUpdate() {
        List<AirVRCameraRig> cameraRigs = new List<AirVRCameraRig>();
        _cameraRigList.GetAllCameraRigs(cameraRigs);
        foreach (var cameraRig in cameraRigs) {
            cameraRig.OnLateUpdate();
        }
    }

    void OnApplicationQuit() {
        unregisterAllCameraRigs(true);
    }

    void OnDestroy() {
        unregisterAllCameraRigs(false);

        _eventDispatcher.MessageReceived -= onAirVRMessageReceived;
        UnloadOncePerScene();
    }

    internal AirVRServerEventDispatcher eventDispatcher {
        get {
            return _eventDispatcher;
        }
    }

    internal void RegisterCameraRig(AirVRCameraRig cameraRig) {
        _cameraRigList.AddUnboundCameraRig(cameraRig);
    }

    internal void UnregisterCameraRig(AirVRCameraRig cameraRig, bool applicationQuit = false) {
        _cameraRigList.RemoveCameraRig(cameraRig);

        if (applicationQuit == false && cameraRig.isBoundToClient) {
            cameraRig.PreHandOverStreams();
            AirVRServerStreamHandover.HandOverStreamsForNextScene(new AirVRServerStreamHandover.Streams(cameraRig.playerID, cameraRig.mediaStream, cameraRig.inputStream));

            if (Delegate != null) {
                if (cameraRig.isStreaming) {
                    Delegate.AirVRCameraRigDeactivated(cameraRig);
                }
                Delegate.AirVRCameraRigHasBeenUnbound(cameraRig);
            }
            cameraRig.PostHandOverStreams();
        }
    }

    public EventHandler Delegate { private get; set; }

    // handle AirVRMessages
    private void onAirVRMessageReceived(AirVRMessage message) {
        AirVRServerMessage serverMessage = message as AirVRServerMessage;
        int playerID = serverMessage.source.ToInt32();

        if (serverMessage.IsSessionEvent()) {
            if (serverMessage.Name.Equals(AirVRServerMessage.NameConnected)) {
                onAirVRSessionConnected(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameDisconnected)) {
                onAirVRSessionDisconnected(playerID, serverMessage);
            }
        }
        else if (serverMessage.IsPlayerEvent()) {
            if (serverMessage.Name.Equals(AirVRServerMessage.NameCreated)) {
                onAirVRPlayerCreated(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameActivated)) {
                onAirVRPlayerActivated(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameDeactivated)) {
                onAirVRPlayerDeactivated(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameDestroyed)) {
                onAirVRPlayerDestroyed(playerID, serverMessage);
            }
            else if (serverMessage.Name.Equals(AirVRServerMessage.NameShowCopyright)) {
                onAirVRPlayerShowCopyright(playerID, serverMessage);
            }
        }
    }

    private void onAirVRSessionConnected(int playerID, AirVRServerMessage message) {
        AirVRServer.NotifyClientConnected(playerID);
    }

    private void onAirVRPlayerCreated(int playerID, AirVRServerMessage message) {
        AirVRCameraRig selected = notifyCameraRigWillBeBound(playerID);
        if (selected != null) {
            _cameraRigList.RetainCameraRig(selected);
            selected.BindPlayer(playerID);

            onairvr_AcceptPlayer(playerID);
        }
        else {
            onairvr_Disconnect(playerID);
        }
    }

    private void onAirVRPlayerActivated(int playerID, AirVRServerMessage message) {
        AirVRCameraRig cameraRig = _cameraRigList.GetBoundCameraRig(playerID);
        if (cameraRig != null && Delegate != null) {
            Delegate.AirVRCameraRigActivated(cameraRig);
        }
    }

    private void onAirVRPlayerDeactivated(int playerID, AirVRServerMessage message) {
        AirVRCameraRig cameraRig = _cameraRigList.GetBoundCameraRig(playerID);
        if (cameraRig != null && Delegate != null) {
            Delegate.AirVRCameraRigDeactivated(cameraRig);
        }
    }

    private void onAirVRPlayerDestroyed(int playerID, AirVRServerMessage message) {
        AirVRCameraRig unboundCameraRig = _cameraRigList.GetBoundCameraRig(playerID);
        if (unboundCameraRig != null) {
            if (unboundCameraRig.isStreaming && Delegate != null) {
                Delegate.AirVRCameraRigDeactivated(unboundCameraRig);
            }

            unboundCameraRig.UnbindPlayer();
            _cameraRigList.ReleaseCameraRig(unboundCameraRig);

            if (Delegate != null) {
                Delegate.AirVRCameraRigHasBeenUnbound(unboundCameraRig);
            }
        }
    }

    private void onAirVRPlayerShowCopyright(int playerID, AirVRServerMessage message) {
        Debug.Log("(C) 2016-2018 onAirVR. All right reserved.");
    }

    private void onAirVRSessionDisconnected(int playerID, AirVRServerMessage message) {
        AirVRServer.NotifyClientDisconnected(playerID);
    }
}
