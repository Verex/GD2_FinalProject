using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerState
{
    public Vector3 Origin;
};

public struct Frame {

    public Frame(float deltaTime) 
    {
        DeltaTime = deltaTime;
        DeltaPosition = Vector3.zero;
    }

    public Vector3 DeltaPosition;
    public float DeltaTime;
}

public class LagRecord 
{
    public List<Frame> FrameHistory;
    public float HistoryDuration; 
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

    private LagRecord m_LagRecord;

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
        else if(isLocalPlayer)
        {
            FixedUpdateLocalPlayer();
        }
    }

    private void FixedUpdateLocalPlayer()
    {
        UserCmd nextCmd = null;

        while(m_PlayerInput.NextUserCommand(out nextCmd))
        {
            NewState = m_TargetPlayer.ProcessUserCmd(nextCmd, LastPredictedState);

            var frame = new Frame(Time.fixedDeltaTime);

            frame.DeltaPosition = NewState.Origin - LastPredictedState.Origin;

            //m_LagRecord
        }
    }

    private void FixedUpdateServer()
    {
        PlayerState finalState = LastPredictedState;
        UserCmd nextCmd = null;

        while(m_PlayerInput.NextUserCommand(out nextCmd))
        {
            finalState = m_TargetPlayer.ProcessUserCmd(nextCmd, finalState);
        }

        ServerStateUpdate update = new ServerStateUpdate();
        update.netId = m_TargetPlayer.netId.Value;
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