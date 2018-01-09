/**********************************************************************************************

  Copyright 2017-2018 Clicked, Inc. All right reserved.

  Licensed under the onAirVR Server Software License.
  You may obtain a copy of the License at https://onairvr.io/downloads/licenses/onairvrserver.

 **********************************************************************************************/

using UnityEngine;
using System;
using System.Runtime.InteropServices;

public abstract class AirVREventDispatcher {
    public delegate void MessageReceiveHandler(AirVRMessage message);
    public event MessageReceiveHandler MessageReceived;

    protected abstract AirVRMessage ParseMessageImpl(IntPtr source, string message);
    protected abstract bool CheckMessageQueueImpl(out IntPtr source, out IntPtr data, out int length);
    protected abstract void RemoveFirstMessageFromQueueImpl();

    protected virtual void OnMessageReceived(AirVRMessage message) {
        if (MessageReceived != null) {
            MessageReceived(message);
        }
    }

    public void DispatchEvent() {
        if (Application.platform != RuntimePlatform.Android || Application.isEditor == false) {
            IntPtr source = default(IntPtr);
            IntPtr data = default(IntPtr);
            int length = 0;

            while (CheckMessageQueueImpl(out source, out data, out length)) {
                byte[] array = new byte[length];
                Marshal.Copy(data, array, 0, length);
                RemoveFirstMessageFromQueueImpl();

                OnMessageReceived(ParseMessageImpl(source, System.Text.Encoding.UTF8.GetString(array, 0, length)));
            }
        }
    }
}
