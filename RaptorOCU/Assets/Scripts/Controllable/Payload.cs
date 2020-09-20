using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllable
{
    public class Payload : Unit
    {
        public GameObject payloadDisplay;

        public override void Init(string id, int num, Vector3 realPos)
        {
            base.Init(id, num, realPos);
            payloadDisplay.GetComponent<PayloadDisplayItem>().SetText(id);
        }

        public override void SetSelectedColors(bool isSelected)
        {
            if (isSelected)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = focusedColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusInvColor;
                payloadDisplay.GetComponent<PayloadDisplayItem>().SetSelectionDisplay(true);
            }
            else
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = payloadColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusedColor;
                payloadDisplay.GetComponent<PayloadDisplayItem>().SetSelectionDisplay(false);
            }
        }

        private void OnDestroy()
        {
            Destroy(payloadDisplay);
        }
    }
}
