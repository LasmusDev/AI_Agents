using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PythonMessager : MonoBehaviour
{
    TcpClient client;
    NetworkStream stream;
    public bool shutDownServer = false;
    async void Start()
    {
        client = new TcpClient("127.0.0.1", 65432);
        stream = client.GetStream();

        await SendText("Hello, this is a testtext");
    }

    async Task SendText(string msg)
    {
        byte[] payload = Encoding.UTF8.GetBytes(msg);

        // 4-byte length prefix (big-endian)
        byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(lengthPrefix);
        await stream.WriteAsync(lengthPrefix, 0, 4);
        await stream.WriteAsync(payload, 0, payload.Length);

        byte[] buffer = new byte[1024];
        int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);

        string response = Encoding.UTF8.GetString(buffer, 0, bytes);
        Debug.Log("Result from Python: " + response);
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
            string response = Encoding.UTF8.GetString(responseBuffer);
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

    void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
    }
}
