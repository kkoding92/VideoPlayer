/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Runtime.InteropServices;

public class AirVRServerParams {
    public const float DefaultMaxFrameRate = 60.0f;
    public const float DefaultDefaultFrameRate = 30.0f;
    public const int DefaultVideoBitrate = 24000000;
    public const int DefaultMaxClientCount = 1;
    public const int DefaultPort = 9090;
    public const string DefaultLicenseFilePath = "noncommercial.license";

    public AirVRServerParams() {
        maxFrameRate = DefaultMaxFrameRate;
        defaultFrameRate = DefaultDefaultFrameRate;
        videoBitrate = DefaultVideoBitrate;
        maxClientCount = DefaultMaxClientCount;
        portSTAP = DefaultPort;
        licenseFilePath = DefaultLicenseFilePath;
    }

    public AirVRServerParams(AirVRServerInitParams initParams) {
        maxFrameRate = initParams.maxFrameRate;
        defaultFrameRate = initParams.defaultFrameRate;
        videoBitrate = initParams.videoBitrate;
        maxClientCount = initParams.maxClientCount;
        portSTAP = initParams.port;
        licenseFilePath = initParams.licenseFilePath;
    }

    public float maxFrameRate           { get; private set; }
    public float defaultFrameRate       { get; private set; }
    public int videoBitrate             { get; private set; }
    public int maxClientCount           { get; private set; }
    public string licenseFilePath       { get; private set; }
    public int portSTAP                 { get; private set; }
    public int portAMP                  { get; private set; }
    public bool loopbackOnlyForSTAP     { get; private set; }
    public string userData              { get; private set; }
    public string groupServer           { get; private set; }

    private bool parsePort(string value, out int parsed) {
        return int.TryParse(value, out parsed) && 0 <= parsed && parsed <= 65535;
    }

    public void ParseCommandLineArgs(string[] args) {
        if (args == null) {
            return;
        }

        for (int i = 0; i < args.Length; i++) {
            int splitIndex = args[i].IndexOf("=");
            if (splitIndex <= 0) {
                continue;
            }

            string name = args[i].Substring(0, splitIndex);
            string value = args[i].Substring(splitIndex + 1);
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value)) {
                continue;
            }

            if (name.Equals("onairvr_stap_port")) {
                int parsed = portSTAP;
                if (parsePort(value, out parsed)) {
                    portSTAP = parsed;
                }
                else {
                    Debug.Log("[WARNING] STAP Port number of the command line argument is invalid : " + value);
                }
            }
            else if (name.Equals("onairvr_amp_port")) {
                int parsed = 0;
                if (parsePort(value, out parsed)) {
                    portAMP = parsed;
                }
                else {
                    Debug.Log("[WARNING] AMP Port number of the command line argument is invalid : " + value);
                }
            }
            else if (name.Equals("onairvr_loopback_only")) {
                loopbackOnlyForSTAP = value.Equals("true");
            }
            else if (name.Equals("onairvr_license")) {
                licenseFilePath = value;
            }
            else if (name.Equals("onairvr_video_bitrate")) {
                int parsed = 0;
                if (int.TryParse(value, out parsed) && parsed > 0) {
                    videoBitrate = parsed;
                }
            }
            else if (name.Equals("onairvr_user_data")) {
                userData = WWW.UnEscapeURL(value);
            }
            else if (name.Equals("onairvr_group_server")) {
                groupServer = value;
            }
        }
    }
}

public class AirVRServer : MonoBehaviour {
    private const int StartupErrorNotSupportdingGPU = -1;
    private const int StartupErrorLicenseNotYetVerified = -2;
    private const int StartupErrorLicenseFileNotFound = -3;
    private const int StartupErrorInvalidLicenseFile = -4;
    private const int StartupErrorLicenseExpired = -5;

    private const int GroupOfPictures = 60;

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_GetAirVRServerPluginPtr(ref System.IntPtr result);

    [DllImport(AirVRServerPlugin.AudioPluginName)]
    private static extern void onairvr_SetAirVRServerPluginPtr(System.IntPtr ptr);

    [DllImport(AirVRServerPlugin.Name, CharSet = CharSet.Ansi)]
    private static extern int onairvr_SetLicenseFile(string filePath);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern int onairvr_Startup(int maxConnectionCount, int portSTAP, int portAMP, bool loopbackOnlyForSTAP, int audioSampleRate);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern System.IntPtr onairvr_Startup_RenderThread_Func();

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_Shutdown();

    [DllImport(AirVRServerPlugin.Name)]
    private static extern IntPtr onairvr_Shutdown_RenderThread_Func();

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_SetVideoEncoderParameters(float maxFrameRate, float defaultFrameRate,
                                                                 int maxBitRate, int defaultBitRate, int gopCount);

    [DllImport(AirVRServerPlugin.AudioPluginName)]
    private static extern void onairvr_EncodeAudioFrame(int playerID, float[] data, int sampleCount, int channels, double timestamp);

    [DllImport(AirVRServerPlugin.AudioPluginName)]
    private static extern void onairvr_EncodeAudioFrameForAllPlayers(float[] data, int sampleCount, int channels, double timestamp);

    //[DllImport(AirVRServerPlugin.Name)]
    //private static extern System.IntPtr RenderServerShutdownFunc();

    public interface EventHandler {
        void AirVRServerFailed(string reason);
        void AirVRServerClientConnected(int clientHandle);
        void AirVRServerClientDisconnected(int clientHandle);
    }

    private static AirVRServer _instance;
    private static EventHandler _Delegate;

    internal static void NotifyClientConnected(int clientHandle) {
        if (_Delegate != null) {
            _Delegate.AirVRServerClientConnected(clientHandle);
        }
    }

    internal static void NotifyClientDisconnected(int clientHandle) {
        if (_Delegate != null) {
            _Delegate.AirVRServerClientDisconnected(clientHandle);
        }
    }

    internal static AirVRServerParams serverParams {
        get {
            Assert.IsNotNull(_instance);
            Assert.IsNotNull(_instance._serverParams);

            return _instance._serverParams;
        }
    }

    internal static void LoadOnce(AirVRServerInitParams initParams = null) {
        if (_instance == null) {
            GameObject go = new GameObject("AirVRServer");
            go.AddComponent<AirVRServer>();
            Assert.IsTrue(_instance != null);

            _instance._serverParams = (initParams != null) ? new AirVRServerParams(initParams) : new AirVRServerParams();
            _instance._serverParams.ParseCommandLineArgs(Environment.GetCommandLineArgs());
        }
    }

    public static EventHandler Delegate {
        set {
            _Delegate = value;
        }
    }

    public static void SendAudioFrame(AirVRCameraRig cameraRig, float[] data, int sampleCount, int channels, double timestamp) {
        if (cameraRig.isBoundToClient) {
            onairvr_EncodeAudioFrame(cameraRig.playerID, data, data.Length / channels, channels, AudioSettings.dspTime);
        }
    }

    public static void SendAudioFrameToAllCameraRigs(float[] data, int sampleCount, int channels, double timestamp) {
        onairvr_EncodeAudioFrameForAllPlayers(data, data.Length / channels, channels, AudioSettings.dspTime);
    }

    private bool _startedUp = false;
    private AirVRServerParams _serverParams;

    void Awake() {
        if (_instance != null) {
            new UnityException("[ONAIRVR] ERROR: There must exist only one AirVRServer instance.");
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        try {
            onairvr_SetLicenseFile(Application.isEditor ? System.IO.Path.Combine("Assets/onAirVR/Server/Editor/Misc", AirVRServerParams.DefaultLicenseFilePath) : serverParams.licenseFilePath);
            onairvr_SetVideoEncoderParameters(serverParams.maxFrameRate, serverParams.defaultFrameRate, serverParams.videoBitrate, serverParams.videoBitrate, GroupOfPictures);

            int startupResult = onairvr_Startup(serverParams.maxClientCount, serverParams.portSTAP, serverParams.portAMP, serverParams.loopbackOnlyForSTAP, AudioSettings.outputSampleRate);
            if (startupResult == 0) {   // no error
                System.IntPtr pluginPtr = System.IntPtr.Zero;
                onairvr_GetAirVRServerPluginPtr(ref pluginPtr);
                onairvr_SetAirVRServerPluginPtr(pluginPtr);

                GL.IssuePluginEvent(onairvr_Startup_RenderThread_Func(), 0);
                _startedUp = true;

                Debug.Log("[INFO] The ONAIRVR Server has started on port " + serverParams.portSTAP + ".");
            }
            else {
                string reason;
                switch (startupResult) {
                    case StartupErrorNotSupportdingGPU:
                        reason = "Graphic device is not supported";
                        break;
                    case StartupErrorLicenseNotYetVerified:
                        reason = "License is not yet verified";
                        break;
                    case StartupErrorLicenseFileNotFound:
                        reason = "License file not found";
                        break;
                    case StartupErrorInvalidLicenseFile:
                        reason = "Invalid license file";
                        break;
                    case StartupErrorLicenseExpired:
                        reason = "License expired";
                        break;
                    default:
                        reason = "Unknown error occurred";
                        break;
                }

                Debug.Log("[ONAIRVR] Failed to startup : " + reason);
                if (_Delegate != null) {
                    _Delegate.AirVRServerFailed(reason);
                }
            }
        }
        catch (System.DllNotFoundException) {
            if (_Delegate != null) {
                _Delegate.AirVRServerFailed("Failed to load onAirVR server plugin");
            }
        }
    }

    void OnDestroy() {
        if (_startedUp) {
            GL.IssuePluginEvent(onairvr_Shutdown_RenderThread_Func(), 0);
            GL.Flush();

            onairvr_Shutdown();
        }
    }
}
