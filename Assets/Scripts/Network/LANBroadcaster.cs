using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class LANBroadcaster : MonoBehaviour
{
    public int port = 47777;

    private UdpClient udp;

    private void Start()
    {
        if (PlayerPrefs.GetInt("repeater") == 1)
        {
            udp = new UdpClient();
            udp.EnableBroadcast = true;

            InvokeRepeating(nameof(Broadcast), 0f, 1f);    
        }
    }
    
    private void OnDestroy()
    {
        udp?.Close();
    }
    
    private void Broadcast()
    {
        byte[] data = Encoding.UTF8.GetBytes("HELLO");

        // 255.255.255.255 = broadcast to the local network
        udp.Send(data, data.Length,
            new IPEndPoint(IPAddress.Broadcast, port));
    }
   
}