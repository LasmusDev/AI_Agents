using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PythonMessenger : MonoBehaviour
{
    TcpClient client;
    NetworkStream stream;
    public bool shutDownServer = false;
    public AudioSource outputSource;
    List<float> pendingAudio = new List<float>();
    object lockObj = new object();
    AudioClip clip;
    async void Start()
    {
        client = new TcpClient("127.0.0.1", 65432);
        stream = client.GetStream();
        Task.Run(GetResponses);        
    }

    public async Task SendText(string msg)
    {
        byte[] payload = Encoding.UTF8.GetBytes(msg);

        // 4-byte length prefix (big-endian)
        byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthPrefix);
        await stream.WriteAsync(lengthPrefix, 0, 4);
        await stream.WriteAsync(payload, 0, payload.Length);
    }

    public void Update()
    {
        if (pendingAudio != null)
        {
            float[] data;


            if (outputSource.isPlaying){
                return;
            }
            lock (lockObj)
            {
                data = pendingAudio.ToArray();
                pendingAudio.Clear();
            }
            if(data.Length == 0){
                return;
            }

            clip = AudioClip.Create("ReceivedAudio", data.Length, 1, 24000, false);
            clip.SetData(data, 0);
            outputSource.clip = clip;
            outputSource.Stop();
            outputSource.time = 0;
            outputSource.Play();
        }
    }

    async Task GetResponses()
    {
        while (shutDownServer == false)
        {
            byte[] lenBuffer = await ReadExact(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenBuffer);

            int responseLength = BitConverter.ToInt32(lenBuffer, 0);

            // Read response payload
            byte[] responseBuffer = await ReadExact(responseLength);
            Debug.Log(responseBuffer.Length);
            float[] floatArray = new float[responseBuffer.Length / 4];

            Buffer.BlockCopy(responseBuffer, 0, floatArray, 0, responseBuffer.Length);
            lock (lockObj)
            {
                pendingAudio.AddRange(floatArray);
            }
        }
    }

    async Task<byte[]> ReadExact(int n)
    {
        byte[] buffer = new byte[n];
        int totalRead = 0;

        while (totalRead < n)
        {
            int bytesRead = await stream.ReadAsync(buffer, totalRead, n - totalRead);
            if (bytesRead == 0)
                throw new Exception("Connection closed");

            totalRead += bytesRead;
        }

        return buffer;
    }

    void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
    }

    
}
