using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

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

public class PlayerInputSynchronization : NetworkBehaviour {
    private int m_LastOutgoingSeq = 0;
    private int m_LastIncomingSeq = 0;

    private NetworkClient m_Client;

    public void Initialize(NetworkClient client) {
        m_Client = client;

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

    public void PipeUserCommand(UserCmd cmd) {
        var newMessage = InputSynchronizationMessage.FromUserCmd(cmd);
    }

    void ServerReceiveCommand(NetworkMessage message) {

    }
}