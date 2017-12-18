using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICircleVRTransportEventHandler
{
    void OnData(string data);
    void OnManualUpdate();
    void SendReliable(string data);
    void SendUnreliable(string data);
    void SendStateUpdate(string data);
}
