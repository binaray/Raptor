using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldScaler : Singleton<WorldScaler>
{ 
    public static float worldScale = 0.36f;//2;   //Current scale 2 units in Unity to 2m irl

    public static Vector3 WorldToRealPosition(Vector3 worldPos)
    {
        return worldPos / worldScale;
    }
    public static float WorldToRealPosition(float worldPos)
    {
        return worldPos / worldScale;
    }

    public static Vector3 RealToWorldPosition(Vector3 realPos)
    {
        return realPos * worldScale;
    }
    public static float RealToWorldPosition(float realPos)
    {
        return realPos * worldScale;
    }
}
