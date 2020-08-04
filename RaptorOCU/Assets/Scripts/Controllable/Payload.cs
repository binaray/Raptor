using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllable
{
    public class Payload : Unit
    {
        private void Start()
        {
            Init();
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
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = payloadColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusedColor;
            }
        }
    }
}
