using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mirror;
using Mirror.Discovery;
using ScriptableObjectArchitecture;
using UnityEngine;
using Debug = DebugFile;

public class CustomNetworkManager : NetworkManager
{
    public bool offlineMode; //TODO remove?;
    
    public delegate void OnConnectionEstablished(GameObject playerGameObject);
    public static OnConnectionEstablished ConnectionEstablished = delegate {};
    
    [Header("Discovery")]
    [SerializeField] private NetworkDiscovery _networkDiscovery;
    
    private void Start()   
    {
        if (offlineMode) Instantiate(playerPrefab); //TODO needed?

        if (PlayerPrefs.GetInt("host", 0) == 1)
        {
            Debug.Log("starting host, advertising server");
            StartHost();
            _networkDiscovery.AdvertiseServer();
        }
        else
        {
            Debug.Log("start looking for server");
            _networkDiscovery.StartDiscovery();
        }
    }

    public void OnServerFound(ServerResponse response)
    {
        Debug.Log("OnServerFound");
        networkAddress = response.EndPoint.Address.ToString();
        _networkDiscovery.StopDiscovery();
        //StartClient(response.uri);
        StartCoroutine(TryConnect());
    }
    
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Debug.Log("OnServerAddPlayer, spawning player and adding player for connection");
        // add player at correct spawn position
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log("connected to client " + networkAddress);
        ConnectionEstablished(player);
    }

    public void EnableNetworkGUI(bool show)
    {
        //GetComponent<NetworkManagerHUD>().showGUI = show;
    }         
    
    public override void OnServerDisconnect(NetworkConnectionToClient conn) //TODO handle disconnection
    {
        // call base functionality (actually destroys the player)
        base.OnServerDisconnect(conn);
    }

    private IEnumerator TryConnect()
    {
        while (!NetworkClient.isConnected)
        {
            Debug.Log("trying to connect to host.");
            StartClient();
            yield return new WaitForSeconds(4);
        }
        Debug.Log("connected to host " + networkAddress);
    }
    
}
