using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine.Networking;


[System.Serializable]
public class UserCmd
{

    private readonly int m_SequenceNumber;

    public byte Buttons;

    public int SequenceNumber
    {
        get
        {
            return m_SequenceNumber;
        }
    }

    public UserCmd() { }

    public UserCmd(int sequenceNumber)
    {
        m_SequenceNumber = sequenceNumber;
    }

    public bool ActionPressed(ushort field)
    {
        var result = (Buttons & field);
        return result != 0;
    }

    public byte[] Serialize()
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, this);
            return ms.ToArray();
        }
        return null;
    }

    public static UserCmd DeSerialize(byte[] data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(data))
        {
            var deserailzedCmd = bf.Deserialize(
                ms
            ) as UserCmd;
            return deserailzedCmd;
        }
        return null;
    }
}