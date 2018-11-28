using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

public class NetworkHandler : NetworkManager
{
    public List<Player> Players;

    [SerializeField] private GameObject m_PlayerShipPrefab;
    private Dictionary<int, Transform> m_ShipSpawns;
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

    public float RoundTripTime
    {
        //HACK-HACK:conversion from int32 to float is no bueno
        get
        {
            return (float)(client.GetRTT() / 1000.0);
        }
    }

    public void RegisterShipSpawn(int playerId, Transform spawnTransform)
    {
        // Add to spawns dictionary.
        m_ShipSpawns.Add(playerId, spawnTransform);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Initialize spawn transform dictionary.
        m_ShipSpawns = new Dictionary<int, Transform>();

        // Listen for input sync commands.
        NetworkServer.RegisterHandler(
            InputSynchronizationMessage.MessageID,
            ServerReceiveCommand
        );
    }

    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);

        // Listen for client update.
        client.RegisterHandler(
            ServerStateUpdate.MessageID,
            ClientReceiveUpdate
        );
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

        // Get player gameobject.
        Player player = playerObject.GetComponent<Player>();

        // Add player reference to player list.
        Players.Add(player);

        // Ready the player for connection.
        NetworkServer.AddPlayerForConnection(conn, playerObject, playerControllerId);

        // Calculate this based on new player in list.
        int spawnedPlayerID = Players.Count - 1;

        // HACK HACK - Only for 2 players...
        if (spawnedPlayerID == 1)
        {
            // Start the game.
            RaceManager.Instance.StartRace();
        }

        // Define default position and rotations for ship.
        Vector3 shipSpawnPosition = Vector3.zero;
        Quaternion shipSpawnRotation = Quaternion.identity;

        // Check if player has spawn transform set.
        if (m_ShipSpawns.ContainsKey(spawnedPlayerID))
        {
            // Get spawn point transform.
            Transform spawn = m_ShipSpawns[spawnedPlayerID];

            // Apply spawn position and rotation.
            shipSpawnPosition = spawn.position;
            shipSpawnRotation = spawn.rotation;
        }

        // Instantiate ship for player.
        GameObject shipObject = Instantiate(
            m_PlayerShipPrefab,
            shipSpawnPosition,
            shipSpawnRotation
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

        base.OnServerDisconnect(conn);
    }

    void ServerReceiveCommand(NetworkMessage message)
    {
        InputSynchronizationMessage inputMessage = message.ReadMessage<InputSynchronizationMessage>();

        // Get network ID from sent object.
        NetworkInstanceId netId = new NetworkInstanceId(inputMessage.netId);

        // Find instance of local object.
        GameObject foundObj = NetworkServer.FindLocalObject(netId);

        // Verify object existance.
        if (foundObj == null) return;

        // Confirm ownership (only works for single player on one machine).
        if (message.conn.playerControllers[0].unetView.netId == netId)
        {
            // Deserialize user command.
            UserCmd inputCommand = UserCmd.DeSerialize(inputMessage.messageData);

            // Get reference to player input.
            PlayerInputSynchronization input = foundObj.GetComponent<PlayerInputSynchronization>();

            // Send user command to object.
            input.HandleUserCommand(inputCommand);
        }
    }

    void ClientReceiveUpdate(NetworkMessage message)
    {
        ServerStateUpdate updateMessage = message.ReadMessage<ServerStateUpdate>();

        NetworkInstanceId netId = new NetworkInstanceId(updateMessage.netId);

        GameObject foundObj = NetworkServer.FindLocalObject(netId);

        if (foundObj == null) return;

        if (message.conn.playerControllers[0].unetView.netId == netId)
        {
            PlayerNetworkTransform networkTransform = foundObj.GetComponent<PlayerNetworkTransform>();
            PlayerState serverState = new PlayerState();
            serverState.Origin = updateMessage.Origin;
            networkTransform.OnServerFrame(serverState);
        }
    }
}