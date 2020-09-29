using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RaptorPlanData
{
    public List<PayloadData> payloadDatas = new List<PayloadData>();
    public List<BeaconData> beaconDatas = new List<BeaconData>(); //Until beacons can be controlled, this can be ignored
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}

[System.Serializable]
public class UnitData
{
    public SerializableTypes.SVector3 position;
    public SerializableTypes.SQuaternion rotation;

    public virtual void Init(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}

[System.Serializable]
public class PayloadData : UnitData
{
}

[System.Serializable]
public class BeaconData : UnitData
{
}