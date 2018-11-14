using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;


public class NetworkHandler : NetworkManager
{
    //Singleton Design Pattern =) 
    private static NetworkHandler s_Instance;
    public static NetworkHandler Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = GameObject.FindObjectOfType(
                    typeof(NetworkHandler)
                ) as NetworkHandler; //Initialize our network handler
            }
            return s_Instance;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
        GameObject playerObj = Instantiate(
            playerPrefab,
            Vector3.zero,
            Quaternion.identity
        );
        Assert.IsNotNull(playerObj);
        var input = playerObj.GetComponent<PlayerInputSynchronization>() as PlayerInputSynchronization;

        input.InitializeServer(); //Setup message handler

        NetworkServer.AddPlayerForConnection(conn, playerObj, playerControllerId);
    }
}