using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllable
{
    public class Beacon : Unit
    {
        public override void Init(string id, int num, Vector3 realPos)
        {
            base.Init(id, num, realPos);
        }

        public override void SetSelectedColors(bool isSelected)
        {
            if (isSelected)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = focusedColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusInvColor;
            }
            else
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = beaconColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusedColor;
            }
        }
    }
}
