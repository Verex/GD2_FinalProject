using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

using InControl;

public class InputSynchronizationMessage : MessageBase
{
    public static readonly short MessageID = 888;
    public byte[] messageData;
    public int messageLength;

    public static InputSynchronizationMessage FromUserCmd(UserCmd command)
    {
        var messageData = command.Serialize();
        var messageLength = messageData.Length;
        //Fill our message fields to be send
        var newMessage = new InputSynchronizationMessage();
        newMessage.messageData = messageData;
        newMessage.messageLength = messageLength;

        return newMessage;
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

    private PlayerInputBindings m_InputBindings;

    [Server]
    public void InitializeServer()
    {
        //Set server command receive delegate
        NetworkServer.RegisterHandler(
            InputSynchronizationMessage.MessageID,
            ServerReceiveCommand
        );
    }

    public UserCmd CreateUserCmd()
    {
        UserCmd newCommand = new UserCmd(
            m_LastOutgoingSeq++
        );

        return newCommand;
    }

    void Start() 
    {
        if(isClient) {
            m_InputBindings = new PlayerInputBindings(); //Initialize our client-sided input bindings
        }
    }

    public void FixedUpdate()
    {
        if (isServer)
        {
            FixedUpdateServer();
        }
        else if (isClient)
        {
            FixedUpdateClient();
        }
    }

    /*
        The client will record player input from InControl and then pipe it to the server as a UserCmd
    */
    private void FixedUpdateClient()
    {
        UserCmd newCmd = CreateUserCmd();
        //Fill usercmd with user input
        newCmd.Buttons = 0;
        if(m_InputBindings.Accelerate.WasPressed) {
            newCmd.Buttons |= IN_ACCELERATE;
        }
        if(m_InputBindings.Deccelerate.WasPressed) {
            newCmd.Buttons |= IN_DECCELERATE;
        }
        if(m_InputBindings.Left.WasPressed) {
            newCmd.Buttons |= IN_LEFT;
        }
        if(m_InputBindings.Right.WasPressed) {
            newCmd.Buttons |= IN_RIGHT;
        }
        if(m_InputBindings.Fire.WasPressed) {
            newCmd.Buttons |= IN_FIRE;
        }

        PipeUserCommand(newCmd);
    }

    /*
        The server will receive usercmds from the players, and process them on the appropriate player object.
        Movement will be syncrhonized and interpolated through the DynamicNetworkTransform
    */
    private void FixedUpdateServer()
    {

    }

    public void PipeUserCommand(UserCmd cmd)
    {
        var newMessage = InputSynchronizationMessage.FromUserCmd(cmd);
        connectionToServer.SendByChannel(
            InputSynchronizationMessage.MessageID,
            newMessage,
            Channels.DefaultUnreliable
        );
    }

    void ServerReceiveCommand(NetworkMessage message)
    {
        var inputCommandMessage = message.ReadMessage<InputSynchronizationMessage>();
        var inputCommand = UserCmd.DeSerialize(inputCommandMessage.messageData);
        //HACK HACK(Jake): Commands should definitely be stored first before trying to execute them upon receiving this is for TESTING purposes ONLY.
        GetComponent<Player>().ProcessUserCmd(inputCommand); 
        Debug.Log("Received command: " + inputCommand.SequenceNumber);
    }
}