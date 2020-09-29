using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nav_msgs = RosSharp.RosBridgeClient.Messages.Navigation;

namespace Controllable
{
    public class Unit : MonoBehaviour
    {
        /*Static constant reference vars*/
        public static Color beaconColor = new Color(0.41f, 1.00f, 0.96f);
        public static Color payloadColor = new Color(0.00f, 0.82f, 1.00f);
        public static Color plannerColor = new Color(0.00f, 1.00f, 0.00f);
        public static Color focusedColor = Color.white;
        public static Color focusInvColor = Color.black;

        /*Unit properties*/
        // Real positional data onsite
        protected Vector3 _realPos;
        protected Quaternion _realRot;

        //identification values
        protected string _id;
        private int _num = -1;

        //ROS connection params
        string subscriptionId = "";

        //[ReadOnlyAttribute]
        public Status status;

        /* Getter, setters*/
        public Vector3 realPosition;
        public Quaternion realRotation;
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

        public Transform spriteTransform;
        protected bool isMessageReceived = false;

        public virtual void Init(string id, int num, Vector3 realPos, Quaternion realRot)
        {
            spriteTransform = transform.GetChild(0);
            SetSelectedColors(false);
            this.id = id;
            this.num = num;
            realPosition = realPos;
            realRotation = realRot;
            transform.position = realPos;
            spriteTransform.rotation = realRot;
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

        /*-- SUBSCRIPTION HANDLERS TO IMPLEMENT --*/
        protected void OdomUpdate()
        {
            transform.position = realPosition;
            transform.rotation = realRotation;
        }
        public void OdomSubscribe(string id)
        {
            OcuLogger.Instance.Logv("Subscribing to: " + id);
            subscriptionId = RaptorConnector.Instance.rosSocket.Subscribe<nav_msgs.Odometry>(id, OdomSubscriptionHandler);
        }
        protected virtual void OdomSubscriptionHandler(nav_msgs.Odometry odom)
        {
            realPosition = new Vector3(odom.pose.pose.position.x, odom.pose.pose.position.y);
            realRotation = new Quaternion(odom.pose.pose.orientation.x, odom.pose.pose.orientation.y, odom.pose.pose.orientation.z, odom.pose.pose.orientation.w);
            isMessageReceived = true;
        }

        /*-- Raptor movement- Requires ROS connection --*/
        public void SetupMoveBaseAction(int num)
        {
            //TODO: HANDLE BEACON ID
            GetComponent<MoveBaseActionClient>().SetupAction(num);
        }
        public void SetMoveGoal(Vector3 position, Quaternion rotation)
        {
            GetComponent<MoveBaseActionClient>().SetTargetPoseAndSendGoal(position, rotation);
        }
        public void CancelMoveBaseAction()
        {
            GetComponent<MoveBaseActionClient>().CancelGoal();
        }

        /* Local movement methods- GUI updates only */
        public void Rotate(Vector3 rotationVector)
        {
            spriteTransform.Rotate(rotationVector);
            realRotation = spriteTransform.rotation;
        }
        public void SetRotation(Quaternion rotation)
        {
            spriteTransform.rotation = rotation;
            realRotation = spriteTransform.rotation;
        }
        public void SetRotation(Vector3 eulerAngles)
        {
            spriteTransform.eulerAngles = eulerAngles;
            realRotation = spriteTransform.rotation;
        }
        public void MoveForward(float forwardDelta)
        {
            Vector3 directionVector = spriteTransform.rotation * Vector3.up;
            realPosition += directionVector * forwardDelta;
            transform.position = realPosition;
        }
        public void MoveAndRotateTowards(Vector2 target, Quaternion targetRotation, float forwardDelta, float rotationDelta)
        {
            Vector2 nextPos = Vector2.MoveTowards(transform.position, target, forwardDelta);
            transform.position = nextPos;
            spriteTransform.rotation = Quaternion.RotateTowards(targetRotation, Quaternion.FromToRotation(transform.position, target), rotationDelta);
            realPosition = nextPos;
            realRotation = spriteTransform.rotation;
        }
    }

    public enum Status
    {
        Alive,
        Dead
    }
}