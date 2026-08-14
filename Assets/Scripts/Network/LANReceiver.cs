using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LANReceiver : MonoBehaviour
{
    public int port = 47777;

    private UdpClient udp;
    private bool running;

    private void Start()
    {
        if (PlayerPrefs.GetInt("repeater") == 0)
        {
            udp = new UdpClient(port);
            running = true;

            udp.BeginReceive(OnDataReceived, null);

            Debug.Log($"LAN Receiver listening on UDP port {port}");    
        }
    }
    
    private void OnDestroy()
    {
        running = false;

        if (udp != null)
        {
            udp.Close();
            udp = null;
        }
    }
    
    private void OnDataReceived(System.IAsyncResult result)
    {
        if (!running) return;

        try
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] data = udp.EndReceive(result, ref remoteEndPoint);
            string message = Encoding.UTF8.GetString(data);

            Debug.Log($"Received '{message}' from {remoteEndPoint.Address}:{remoteEndPoint.Port}");

            // Continue listening for the next packet.
            udp.BeginReceive(OnDataReceived, null);
        }
        catch (System.ObjectDisposedException)
        {
            // Socket was closed during shutdown.
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UDP receive error: {e}");
        }
    }


}