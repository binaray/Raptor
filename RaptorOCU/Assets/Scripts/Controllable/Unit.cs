using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllable
{
    public class Unit : MonoBehaviour
    {
        public Vector3 position;
        public Quaternion rotation;
        public string id;
        public Status status;

        public static Color beaconColor = new Color(0.41f, 1.00f, 0.96f);
        public static Color payloadColor = new Color(0.00f, 0.82f, 1.00f);
        public static Color focusedColor = Color.white;
        public static Color focusInvColor = Color.black;

        public virtual void SetSelectedColors(bool isSelected)
        {
            if (isSelected)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = focusedColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusInvColor;
            }
            else
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = focusInvColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusedColor;
            }
        }
        
    }

    public enum Status
    {
        Alive,
        Dead
    }
}