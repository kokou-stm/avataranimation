using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    [Header("UDP Settings")]
    public int port = 5052;

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    // MediaPipe Holistic constants
    private const int POSE_FLOATS = 33 * 4;   
    private const int FACE_FLOATS = 468 * 3;   
    private const int HAND_FLOATS = 21 * 3;    
    private const int TOTAL_FLOATS = POSE_FLOATS + FACE_FLOATS + HAND_FLOATS * 2;  

    void Start()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        udpClient = new UdpClient(port);

        Debug.Log($"📡 UDP Receiver started on port {port}");
        udpClient.BeginReceive(ReceiveData, null);
    }

    private void ReceiveData(IAsyncResult result)
    {
        byte[] data = udpClient.EndReceive(result, ref remoteEndPoint);

        // Safety check
        if (data.Length != TOTAL_FLOATS * 4)
        {
            Debug.LogWarning($"⚠️ Unexpected packet size: {data.Length} bytes");
            udpClient.BeginReceive(ReceiveData, null);
            return;
        }

        // Convert bytes → floats
        float[] values = new float[TOTAL_FLOATS];
        Buffer.BlockCopy(data, 0, values, 0, data.Length);

        // -------- DEBUG DISPLAY --------
        Debug.Log(
            $"POSE[0]  x={values[0]:F3}, y={values[1]:F3}, z={values[2]:F3}, vis={values[3]:F3}\n" +
            $"FACE[0]  x={values[POSE_FLOATS]:F3}, y={values[POSE_FLOATS + 1]:F3}, z={values[POSE_FLOATS + 2]:F3}\n" +
            $"LH[0]    x={values[POSE_FLOATS + FACE_FLOATS]:F3}, y={values[POSE_FLOATS + FACE_FLOATS + 1]:F3}, z={values[POSE_FLOATS + FACE_FLOATS + 2]:F3}\n" +
            $"RH[0]    x={values[POSE_FLOATS + FACE_FLOATS + HAND_FLOATS]:F3}, y={values[POSE_FLOATS + FACE_FLOATS + HAND_FLOATS + 1]:F3}, z={values[POSE_FLOATS + FACE_FLOATS + HAND_FLOATS + 2]:F3}"
        );
        // --------------------------------

        udpClient.BeginReceive(ReceiveData, null);
    }

    private void OnApplicationQuit()
    {
        udpClient?.Close();
        Debug.Log(" UDP Receiver stopped");
    }
}
