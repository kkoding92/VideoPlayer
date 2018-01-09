using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICircleVRTransportEventHandler
{
    void OnData(byte key , byte[] data);
    void OnManualUpdate();
    void SendReliable(byte key , byte[] data);
    void SendUnreliable(byte key, byte[] data);
    void SendStateUpdate(byte key, byte[] data);
    void SendReliable(byte key);
    void SendUnreliable(byte key);
    void SendStateUpdate(byte key);
    void SendReliable(int connectionId , byte key, byte[] data);
    void SendUnreliable(int connectionId, byte key, byte[] data);
    void SendStateUpdate(int connectionId, byte key, byte[] data);
    void SendReliable(int connectionId, byte key);
    void SendUnreliable(int connectionId, byte key);
    void SendStateUpdate(int connectionId, byte key);
    string ByteToString(byte[] strByte);
    byte[] StringToByte(string str);
}
