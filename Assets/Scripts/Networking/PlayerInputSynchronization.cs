using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

using InControl;

public class InputSynchronizationMessage : MessageBase
{
    public static readonly short MessageID = 888;
    public uint netId;
    public byte[] messageData;

    public static InputSynchronizationMessage FromUserCmd(UserCmd command)
    {
        var messageData = command.Serialize();
        var messageLength = messageData.Length;

        //Fill our message fields to be send
        var newMessage = new InputSynchronizationMessage();
        newMessage.messageData = messageData;

        return newMessage;
    }

    public override void Deserialize(NetworkReader reader)
    {
        netId = reader.ReadPackedUInt32();
        messageData = reader.ReadBytesAndSize();
    }

    public override void Serialize(NetworkWriter writer)
    {
        writer.WritePackedUInt32(netId);
        writer.WriteBytesFull(messageData);
    }
}

public class PlayerInputSynchronization : NetworkBehaviour
{

    /*
        To save bandwith, all of our actions are serialized into a single 8-bit field
        Bit 0: Left
        Bit 1: Right
        Bit 2: Accel
        Bit 3: Deccel
        Bit 4: Fire
        ....?
     */
    public static readonly byte IN_LEFT = 1 << 0;
    public static readonly byte IN_RIGHT = 1 << 1;
    public static readonly byte IN_ACCELERATE = 1 << 2;
    public static readonly byte IN_DECCELERATE = 1 << 3;
    public static readonly byte IN_FIRE = 1 << 4;


    private int m_LastOutgoingSeq = 0;
    private int m_LastIncomingSeq = 0;
    private UserCmd m_UserCmd;
    private UserCmd m_LastUserCmd;
    private Queue<UserCmd> m_StoredCmds;

    private PlayerInputBindings m_InputBindings;
    private Player m_TargetPlayer;

    public UserCmd CreateUserCmd()
    {
        UserCmd newCommand = new UserCmd(
            m_LastOutgoingSeq + 1
        );

        return newCommand;
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            m_InputBindings = new PlayerInputBindings(); //Initialize our client-sided input bindings
            m_InputBindings.InitializeBindings();
            m_UserCmd = CreateUserCmd();
            m_LastUserCmd = CreateUserCmd();
        }

        m_StoredCmds = new Queue<UserCmd>();
        m_TargetPlayer = GetComponent<Player>();
    }

    public void Update()
    {
        if (isLocalPlayer)
        {
            LocalPlayerUpdate();
        }
    }

    public void LocalPlayerUpdate()
    {
        // Clear current input command.
        m_UserCmd.Buttons = 0;

        if (m_InputBindings.Accelerate.WasPressed || m_InputBindings.Accelerate.WasRepeated)
        {
            m_UserCmd.Buttons |= IN_ACCELERATE;
        }
        if (m_InputBindings.Deccelerate.WasPressed)
        {
            m_UserCmd.Buttons |= IN_DECCELERATE;
        }
        if (m_InputBindings.Left.IsPressed)
        {
            m_UserCmd.Buttons |= IN_LEFT;
        }
        if (m_InputBindings.Right.IsPressed)
        {
            m_UserCmd.Buttons |= IN_RIGHT;
        }
        if (m_InputBindings.Fire.WasPressed)
        {
            m_UserCmd.Buttons |= IN_FIRE;
        }
    }

    public void FixedUpdate()
    {
        if (isServer)
        {
            FixedUpdateServer();
        }
        if (isLocalPlayer)
        {
            LocalPlayerFixedUpdate();
        }
    }

    /*
        The client will record player input from InControl and then pipe it to the server as a UserCmd
    */
    private void LocalPlayerFixedUpdate()
    {
        if (m_UserCmd.Buttons != m_LastUserCmd.Buttons)
        {
            // Handle local user cmd.
            m_TargetPlayer.ProcessUserCmd(m_UserCmd);

            PipeUserCommand(m_UserCmd);

            // Update user buttons.
            m_LastUserCmd.Buttons = m_UserCmd.Buttons;
        }
    }

    /*
        The server will receive usercmds from the players, and process them on the appropriate player object.
        Movement will be syncrhonized and interpolated through the DynamicNetworkTransform
    */
    private void FixedUpdateServer()
    {
        while (m_StoredCmds.Count != 0)
        {
            var commandToCompute = m_StoredCmds.Dequeue();
            m_TargetPlayer.ProcessUserCmd(commandToCompute);
        }
    }

    public void PipeUserCommand(UserCmd cmd)
    {
        if (cmd.SequenceNumber - m_LastOutgoingSeq > 1)
        {
            //We are missing some commands, lets look at our choked command history.
            return;
        }

        // Create input message.
        InputSynchronizationMessage msg = new InputSynchronizationMessage();

        // Assign message values.
        msg.netId = netId.Value;
        msg.messageData = cmd.Serialize();

        // Send message to server.
        connectionToServer.SendByChannel(
            InputSynchronizationMessage.MessageID,
            msg,
            Channels.DefaultUnreliable
        );

        m_LastOutgoingSeq++;
    }

    public void HandleUserCommand(UserCmd cmd)
    {
        if (cmd.SequenceNumber - m_LastIncomingSeq > 1)
        {
            //We are missing some commands lets start predicting
            //Run prediction code
        }
        else
        {
            m_LastIncomingSeq = cmd.SequenceNumber;
            m_StoredCmds.Enqueue(cmd);
        }
    }
}