using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public struct PlayerState
{
    public Vector3 Origin;
};

public struct Frame {

}

public class ServerStateUpdate : MessageBase
{
    public static readonly short MessageID = 889;
    public uint netId;
    
    public Vector3 Origin;

    public override void Deserialize(NetworkReader reader)
    {
        netId = reader.ReadPackedUInt32();
        Origin = reader.ReadVector3();
    }

    public override void Serialize(NetworkWriter writer)
    {
        writer.WritePackedUInt32(netId);
        writer.Write(Origin);
    }
}

[AddComponentMenu("Network/Player Network Transform")]
public class PlayerNetworkTransform : NetworkBehaviour
{
    public PlayerState LastPredictedState;
    public PlayerState NewState;

    private Player m_TargetPlayer;
    private PlayerInputSynchronization m_PlayerInput;

    public void Awake()
    {
        m_TargetPlayer = GetComponent<Player>();
        m_PlayerInput = GetComponent<PlayerInputSynchronization>();
    }

    void FixedUpdate()
    {
        if(isServer)
        {
            FixedUpdateServer();
        }
    }

    private void FixedUpdateServer()
    {

        PlayerState finalState = LastPredictedState;
        while(m_PlayerInput.StoredCommands.Count) 
        {
            //Process received commands on the server
            var command = m_PlayerInput.StoredCommands.Dequeue();
            finalState = m_TargetPlayer.ProcessUserCmd(command, finalState);
        }

        ServerStateUpdate update = new ServerStateUpdate();
        update.netId = m_TargetPlayer.netIdl
        update.Origin = finalState.Origin;

        connectionToClient.SendByChannel(
            ServerStateUpdate.MessageID,
            update,
            Channels.DefaultReliable
        );
    }

    public void OnServerFrame(PlayerState severState)
    {

    }
}