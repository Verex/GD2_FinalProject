using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

public class NetworkHandler : NetworkManager
{
    public List<Player> Players;

    [SerializeField] private GameObject m_PlayerShipPrefab;
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


    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        // Create player game object.
        GameObject playerObject = Instantiate(
            playerPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        Assert.IsNotNull(playerObject);

        // Get sync component.
        PlayerInputSynchronization input = playerObject.GetComponent<PlayerInputSynchronization>();

        // Set up message handler.
        input.InitializeServer();

        // Get player gameobject.
        Player player = playerObject.GetComponent<Player>();

        // Add player reference to player list.
        Players.Add(player);

        // Ready the player for connection.
        NetworkServer.AddPlayerForConnection(conn, playerObject, playerControllerId);

        // Instantiate ship for player.
        GameObject shipObject = Instantiate(
            m_PlayerShipPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        // Get ship controller component.
        ShipController controller = shipObject.GetComponent<ShipController>();

        // Spawn ship in network.
        NetworkServer.Spawn(shipObject);

        // Assign player to ship controller.
        player.Possess(controller);

        // Posess player ship on client.
        controller.TargetSetupShip(conn);

        Debug.Log("Player added. (" + Players.Count + ")");
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.playerControllers.Count > 0)
        {
            // Get player component reference.
            Player playerComponent = conn.playerControllers[0].gameObject.GetComponent<Player>();

            // Remove from list.
            Players.Remove(playerComponent);

            Debug.Log("Player removed. (" + Players.Count + ")");
        }

        base.OnServerConnect(conn);
    }
}