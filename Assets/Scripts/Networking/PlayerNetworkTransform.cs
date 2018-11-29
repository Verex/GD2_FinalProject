using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerState
{
    public PlayerState(Vector3 origin, Vector3 velocity)
    {
        Origin = origin;
        Velocity = velocity;
    }
    public Vector3 Origin;
    public Vector3 Velocity;
};

public class Frame {

    public Frame(float deltaTime) 
    {
        DeltaTime = deltaTime;
        DeltaPosition = Vector3.zero;
    }

    public Vector3 Velocity;

    public Vector3 DeltaPosition;
    public float DeltaTime;

    public UserCmd input;
}

public class LagRecord 
{
    public List<Frame> FrameHistory;
    public float HistoryDuration;

    public LagRecord()
    {
        FrameHistory = new List<Frame>();
    } 
}

public class ServerStateUpdate : MessageBase
{
    public static readonly short MessageID = 889;
    public uint netId;
    
    public Vector3 Origin;
    public Vector3 Velocity;

    public override void Deserialize(NetworkReader reader)
    {
        netId = reader.ReadPackedUInt32();
        Origin = reader.ReadVector3();
        Velocity = reader.ReadVector3();
    }

    public override void Serialize(NetworkWriter writer)
    {
        writer.WritePackedUInt32(netId);
        writer.Write(Origin);
        writer.Write(Velocity);
    }
}

[AddComponentMenu("Network/Player Network Transform")]
public class PlayerNetworkTransform : NetworkBehaviour
{
    public PlayerState LastPredictedState;
    public PlayerState NewState;
    public PlayerState ServerState;

    private Player m_TargetPlayer;
    private PlayerInputSynchronization m_PlayerInput;

    private LagRecord m_LagRecord;

    private bool m_Initialized = false;

    public readonly float CONVERGENCE_MULTIPLIER = 0.05f;

    public void Start()
    {
        m_LagRecord = new LagRecord();
    }

    public void Awake()
    {
        m_TargetPlayer = GetComponent<Player>();
        m_PlayerInput = GetComponent<PlayerInputSynchronization>();
        //Initialize predicted state


    }

    public void Initialize(Vector3 position)
    {
        LastPredictedState = new PlayerState(position, Vector3.zero);
        //Intialize new state
        NewState = new PlayerState(position, Vector3.zero);
        //Initialize server state
        ServerState = new PlayerState(position, Vector3.zero);

        m_Initialized = true;
    }

    void Update()
    {

    }
    void FixedUpdate()
    {
        if(!m_Initialized)
        {
            return;
        }
        if(isServer)
        {
            FixedUpdateServer();
        }
        if(isLocalPlayer)
        {
            UpdateClient();
        }
    }

    private void UpdateClient()
    {
        UserCmd nextCmd = null;

        while(m_PlayerInput.NextUserCommand(out nextCmd))
        {
            //Temporary state for client prediction
            NewState = m_TargetPlayer.ProcessUserCmd(nextCmd, LastPredictedState, Time.fixedDeltaTime);

            //Client frame for this duration
            var frame = new Frame(Time.fixedDeltaTime);
            frame.DeltaPosition = NewState.Origin - LastPredictedState.Origin; //Displacement
            frame.Velocity = NewState.Velocity;
            frame.input = nextCmd;

            m_LagRecord.FrameHistory.Add(frame);
            m_LagRecord.HistoryDuration += Time.fixedDeltaTime; //Duration of frame

            LastPredictedState = NewState;
        }
        if(isServer)
        {
            m_TargetPlayer.MoveShip(LastPredictedState.Origin);
        }
        else
        {
            float latency = NetworkHandler.Instance.RoundTripTime;
            var extrapolatedPosition = LastPredictedState.Origin + NewState.Velocity *  latency * CONVERGENCE_MULTIPLIER;
            var interpolationFraction = Time.fixedDeltaTime / (latency * (1 + CONVERGENCE_MULTIPLIER));
            var clientStatePosition = m_TargetPlayer.CurrentShipPosition;
            clientStatePosition += (extrapolatedPosition - clientStatePosition) * interpolationFraction;
            m_TargetPlayer.MoveShip(clientStatePosition);
        }

        //Extrapolate position here
        m_TargetPlayer.MoveShip(LastPredictedState.Origin);
    }

    private void FixedUpdateServer()
    {
        PlayerState finalState = ServerState;
        UserCmd nextCmd = null;

        while(m_PlayerInput.NextUserCommand(out nextCmd))
        {
            finalState = m_TargetPlayer.ProcessUserCmd(nextCmd, finalState, Time.fixedDeltaTime);
        }

        ServerStateUpdate update = new ServerStateUpdate();
        update.netId = m_TargetPlayer.netId.Value;
        update.Origin = finalState.Origin;

        m_TargetPlayer.MoveShip(finalState.Origin);

        connectionToClient.SendByChannel(
            ServerStateUpdate.MessageID,
            update,
            Channels.DefaultReliable
        );

        ServerState = finalState;

    }

    public void OnServerFrame(PlayerState serverUpdate)
    {
        if(isServer)
        {
            LastPredictedState = serverUpdate;
            return;
        }
        float latency = NetworkHandler.Instance.RoundTripTime;
        float simulationTime = Mathf.Max(0, m_LagRecord.HistoryDuration - latency);
        m_LagRecord.HistoryDuration = m_LagRecord.HistoryDuration - simulationTime;
    
        while(m_LagRecord.FrameHistory.Count > 0 && simulationTime > 0)
        {
            if(simulationTime >= m_LagRecord.FrameHistory[0].DeltaTime)
            {
                simulationTime -= m_LagRecord.FrameHistory[0].DeltaTime;
                m_LagRecord.FrameHistory.RemoveAt(0);
            }
            else
            {
                int x = 0;
                //Move back linear fraction
                var fraction = 1 - simulationTime / m_LagRecord.FrameHistory[0].DeltaTime;
                m_LagRecord.FrameHistory[0].DeltaTime = m_LagRecord.FrameHistory[0].DeltaTime - simulationTime;
                m_LagRecord.FrameHistory[0].DeltaPosition = m_LagRecord.FrameHistory[0].DeltaPosition * fraction;
                break;
            }
        }
        ServerState = serverUpdate;

        //If we're teleporting, go ahead and replay inputs from server position
        if((ServerState.Velocity - m_LagRecord.FrameHistory[0].Velocity).magnitude > 0.3f)
        {
            Debug.Log("REPLAYING INPUTS FROM SERVER STATE");
            LastPredictedState = ServerState;
            foreach(var frame in m_LagRecord.FrameHistory)
            {
                NewState = m_TargetPlayer.ProcessUserCmd(frame.input, LastPredictedState, frame.DeltaTime);
                frame.DeltaPosition = NewState.Origin - LastPredictedState.Origin;
                frame.Velocity = NewState.Velocity;
                LastPredictedState = NewState;
            }
        }
        else
        {
            LastPredictedState.Origin = serverUpdate.Origin;
            //Add any unaccounted deltas
            foreach(var frame in m_LagRecord.FrameHistory)
            {
                LastPredictedState.Origin += frame.DeltaPosition;
            }
        }

    }
}