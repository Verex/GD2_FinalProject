using System;

public class UserCmd
{

    private readonly int m_SequenceNumber;

    public int SequenceNumber
    {
        get
        {
            return m_SequenceNumber;
        }
    }

    public UserCmd(int sequenceNumber)
    {
        m_SequenceNumber = sequenceNumber;
    }
}