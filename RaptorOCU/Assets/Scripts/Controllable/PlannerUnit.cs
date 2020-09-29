using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllable
{
    public class PlannerUnit : Unit
    {
        public Unit parentUnit;
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void MoveParentToPlan()
        {
            //TODO: action here
            if (RaptorConnector.Instance.buildMode == RaptorConnector.BuildMode.Prodution)
            {
                parentUnit.SetMoveGoal(realPosition, realRotation);
            }
            else
            {
                parentUnit.realPosition = realPosition;
                parentUnit.transform.position = realPosition;
                parentUnit.SetRotation(realRotation);
            }
        }

        public override void Init(string id, int raptorNum, Vector3 realPos, Quaternion realRot)
        {
            realPos.z = -1;
            base.Init(id, raptorNum, realPos, realRot);
        }

        public void LoadPayloadData(PayloadData p)
        {
            realPosition = p.position;
            realRotation = p.rotation;
            transform.position = p.position;
            spriteTransform.rotation = p.rotation;
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
                //posText.color = focusedColor;
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
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = plannerColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusedColor;
                //posText.color = payloadColor;
                colorKey[0].color = plannerColor;
                colorKey[0].time = 0.0f;
                colorKey[1].color = plannerColor;
                colorKey[1].time = 1.0f;
                alphaKey[0].alpha = 1.0f;
                alphaKey[0].time = 0.0f;
                alphaKey[1].alpha = 1.0f;
                alphaKey[1].time = 1.0f;
            }
            gradient.SetKeys(colorKey, alphaKey);
            transform.GetChild(2).GetComponent<LineRenderer>().colorGradient = gradient;
        }
    }
}