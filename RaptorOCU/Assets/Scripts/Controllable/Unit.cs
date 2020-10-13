using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using nav_msgs = RosSharp.RosBridgeClient.Messages.Navigation;

namespace Controllable
{
    public class Unit : MonoBehaviour
    {
        #region Static constant variables
        public static Color beaconColor = new Color(0.41f, 1.00f, 0.96f);
        public static Color payloadColor = new Color(0.00f, 0.82f, 1.00f);
        public static Color plannerColor = new Color(0.00f, 1.00f, 0.00f);
        public static Color deadColor = new Color(1.00f, 0.00f, 0.00f);
        public static Color focusedColor = Color.white;
        public static Color focusInvColor = Color.black;
        #endregion

        #region Unit runtime variables
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

        public Vector3 realPosition;
        public Quaternion realRotation;
        
        //Designated unit id
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }

        //Designated unit number
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
        #endregion

        #region Unit initialization and display methods
        public virtual void Init(string id, int num, Vector3 realPos, Quaternion realRot)
        {
            spriteTransform = transform.GetChild(0);
            SetSelectedColors(false);
            this.id = id;
            this.num = num;
            SetRealPosition(realPos);
            SetRotation(realRot);
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

        //base method to be overriden by payload and beacon
        protected virtual void SetDisplayAttachedGuiStatus(Status newStatus)
        {
        }
        #endregion

        #region ROS Odometry subscription methods
        protected void OdomUpdate()
        {
            transform.position = WorldScaler.RealToWorldPosition(realPosition);
            spriteTransform.rotation = realRotation;
            //spriteTransform.eulerAngles -= new Vector3(0, 0, 90f);
        }
        public void OdomSubscribe(int i)
        {
            string odomId = string.Format("raptor{0}/odometry/filtered", i);
            OcuLogger.Instance.Logv("Subscribing to: " + odomId);
            subscriptionId = RaptorConnector.Instance.rosSocket.Subscribe<nav_msgs.Odometry>(odomId, OdomSubscriptionHandler);
        }
        protected virtual void OdomSubscriptionHandler(nav_msgs.Odometry odom)
        {
            // For some reason gui updates are not updated here so flag to main loop is used instead
            // TODO: test updates using getter/setter
            //realPosition = new Vector3(odom.pose.pose.position.x, odom.pose.pose.position.y);
            realPosition = new Vector3(odom.pose.pose.position.x, odom.pose.pose.position.y);
            realRotation = new Quaternion(odom.pose.pose.orientation.x, odom.pose.pose.orientation.y, odom.pose.pose.orientation.z, odom.pose.pose.orientation.w);
            //realRotation = new Quaternion(odom.pose.pose.orientation.y, -odom.pose.pose.orientation.z, -odom.pose.pose.orientation.x, odom.pose.pose.orientation.w);
            isMessageReceived = true;
            timeElapsed = 0f;
        }
        #endregion

        #region ROS Joystick publisher methods /*Not yet implemented!*/
        /*-- Joystick (Manual movement) publisher --*/
        private JoyAxisReader[] JoyAxisReaders;
        private JoyButtonReader[] JoyButtonReaders;
        public string FrameId = "Unity";
        private string JoyPublicationId;
        private RosSharp.RosBridgeClient.Messages.Sensor.Joy message;
        public void JoyPublisherSetup(int num)
        {
            message = new RosSharp.RosBridgeClient.Messages.Sensor.Joy();
            message.header.frame_id = FrameId;
            message.axes = new float[2];
            message.buttons = new int[0];
            JoyPublicationId = RaptorConnector.Instance.rosSocket.Advertise<RosSharp.RosBridgeClient.Messages.Sensor.Joy>("/goal");
        }
        public void PublishJoy()
        {
            message.header.Update();
            message.axes[0] = Input.GetAxis("Horizontal");
            message.axes[1] = Input.GetAxis("Vertical");
            RaptorConnector.Instance.rosSocket.Publish(JoyPublicationId, message);
        }
        #endregion

        #region ROS move base action methods
        public void SetupMoveBaseAction(int num)
        {
            //TODO: HANDLE BEACON ID
            GetComponent<MoveBaseActionClient>().SetupAction(num);
        }
        public void SetMoveGoal(Vector3 targetRealPos, Quaternion rotation)
        {
            GetComponent<MoveBaseActionClient>().SetTargetPoseAndSendGoal(targetRealPos, rotation);
        }
        public void CancelMoveBaseAction()
        {
            GetComponent<MoveBaseActionClient>().CancelGoal();
        }
        public virtual string GetActionStatus()
        {
            return "-";
        }
        #endregion

        #region Check connection timeout coroutine
        float timeout = 5f;
        float timeElapsed = 0f;
        protected IEnumerator ConnectionTimeout()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                timeElapsed += 1f;
                if (timeElapsed >= timeout && status != Status.Dead)
                {
                    status = Status.Dead;
                    SetDisplayAttachedGuiStatus(status);
                    OcuLogger.Instance.Loge("Lost connection to: " + id);
                    SetSelectedColors(false);
                    isMessageReceived = false;
                    if (this is Payload)
                    {
                        OcuManager.Instance.operationalPayloadIds.Remove(num);
                    }
                }
                else if (isMessageReceived == true)
                {
                    status = Status.Alive;
                    SetDisplayAttachedGuiStatus(status);
                    SetSelectedColors(false);
                    if (this is Payload)
                    {
                        OcuManager.Instance.operationalPayloadIds.Add(num);
                    }
                }
            }
        }
        #endregion

        #region Local movement methods
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
            Vector3 directionVector = spriteTransform.rotation * Vector3.right;
            SetWorldPosition(transform.position + directionVector * forwardDelta);
        }
        public void MoveAndRotateTowards(Vector2 target, Quaternion targetRotation, float forwardDelta, float rotationDelta)
        {
            SetWorldPosition(Vector2.MoveTowards(transform.position, target, forwardDelta));
            SetRotation(Quaternion.RotateTowards(targetRotation, Quaternion.FromToRotation(transform.position, target), rotationDelta));
        }
        #endregion

        #region Set position methods
        public void SetRealPosition(Vector3 realPos)
        {
            transform.position = WorldScaler.RealToWorldPosition(realPos);
            realPosition = realPos;
        }
        public void SetWorldPosition(Vector3 worldPos)
        {
            transform.position = worldPos;
            realPosition = WorldScaler.WorldToRealPosition(worldPos);
        }
        #endregion
    }


    //Unit status
    public enum Status
    {
        Alive,
        Dead
    }
}