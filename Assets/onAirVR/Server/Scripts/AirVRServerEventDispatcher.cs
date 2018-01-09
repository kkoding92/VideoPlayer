/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class AirVRServerEventDispatcher : AirVREventDispatcher {
    [DllImport(AirVRServerPlugin.Name)]
    private static extern bool onairvr_CheckMessageQueue(out IntPtr source, out IntPtr data, out int length);

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void onairvr_RemoveFirstMessage();

    [DllImport(AirVRServerPlugin.Name)]
    private static extern void RemoveFirstMessageFromQueue();

    protected override AirVRMessage ParseMessageImpl(IntPtr source, string message) {
        AirVRServerMessage result = JsonUtility.FromJson<AirVRServerMessage>(message);
        result.source = source;

        return result;
    }

    protected override bool CheckMessageQueueImpl(out IntPtr source, out IntPtr data, out int length) {
        return onairvr_CheckMessageQueue(out source, out data, out length);
    }

    protected override void RemoveFirstMessageFromQueueImpl() {
        onairvr_RemoveFirstMessage();
    }
}
