using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllable
{
    public class Unit : MonoBehaviour
    {
        //TODO: differentiate between ros metrics, unity scene, and lat/long
        public Vector3 position;
        public Quaternion rotation;
        public string id;
        public Status status;

        /*Static constant vars*/
        public static Color beaconColor = new Color(0.41f, 1.00f, 0.96f);
        public static Color payloadColor = new Color(0.00f, 0.82f, 1.00f);
        public static Color focusedColor = Color.white;
        public static Color focusInvColor = Color.black;

        private Transform spriteTransform;

        public virtual void Init()
        {
            spriteTransform = transform.GetChild(0);
            SetSelectedColors(false);
        }

        public virtual void SetSelectedColors(bool isSelected)
        {
            if (isSelected)
            {
                spriteTransform.GetComponent<SpriteRenderer>().color = focusedColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusInvColor;
            }
            else
            {
                spriteTransform.GetComponent<SpriteRenderer>().color = focusInvColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusedColor;
            }
        }
        
        public void Rotate(Vector3 rotationVector)
        {
            spriteTransform.Rotate(rotationVector);
        }

        public void MoveForward(float forwardDelta)
        {
            Vector3 directionVector = spriteTransform.rotation * Vector3.up;
            transform.position += directionVector * forwardDelta;
        }

        public void MoveAndRotateTowards(Vector2 target, float forwardDelta, float rotationDelta)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, forwardDelta);
            //spriteTransform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.FromToRotation(transform.position, target), rotationDelta);
        }
    }

    public enum Status
    {
        Alive,
        Dead
    }
}