using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllable
{
    public class Unit : MonoBehaviour
    {
        /*Static constant reference vars*/
        public static Color beaconColor = new Color(0.41f, 1.00f, 0.96f);
        public static Color payloadColor = new Color(0.00f, 0.82f, 1.00f);
        public static Color focusedColor = Color.white;
        public static Color focusInvColor = Color.black;

        /*Unit properties*/
        // Real positional data onsite
        protected Vector3 _realPos;
        protected Quaternion _realRot;

        //identification values
        protected string _id;
        private int _num = -1;

        [ReadOnly]
        public Status status;

        /* Getter, setters*/
        public Vector3 realPosition
        {
            get { return _realPos; }
            set
            {
                //TODO: translate real position to transform position
                transform.position = value;
                _realPos = value;
            }
        }
        // may not be needed
        public Quaternion realRotation
        {
            get { return _realRot; }
            set
            {
                //TODO: translate real rotation to transform rotation
                transform.rotation = value;
                _realRot = value;
            }
        }
        public string id {
            get { return _id; }
            set { _id = value; }
        }
        public int num
        {
            get { return _num; }
            set {
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().text = value.ToString();
                _num = value;
            }
        }


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