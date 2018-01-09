/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using System.Runtime.InteropServices;
using System;
using UnityEngine;

internal class AirVRServerPlugin {
    public const string Name = "onAirVRServerPlugin";
    public const string AudioPluginName = "AudioPlugin_onAirVRServerPlugin";

    private const uint AirVRRenderEventMaskPlayerID = 0xFF000000;
    private const uint AirVRRenderEventMaskArg1 = 0x00FF0000;
    private const uint AirVRRenderEventMaskArg2 = 0x0000FFFF;

    [DllImport(Name)]
    private static extern bool onairvr_GetConfig(int playerID, out IntPtr data, out int length);

    [DllImport(Name)]
    private static extern void onairvr_SetConfig(int playerID, string json);

    public static int RenderEventArg(uint playerID, uint data = 0) {
        return (int)((playerID << 24) & AirVRRenderEventMaskPlayerID) + (int)(data & (AirVRRenderEventMaskArg1 | AirVRRenderEventMaskArg2));
    }

    public static int RenderEventArg(uint playerID, uint arg1, uint arg2) {
        return (int)((playerID << 24) & AirVRRenderEventMaskPlayerID) + (int)((arg1 << 16) & AirVRRenderEventMaskArg1) + (int)(arg2 & AirVRRenderEventMaskArg2);
    }

    public static AirVRClientConfig GetConfig(int playerID) {
        IntPtr data = default(IntPtr);
        int length = 0;
        if (onairvr_GetConfig(playerID, out data, out length)) {
            byte[] array = new byte[length];
            Marshal.Copy(data, array, 0, length);
            return JsonUtility.FromJson<AirVRClientConfig>(System.Text.Encoding.UTF8.GetString(array, 0, length)); 
        }
        return null;
    }

    public static void SetConfig(int playerID, AirVRClientConfig config) {
        onairvr_SetConfig(playerID, JsonUtility.ToJson(config));
    }
}
