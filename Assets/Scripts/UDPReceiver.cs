using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient udpClient;
    public int port = 8000;
    public bool startReceiving = true;
    public bool printToConsole = false;
    public string data;

    void Start()
    {
        InitUDP();
    }

    void InitUDP()
    {
        try
        {
            udpClient = new UdpClient(port); // bind ke port
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (SocketException e)
        {
            Debug.LogError($"[UDPReceiver] Socket error: {e.Message}. Port mungkin sudah digunakan.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UDPReceiver] Error saat inisialisasi: {ex.Message}");
        }
    }

    private void ReceiveData()
    {
        try
        {
            while (startReceiving)
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataBytes = udpClient.Receive(ref anyIP);
                data = Encoding.UTF8.GetString(dataBytes);

                if (printToConsole)
                    Debug.Log($"[UDPReceiver] Data received: {data}");
            }
        }
        catch (Exception err)
        {
            Debug.LogWarning($"[UDPReceiver] Exception dalam ReceiveData: {err.Message}");
        }
    }

    void OnApplicationQuit()
    {
        CloseUDP();
    }

    void OnDestroy()
    {
        CloseUDP();
    }

    void CloseUDP()
    {
        try
        {
            startReceiving = false;

            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }

            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Abort(); // hati-hati: bisa crash kalau thread sedang blocking, tapi di Unity masih sering dipakai
                receiveThread = null;
            }

            Debug.Log("[UDPReceiver] Socket dan thread ditutup.");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[UDPReceiver] Gagal menutup: {e.Message}");
        }
    }
}
