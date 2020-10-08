using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllable
{
    public class Payload : Unit
    {
        public GameObject payloadDisplay;
        private TMPro.TextMeshPro posText;

        private void Awake()
        {
            posText = transform.GetChild(2).GetChild(0).GetComponent<TMPro.TextMeshPro>();
        }

        private void Update()
        {
            //print(((Vector2)realPosition).ToString() + " " + realRotation.ToString());
            if (isMessageReceived)
            {
                OdomUpdate();
                //OcuLogger.Instance.Logv("odom updated"+Time.realtimeSinceStartup);
                //TODO: timeout and set flag to false if no message received for x seconds
                posText.text = ((Vector2)realPosition).ToString();
            }
        }

        public override void Init(string id, int raptorNum, Vector3 realPos, Quaternion realRot)
        {
            base.Init(id, raptorNum, realPos, realRot);
            payloadDisplay.GetComponent<PayloadDisplayItem>().SetText(id);
            if (RaptorConnector.Instance.buildMode == RaptorConnector.BuildMode.Prodution)
                StartCoroutine(ConnectionTimeout());
        }

        public override void SetSelectedColors(bool isSelected)
        {
            Gradient gradient = new Gradient();
            // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            GradientColorKey[] colorKey = new GradientColorKey[2];
            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];

            if (isSelected)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = focusedColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusInvColor;
                payloadDisplay.GetComponent<PayloadDisplayItem>().SetSelectionDisplay(true);
                posText.color = focusedColor;
                colorKey[0].color = focusedColor;
                colorKey[0].time = 0.0f;
                colorKey[1].color = focusedColor;
                colorKey[1].time = 1.0f;
                alphaKey[0].alpha = 1.0f;
                alphaKey[0].time = 0.0f;
                alphaKey[1].alpha = 1.0f;
                alphaKey[1].time = 1.0f;
            }
            else
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = (status == Status.Alive) ? payloadColor : deadColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusedColor;
                payloadDisplay.GetComponent<PayloadDisplayItem>().SetSelectionDisplay(false);
                posText.color = (status == Status.Alive) ? payloadColor : deadColor;
                colorKey[0].color = (status == Status.Alive) ? payloadColor : deadColor;
                colorKey[0].time = 0.0f;
                colorKey[1].color = (status == Status.Alive) ? payloadColor : deadColor;
                colorKey[1].time = 1.0f;
                alphaKey[0].alpha = 1.0f;
                alphaKey[0].time = 0.0f;
                alphaKey[1].alpha = 1.0f;
                alphaKey[1].time = 1.0f;
            }
            gradient.SetKeys(colorKey, alphaKey);
            transform.GetChild(2).GetComponent<LineRenderer>().colorGradient = gradient;
        }

        protected override void SetDisplayAttachedGuiStatus(Status newStatus)
        {
            payloadDisplay.GetComponent<PayloadDisplayItem>().SetLifeDisplay(newStatus);
        }

        private void OnDestroy()
        {
            Destroy(payloadDisplay);
        }
    }
}
