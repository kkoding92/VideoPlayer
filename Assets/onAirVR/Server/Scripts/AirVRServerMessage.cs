/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using UnityEngine.Assertions;
using System;

public class AirVRServerMessage : AirVRMessage {
    // Type : Event
    public const string FromSession = "Session";
    public const string FromPlayer = "Player";
    public const string FromMediaStream = "MediaStream";
    public const string FromInputStream = "InputStream";

    // Type : Event, From : Session
    public const string NameConnected = "Connected";
    public const string NameDisconnected = "Disconnected";

    // Type : Event, From : Player
    public const string NameCreated = "Created";
    public const string NameActivated = "Activated";
    public const string NameDeactivated = "Deactivated";
    public const string NameDestroyed = "Destroyed";
    public const string NameShowCopyright = "ShowCopyright";

    // Type : Event, From : MediaStream
    public const string NameInitialized = "Initialized";
    public const string NameStarted = "Started";
    public const string NameEncodeVideoFrame = "EncodeVideoFrame";
    public const string NameStopped = "Stopped";
    public const string NameCleanupUp = "CleanedUp";

    // Type : Event, From : InputStream
    public const string NameRemoteInputDeviceRegistered = "RemoteInputDeviceRegistered";
    public const string NameRemoteInputDeviceUnregistered = "RemoteInputDeviceUnregistered";

    public static AirVRServerMessage Parse(IntPtr source, string message) {
        AirVRServerMessage result = JsonUtility.FromJson<AirVRServerMessage>(message);

        Assert.IsFalse(string.IsNullOrEmpty(result.Type));
        result.source = source;
        result.postParse();
        return result;
    }

    // Type : Event
    public string From;
    public string Name;

    // Type : Event, From : InputStream, Name : RemoteInputDeviceRegistered / RemoteInputDeviceUnregistered
    public int DeviceID;

    // Type : Event, From : InputStream, Name : RemoteInputDeviceRegistered
    public string DeviceName;

    private bool isEventFrom(string fromWhat) {
        if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(From) || string.IsNullOrEmpty(Name)) {
            return false;
        }
        return Type.Equals(TypeEvent) && From.Equals(fromWhat);
    }

    public bool IsSessionEvent()        { return isEventFrom(FromSession); }
    public bool IsPlayerEvent()         { return isEventFrom(FromPlayer); }
    public bool IsMediaStreamEvent()    { return isEventFrom(FromMediaStream); }
    public bool IsInputStreamEvent()    { return isEventFrom(FromInputStream); }
}
