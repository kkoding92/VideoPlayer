using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class TrackingData
{
    [Serializable]
    public class CircleVRTransform
    {
        public int onAirVRUserId;
        public Vector3 position;
        public Quaternion oriented;
    }

    public CircleVRTransform[] cvTransforms;
}

[Serializable]
public struct ClientData
{
    public int userId;

    public ClientData(int userId)
    {
        this.userId = userId;
    }
}

[Serializable]
public class ContentServerStatus
{
    public bool playing;
    public float elapseTime;

    public ContentServerStatus(bool playing , float elapseTime)
    {
        this.playing = playing;
        this.elapseTime = elapseTime;
    }
}

public abstract class CircleVRTransportBase
{
    public const int REC_BUFFER_SIZE = 1024;

    public readonly int circleVRHostId;
    public readonly int reliableChannel;
    public readonly int stateUpdateChannel;
    public readonly int unreliableChannel;

    private List<int> connectionIDs = new List<int>();

    public CircleVRTransportBase(int maxConnection)
    {
        CreateHost(out circleVRHostId, out reliableChannel, out stateUpdateChannel , out unreliableChannel, maxConnection);
    }

    public CircleVRTransportBase(int maxConnection, int port)
    {
        CreateHost(out circleVRHostId, out reliableChannel, out stateUpdateChannel, out unreliableChannel, port, maxConnection);
    }

    public string Deserialize(byte[] buffer, int recBufferSize)
    {
        byte[] recSizeBuffer = new byte[recBufferSize];

        for (int i = 0; i < recBufferSize; i++)
        {
            recSizeBuffer[i] = buffer[i];
        }

        return ByteToString(recSizeBuffer);
    }

    public void SendBroadcastReliable(string data)
    {
        foreach(int id in connectionIDs)
        {
            SendData(circleVRHostId, data, id, reliableChannel);
        }
    }

    public void SendBroadcastUnreliable(string data)
    {
        foreach (int id in connectionIDs)
        {
            SendData(circleVRHostId, data, id, unreliableChannel);
        }
    }

    public void SendBroadcastStateUpdate(string data)
    {
        foreach (int id in connectionIDs)
        {
            SendData(circleVRHostId, data, id, stateUpdateChannel);
        }
    }

    protected void SendStateUpdate(int connectionId ,string data)
    {
        SendData(circleVRHostId, data, connectionId, stateUpdateChannel);
    }

    protected void SendUnreliable(int connectionId , string data)
    {
        SendData(circleVRHostId, data, connectionId, unreliableChannel);
    }

    protected void SendReliable(int connectionId, string data)
    {
        SendData(circleVRHostId, data, connectionId, reliableChannel);
    }

    private HostTopology TransportInit(out int reliableChannelID, out int stateUpdateChannelId , out int unreliableChannelId, int maxConnection)
    {
        NetworkTransport.Init();
        ConnectionConfig connectionConfig = new ConnectionConfig();
        connectionConfig.FragmentSize = 1000;
        connectionConfig.PacketSize = 1470;
        reliableChannelID = connectionConfig.AddChannel(QosType.ReliableSequenced);
        stateUpdateChannelId = connectionConfig.AddChannel(QosType.StateUpdate);
        unreliableChannelId = connectionConfig.AddChannel(QosType.Unreliable);
        return new HostTopology(connectionConfig, maxConnection);
    }

    private void CreateHost(out int hostID, out int reliableChannelID , out int stateUpdateChannelId, out int unreliableChannelId, int maxConnection)
    {
        hostID = NetworkTransport.AddHost(TransportInit(out reliableChannelID, out stateUpdateChannelId,out unreliableChannelId, maxConnection));
 
    }

    private void CreateHost(out int hostID, out int reliableChannelID, out int stateUpdateChannelId, out int unreliableChannelId, int port, int maxConnection)
    {
        hostID = NetworkTransport.AddHost(TransportInit(out reliableChannelID, out stateUpdateChannelId, out unreliableChannelId, maxConnection), port);
    }

    public virtual void ManualUpdate()
    {
        int outConnectionId;
        int outChannelId;
        int outDataSize;

        byte[] buffer = new byte[REC_BUFFER_SIZE];

        byte error;

        NetworkEventType networkEvent = NetworkEventType.DataEvent;

        do
        {
            networkEvent = NetworkTransport.ReceiveFromHost(circleVRHostId, out outConnectionId, out outChannelId, buffer, REC_BUFFER_SIZE, out outDataSize, out error);
             
            switch (networkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    OnConnect(outConnectionId, error);
                    break;

                case NetworkEventType.DataEvent:
                    OnData(outConnectionId, outChannelId, buffer, outDataSize, error);
                    break;

                case NetworkEventType.DisconnectEvent:
                    OnDisconnect(outConnectionId, error);
                    break;
            }

        } while (networkEvent != NetworkEventType.Nothing);
    }

    protected virtual void OnConnect(int connectionId, byte error)
    {
        connectionIDs.Add(connectionId);

        Debug.Log("OnConnect(connectionId = "
            + connectionId + ", error = " + ((NetworkError)error).ToString() + ")");
    }

    protected virtual void OnDisconnect(int connectionId, byte error)
    {
        connectionIDs.Remove(connectionId);

        Debug.Log("OnDisconnect(connectionId = "
            + connectionId + ", error = " + ((NetworkError)error).ToString() + ")");
    }

    protected virtual void OnBroadcast(byte[] data, int size, byte error)
    {
        Debug.Log("OnBroadcast(data = "
            + data + ", size = " + size + ", error = " + ((NetworkError)error).ToString() + ")");
    }

    protected virtual void OnData(int connectionId, int channelId, byte[] data, int size, byte error)
    {
        //Debug.Log("OnDisconnect(hostId = " + hostId + ", connectionId = "
        //    + connectionId + ", channelId = " + channelId + ", data = "
        //    + data + ", size = " + size + ", error = " + ((NetworkError)error).ToString() + ")");
    }

    private void SendData(int hostId, string str, int connectionId, int channelId)
    {
        byte error;
        byte[] buffer = StringToByte(str);

        Debug.Assert(sizeof(byte) * buffer.Length <= REC_BUFFER_SIZE);

        NetworkTransport.Send(hostId, connectionId, channelId, buffer, buffer.Length, out error);
        NetworkError e = ((NetworkError)error);
        if (e == NetworkError.Ok)
            return;

        Debug.LogError(e.ToString());
        //CircleVRUI.Instance.Log = "\n" + e.ToString();
        //Debug.Log("Send : " + str + "\n" + "BufferSize : " + buffer.Length);
    }

    private string ByteToString(byte[] strByte)
    {
        string str = Encoding.UTF8.GetString(strByte);

        return str;
    }

    private byte[] StringToByte(string str)
    {
        byte[] strByte = Encoding.UTF8.GetBytes(str);

        return strByte;
    }
}

