using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;

class Player : NetworkBehaviour
{

    //This is the target ship controller, it can be set in the editor or overrided at runtime. 
    [SerializeField] private ShipController m_TargetController;

    private int m_LastOutgoingSeq = 0;
    private int m_LastIncomingSeq = 0;

    private UserCmd m_OutgoingCmd;

    private UserCmd CreateUserCmd()
    {
        UserCmd newCommand = new UserCmd(
            m_LastOutgoingSeq++
        ); //TODO(Jake): Ensure object creation, check packet choking

        return newCommand;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        if (isServer && NetworkServer.localClientActive)
            return;
        if (!initialState)
        {
            if (reader.ReadPackedUInt32() == 0)
                return;
        }
        if (isServer)
        {
            //Check incoming user command sequence
            var incomingCmdSize = reader.ReadInt32();
            var incomingCmdData = reader.ReadBytes(incomingCmdSize);
            var incomingCmd = UserCmd.DeSerialize(
                incomingCmdData
            );
            var incomingSequence = incomingCmd.SequenceNumber;
        }
        else
        {

        }
    }

    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        if(initialState) {

        }
        else if(syncVarDirtyBits == 0) {
            writer.WritePackedUInt32(0);
            return false;
        } else {
            writer.WritePackedUInt32(1);
        }
        
        var serializedCmd = m_OutgoingCmd.Serialize();
        writer.Write(serializedCmd.Length);
        writer.Write(
            serializedCmd,
            serializedCmd.Length
        );

        return true;
    }

    public void Possess(ShipController sc)
    {
        Assert.IsNotNull(sc, "Cannot possess a null ship controller, stupid");
        m_TargetController = sc; //Possess target ship controller

        //TODO(Any): Maybe create an OnShipPossessed event or something
    }

    public void Update()
    {
        //Accumulate mouse samples, keyboard samples, etc.

    }
    public void FixedUpdate()
    {
        //Process input, serialize and send to server to be handled by NetworkTransform
        var newCmd = CreateUserCmd();
        m_OutgoingCmd = newCmd;
        SetDirtyBit(1);
        //Construct usercmd
    }
}