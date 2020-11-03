using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient.Messages;
using RosSharp.RosBridgeClient.Messages.Sensor;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Controllable
{
    public class Beacon : Unit
    {
        public GameObject beaconDisplay;
        private string gpsSubId;
        public Vector2 latLong;
        private bool isNatSatReceived = false;

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

        //-GPS data-
        //message type details: http://docs.ros.org/en/api/sensor_msgs/html/msg/NavSatFix.html
        public void GpsSubscribe(string id)
        {
            OcuLogger.Instance.Logv("Subscribing to GPS: " + id);
            NavSatFix natSatData = new NavSatFix();
            gpsSubId = RaptorConnector.Instance.rosSocket.Subscribe<NavSatFix>(id, NatSatSubscriptionHandler);
        }
        protected virtual void NatSatSubscriptionHandler(NavSatFix natSat)
        {
            latLong.x = (float)natSat.latitude;
            latLong.y = (float)natSat.longitude;
            isNatSatReceived = true;
            //timeElapsed = 0f;
        }

        #region ROS Cam /*-TO IMPLEMENT:  ROS Camera Subscription-*/
        CustomWebRequest camImage;
        string camUrl;
        UnityWebRequest webRequest;
        byte[] bytes = new byte[90000]; //Reserved space for dl handler

        public void SetupCamera(string url)
        {
            print("Connecting to: " + url);
            camUrl = url;
            
            StartCoroutine(GetCamTexture());
        }

        IEnumerator GetCamTexture()
        {
            webRequest = new UnityWebRequest(camUrl);
            camImage = new CustomWebRequest(bytes);
            camImage.target = beaconDisplay.GetComponent<RawImage>();
            webRequest.downloadHandler = camImage;
            yield return webRequest.SendWebRequest();

            //while (true)
            //{
            //    if (camImage.newDataReceived)
            //    {
            //        Texture2D camTexture = new Texture2D(2, 2);
            //        camTexture.LoadImage(camImage.completeImageByte);
            //        beaconDisplay.GetComponent<RawImage>().texture = camTexture;
            //        Debug.Log("Applied new Image");
            //    }
            //    else
            //        Debug.Log("Waiting for data..");
            //    yield return new WaitForSeconds(0.2f);
            //}
        }
        #endregion

        #region Unity runtime
        private void Update()
        {
            if (isNatSatReceived)
            {
                print("GPS data: " + latLong.x + ", " + latLong.y);
            }
        }
        private void OnApplicationQuit()
        {
            webRequest.Abort();
        }
        #endregion
    }
}
