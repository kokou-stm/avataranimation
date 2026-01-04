using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    public int port = 5052;
    public float scale = 5f;
    public float pointSize = 0.02f;

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    private float[] values;

    private const int POSE_FLOATS = 33 * 4;   
    private const int FACE_FLOATS = 468 * 3;   
    private const int HAND_FLOATS = 21 * 3;    
    private const int TOTAL_FLOATS = 1662;

    void Start()
    {
        udpClient = new UdpClient(port);
        remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        udpClient.BeginReceive(ReceiveData, null);

        Debug.Log(" UDP Receiver started");
    }

    void ReceiveData(IAsyncResult result)
    {
        byte[] data = udpClient.EndReceive(result, ref remoteEndPoint);

        if (data.Length != TOTAL_FLOATS * 4)
        {
            udpClient.BeginReceive(ReceiveData, null);
            return;
        }

        values = new float[TOTAL_FLOATS];
        Buffer.BlockCopy(data, 0, values, 0, data.Length);

        udpClient.BeginReceive(ReceiveData, null);
    }

    void OnDrawGizmos()
    {
        if (values == null) return;

        // POSE (vert)
        Gizmos.color = Color.green;
        DrawPoints(0, POSE_FLOATS, 4);

        // FACE (cyan)
        Gizmos.color = Color.cyan;
        DrawPoints(POSE_FLOATS, FACE_FLOATS, 3);

        // LEFT HAND (yellow)
        Gizmos.color = Color.yellow;
        DrawPoints(POSE_FLOATS + FACE_FLOATS, HAND_FLOATS, 3);

        // RIGHT HAND (red)
        Gizmos.color = Color.red;
        DrawPoints(POSE_FLOATS + FACE_FLOATS + HAND_FLOATS, HAND_FLOATS, 3);
    }

    void DrawPoints(int offset, int count, int stride)
    {
        for (int i = 0; i < count; i += stride)
        {
            float x = values[offset + i];
            float y = values[offset + i + 1];
            float z = values[offset + i + 2];

            Vector3 pos = ConvertToUnity(x, y, z);
            Gizmos.DrawSphere(pos, pointSize);
        }
    }

    Vector3 ConvertToUnity(float x, float y, float z)
    {
        // MediaPipe to  Unity
        float ux = (x - 0.5f) * scale;
        float uy = (0.5f - y) * scale;
        float uz = -z * scale;

        return new Vector3(ux, uy, uz);
    }

    void OnApplicationQuit()
    {
        udpClient?.Close();
    }
}
