using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public enum CircleVRPacketType : byte
{
    TrackingData = 230,
    Connected,
    Failed,
    ClientData,
    HostData,
    RequestHostData,
    RequestServerContent,
    Play,
    Stop,
    Pause,
    ContentClient,
    ContentServer
}

[Serializable]
public class TrackingData
{
    [Serializable]
    public class CircleVRTransform
    {
        public string onAirVRUserId;
        public Vector3 position;
        public Quaternion oriented;
    }

    public CircleVRTransform[] cvTransforms;
}

[Serializable]
public struct ClientData
{
    public string userId;

    public ClientData(string userId)
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

    private void Decompose(byte[] buffer, int recBufferSize , out byte key , out byte[] data)
    {
        data = new byte[recBufferSize -1];

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = buffer[i+1];
        }

        key = buffer[0];
    }

    public void SendBroadcastReliable(byte key , byte[] data)
    {
        foreach(int id in connectionIDs)
        {
            SendData(circleVRHostId, key , data, id, reliableChannel);
        }
    }

    public void SendBroadcastUnreliable(byte key, byte[] data)
    {
        foreach (int id in connectionIDs)
        {
            SendData(circleVRHostId, key, data, id, unreliableChannel);
        }
    }

    public void SendBroadcastStateUpdate(byte key, byte[] data)
    {
        foreach (int id in connectionIDs)
        {
            SendData(circleVRHostId, key, data, id, stateUpdateChannel);
        }
    }

    public void SendBroadcastReliable(byte key)
    {
        foreach (int id in connectionIDs)
        {
            SendData(circleVRHostId, key, id, reliableChannel);
        }
    }

    public void SendBroadcastUnreliable(byte key)
    {
        foreach (int id in connectionIDs)
        {
            SendData(circleVRHostId, key, id, unreliableChannel);
        }
    }

    public void SendBroadcastStateUpdate(byte key)
    {
        foreach (int id in connectionIDs)
        {
            SendData(circleVRHostId, key, id, stateUpdateChannel);
        }
    }

    public void SendBroadcastReliable(CircleVRPacketType type, byte[] data)
    {
        foreach (int id in connectionIDs)
        {
            SendData(circleVRHostId, (byte)type, data, id, reliableChannel);
        }
    }

    public void SendBroadcastUnreliable(CircleVRPacketType type, byte[] data)
    {
        foreach (int id in connectionIDs)
        {
            SendData(circleVRHostId, (byte)type, data, id, unreliableChannel);
        }
    }

    public void SendBroadcastStateUpdate(CircleVRPacketType type, byte[] data)
    {
        foreach (int id in connectionIDs)
        {
            SendData(circleVRHostId, (byte)type, data, id, stateUpdateChannel);
        }
    }

    public void SendStateUpdate(int connectionId , byte key, byte[] data)
    {
        SendData(circleVRHostId, key, data, connectionId, stateUpdateChannel);
    }

    public void SendUnreliable(int connectionId , byte key, byte[] data)
    {
        SendData(circleVRHostId, key, data, connectionId, unreliableChannel);
    }

    public void SendReliable(int connectionId, byte key, byte[] data)
    {
        SendData(circleVRHostId, key, data, connectionId, reliableChannel);
    }

    public void SendStateUpdate(int connectionId, CircleVRPacketType type, byte[] data)
    {
        SendData(circleVRHostId, (byte)type, data, connectionId, stateUpdateChannel);
    }

    public void SendUnreliable(int connectionId, CircleVRPacketType type, byte[] data)
    {
        SendData(circleVRHostId, (byte)type, data, connectionId, unreliableChannel);
    }

    public void SendReliable(int connectionId, CircleVRPacketType type, byte[] data)
    {
        SendData(circleVRHostId, (byte)type, data, connectionId, reliableChannel);
    }

    public void SendStateUpdate(int connectionId, CircleVRPacketType type)
    {
        SendData(circleVRHostId, (byte)type, connectionId, stateUpdateChannel);
    }

    public void SendUnreliable(int connectionId, CircleVRPacketType type)
    {
        SendData(circleVRHostId, (byte)type, connectionId, unreliableChannel);
    }

    public void SendReliable(int connectionId, CircleVRPacketType type)
    {
        SendData(circleVRHostId, (byte)type, connectionId, reliableChannel);
    }

    public void SendStateUpdate(int connectionId, byte key)
    {
        SendData(circleVRHostId, key, connectionId, stateUpdateChannel);
    }

    public void SendUnreliable(int connectionId, byte key)
    {
        SendData(circleVRHostId, key, connectionId, unreliableChannel);
    }

    public void SendReliable(int connectionId, byte key)
    {
        SendData(circleVRHostId, key, connectionId, reliableChannel);
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
                    {
                        byte key;
                        byte[] data;
                        Decompose(buffer, outDataSize , out key , out data);
                        OnData(outConnectionId, outChannelId, key, data, error);
                    }
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
    }

    protected virtual void OnDisconnect(int connectionId, byte error)
    {
        connectionIDs.Remove(connectionId);
    }

    protected virtual void OnBroadcast(byte[] data, int size, byte error)
    {
        Debug.Log("OnBroadcast(data = "
            + data + ", size = " + size + ", error = " + ((NetworkError)error).ToString() + ")");
    }

    protected virtual void OnData(int connectionId, int channelId, byte key, byte[] data, byte error)
    {
        //Debug.Log("OnDisconnect(hostId = " + hostId + ", connectionId = "
        //    + connectionId + ", channelId = " + channelId + ", data = "
        //    + data + ", size = " + size + ", error = " + ((NetworkError)error).ToString() + ")");
    }

    private void SendData(int hostId, byte key, byte[] data, int connectionId, int channelId)
    {
        byte[] buffer = new byte[data.Length + 1];

        buffer[0] = key;
        
        for(int i=1; i<buffer.Length; i++)
        {
            buffer[i] = data[i - 1];
        }

        SendData(hostId, buffer, connectionId, channelId);
    }

    private void SendData(int hostId, byte key, int connectionId, int channelId)
    {
        byte[] buffer = new byte[1];
        buffer[0] = key;
        SendData(hostId, buffer, connectionId, channelId);
    }

    private void SendData(int hostId, byte[] data, int connectionId, int channelId)
    {
        byte error;

        Debug.Assert(sizeof(byte) * data.Length <= REC_BUFFER_SIZE);

        NetworkTransport.Send(hostId, connectionId, channelId, data, data.Length, out error);
        NetworkError e = ((NetworkError)error);
        if (e == NetworkError.Ok)
            return;

        Debug.Log("[INFO] Send Error : " + e.ToString());
        CircleVRUI.Instance.Log = "\nSend Error : " + e.ToString();
        //Debug.Log("Send : " + str + "\n" + "BufferSize : " + buffer.Length);
    }

    public string ByteToString(byte[] strByte)
    {
        string str = Encoding.UTF8.GetString(strByte);

        return str;
    }

    public byte[] IntToByte(int data)
    {
        return BitConverter.GetBytes(data);
    }

    public byte[] StringToByte(string str)
    {
        byte[] strByte = Encoding.UTF8.GetBytes(str);

        return strByte;
    }
}

