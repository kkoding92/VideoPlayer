using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class CircleVRProtocol
{
    public const int REC_BUFFER_SIZE = 1024;

    public void SendData(int hostId, string str, int connectionId, int channelId)
    {
        byte error;
        byte[] buffer = StringToByte(str);

        Debug.Assert(sizeof(byte) * buffer.Length <= REC_BUFFER_SIZE);

        NetworkTransport.Send(hostId, connectionId, channelId, buffer, buffer.Length, out error);
        NetworkError e = ((NetworkError)error);
        if (e == NetworkError.Ok)
            return;

        Debug.LogError(e.ToString());
        CircleVR.Instance.Log = e.ToString();
        //Debug.Log("Send : " + str + "\n" + "BufferSize : " + buffer.Length);
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
