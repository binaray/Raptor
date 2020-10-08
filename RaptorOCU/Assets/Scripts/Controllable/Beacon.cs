using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient.Messages;
using UnityEngine.UI;

namespace Controllable
{
    public class Beacon : Unit
    {
        public GameObject beaconDisplay;

        public override void Init(string id, int num, Vector3 realPos, Quaternion realRot)
        {
            beaconDisplay.transform.GetChild(0).GetComponent<Text>().text = id;
            base.Init(id, num, realPos, realRot) ;
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
                transform.GetChild(0).GetComponent<SpriteRenderer>().color = (status == Status.Alive) ? beaconColor : deadColor;
                transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().color = focusedColor;
            }
        }

        //private string cameraSubscriptionId;
        //protected void CaneraUpdate()
        //{
        //    transform.position = realPosition;
        //    transform.rotation = realRotation;
        //}
        //public void CameraSubscribe(string id)
        //{
        //    OcuLogger.Instance.Logv("Subscribing to: " + id);
        //    cameraSubscriptionId = RaptorConnector.Instance.rosSocket.Subscribe<nav_msgs.Odometry>(id, CameraSubscriptionHandler);
        //}
        //protected virtual void CameraSubscriptionHandler(nav_msgs.Odometry odom)
        //{
        //    realPosition = new Vector3(odom.pose.pose.position.x, odom.pose.pose.position.y);
        //    realRotation = new Quaternion(odom.pose.pose.orientation.x, odom.pose.pose.orientation.y, odom.pose.pose.orientation.z, odom.pose.pose.orientation.w);
        //    isMessageReceived = true;
        //}
    }
}
