using System;
using UnityEngine;

[Serializable]
public class SerializableDateTime : IComparable<SerializableDateTime>
{
    [SerializeField]
    private long m_ticks;

    private bool initialized;
    private DateTime m_dateTime;
    public DateTime DateTime
    {
        get
        {
            if (!initialized)
            {
                m_dateTime = new DateTime(m_ticks);
                initialized = true;
            }

            return m_dateTime;
        }
    }

    public SerializableDateTime(DateTime dateTime)
    {
        m_ticks = dateTime.Ticks;
        m_dateTime = dateTime;
        initialized = true;
    }

    public int CompareTo(SerializableDateTime other)
    {
        if (other == null)
        {
            return 1;
        }
        return m_ticks.CompareTo(other.m_ticks);
    }
}
