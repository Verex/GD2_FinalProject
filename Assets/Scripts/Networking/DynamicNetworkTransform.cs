using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


[AddComponentMenu("Network/Non-Shitty Network Transform")]
public class DynamicNetworkTransform : NetworkBehaviour
{

    Vector2 m_TargetSyncPosition;
    Vector2 m_FixedDeltaPos;

    Vector3 m_PreviousPosition;


    NetworkWriter m_LocalNetworkWriter;

    float m_LastClientSyncTime; //Last client received packet
    float m_LastClientSendTime; //Last outgoing packet


    // Update is called once per frame
    void Update()
    {
        //Interpolate between our last two positional updates
        if (isClient)
        {
            var lerpFraction = (Time.time - m_LastClientSyncTime) / GetNetworkSendInterval();
            transform.position = Vector3.Lerp(
                m_PreviousPosition,
                m_TargetSyncPosition,
                lerpFraction
            );
        }
        if (isServer)
        {
            transform.position = new Vector3(
                Mathf.Sin(Time.time * 2f) * 3f,
                Mathf.Cos(Time.time * 2f) * 3f,
                0f
            );
        }
        if (!hasAuthority)
            return;
        if (!localPlayerAuthority)
            return;
        if (NetworkServer.active)
            return;
        if (Time.time - m_LastClientSendTime > GetNetworkSendInterval())
        {
            SendTransform();
            m_LastClientSendTime = Time.time;
        }
    }

    void Awake()
    {
        m_PreviousPosition = transform.position;

        if (localPlayerAuthority)
        {
            m_LocalNetworkWriter = new NetworkWriter();
        }
    }


    bool HasMoved()
    {
        if ((transform.position - m_PreviousPosition).magnitude > 0.01)
        {
            return true;
        }
        return false;
    }

    [Client]
    void SendTransform()
    {
        if (!HasMoved() || ClientScene.readyConnection == null)
            return;

        m_LocalNetworkWriter.StartMessage(MsgType.LocalPlayerTransform);
        m_LocalNetworkWriter.Write(netId);

        m_LocalNetworkWriter.Write(transform.position);
        m_PreviousPosition = transform.position;

        m_LocalNetworkWriter.FinishMessage();

        ClientScene.readyConnection.SendWriter(m_LocalNetworkWriter, GetNetworkChannel());

    }

    public override void OnStartServer()
    {
        m_LastClientSyncTime = 0f;
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

        if (hasAuthority)
        {
            reader.ReadVector3();
            return;
        }

        if (isServer)
        {
            var pos = reader.ReadVector3();
            transform.position = pos;
        }
        else
        {
            m_PreviousPosition = transform.position;
            m_TargetSyncPosition = reader.ReadVector3();
        }

        m_LastClientSyncTime = Time.time;
    }

    void FixedUpdateServer()
    {
        if (syncVarDirtyBits != 0)
            return;
        if (!NetworkServer.active)
            return;
        if (!isServer)
            return;
        if (GetNetworkSendInterval() == 0)
            return;
        float distance = (transform.position - m_PreviousPosition).magnitude;
        if (distance < 0.01)
        {
            return;
        }

        SetDirtyBit(1);
    }

    void FixedUpdateClient()
    {
        if (m_LastClientSyncTime == 0)
            return; //No incoming data
        if (!NetworkServer.active && !NetworkClient.active)
            return;
        if (!isServer && !isClient)
            return;
        if (GetNetworkSendInterval() == 0)
        {
            return;
        }
        if (hasAuthority)
            return;
    }

    void FixedUpdate()
    {
        if (isServer)
        {
            FixedUpdateServer();
        }
        if (isClient)
        {
            FixedUpdateClient();
        }
    }


    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        if (initialState)
        {

        }
        else if (syncVarDirtyBits == 0)
        {
            writer.WritePackedUInt32(0);
            return false;
        }
        else
        {
            writer.WritePackedUInt32(1);
        }

        writer.Write(transform.position);
        m_PreviousPosition = transform.position;

        return true;
    }

    public override int GetNetworkChannel()
    {
        return Channels.DefaultUnreliable;
    }

    public override void OnStartAuthority()
    {
        m_LastClientSyncTime = 0;
    }
}
