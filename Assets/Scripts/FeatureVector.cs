<<<<<<< HEAD
﻿using SQLite4Unity3d;
using System;
using System.Reflection;
using System.Text;

public class FeatureVector
{
    public double PalmToThumbDistance { get; set; }
    public double PalmToIndexDistance { get; set; }
    public double PalmToMiddleDistance { get; set; }
    public double PalmToRingDistance { get; set; }
    public double PalmToPinkyDistance { get; set; }
    public double PinkyToRingDistance { get; set; }
    public double RingToMiddleDistance { get; set; }
    public double MiddleToIndexDistance { get; set; }
    public double IndexToThumbDistance { get; set; }
    public double ThumbToHandNormalDistance { get; set; }
    public double IndexToHandNormalDistance { get; set; }
    public double MiddleToHandNormalDistance { get; set; }
    public double RingToHandNormalDistance { get; set; }
    public double PinkyToHandNormalDistance { get; set; }
    public double RadiusSphere { get; set; }
    public double PinchStrength { get; set; }
    public double GrabStrength { get; set; }
    public int NumExtendedFingers { get; set; }
    public string Gesture { get; set; }
    public int GestureClassLabel { get; set; }
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public double [] createInputVector()
    {
        return new double []
        {
            PalmToThumbDistance, PalmToIndexDistance, PalmToMiddleDistance, PalmToRingDistance, PalmToPinkyDistance,
            IndexToThumbDistance, RingToMiddleDistance, MiddleToIndexDistance, PinkyToRingDistance,
            ThumbToHandNormalDistance, IndexToHandNormalDistance, MiddleToHandNormalDistance, RingToHandNormalDistance, PinkyToHandNormalDistance,
            RadiusSphere, NumExtendedFingers, PinchStrength, GrabStrength
        };
    }

    public override string ToString()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        PropertyInfo[] properties = GetType().GetProperties(flags);

        StringBuilder sb = new StringBuilder();

        foreach (var property in properties)
        {
            object value = property.GetValue(this, null);
            sb.AppendFormat("{0}: {1}{2}", property.Name, value != null ? value : null, Environment.NewLine);
        }

        return sb.ToString();
    }
=======
﻿using SQLite4Unity3d;
using System;
using System.Reflection;
using System.Text;

public class FeatureVector
{
    public double PalmToThumbDistance { get; set; }
    public double PalmToIndexDistance { get; set; }
    public double PalmToMiddleDistance { get; set; }
    public double PalmToRingDistance { get; set; }
    public double PalmToPinkyDistance { get; set; }
    public double PinkyToRingDistance { get; set; }
    public double RingToMiddleDistance { get; set; }
    public double MiddleToIndexDistance { get; set; }
    public double IndexToThumbDistance { get; set; }
    public double ThumbFingerBending { get; set; }
    public double IndexFingerBending { get; set; }
    public double MiddleFingerBending { get; set; }
    public double RingFingerBending { get; set; }
    public double PinkyFingerBending { get; set; }
    public string Gesture { get; set; }
    public int GestureClassLabel { get; set; }
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    public double [] createInputVector()
    {
        return new double []
        {
            PalmToThumbDistance, PalmToIndexDistance, PalmToMiddleDistance, PalmToRingDistance, PalmToPinkyDistance,
            IndexToThumbDistance, RingToMiddleDistance, MiddleToIndexDistance, PinkyToRingDistance, ThumbFingerBending,
            IndexFingerBending, MiddleFingerBending, RingFingerBending, PinkyFingerBending
        };
    }

    public override string ToString()
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        PropertyInfo[] properties = GetType().GetProperties(flags);

        StringBuilder sb = new StringBuilder();

        foreach (var property in properties)
        {
            object value = property.GetValue(this, null);
            sb.AppendFormat("{0}: {1}{2}", property.Name, value != null ? value : null, Environment.NewLine);
        }

        return sb.ToString();
    }
>>>>>>> 605f8f612bc6fcba0b15f3361ad64eaf7511d55e
}