using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient.Messages;
using RosSharp.RosBridgeClient.Messages.Sensor;
using UnityEngine.UI;
using UnityEngine.Networking;
using sensor_msgs = RosSharp.RosBridgeClient.Messages.Sensor;

namespace Controllable
{
    public class Beacon : Unit
    {
        public GameObject beaconDisplay;
        private string gpsSubId;
        private string imageSubId;
        public Vector2 latLong;
        public byte[] imageData;
        private bool isNatSatReceived = false;
        private bool isImgReceived = false;
        public Texture2D camTex;


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

        #region ros subscriptions
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

        //-Video Data-
        //havent decided which is used yet 
        //compressed image message type details: http://docs.ros.org/en/api/sensor_msgs/html/msg/CompressedImage.html 
        //image message type details: https://docs.ros.org/en/melodic/api/sensor_msgs/html/msg/Image.html
        public void ImageSubscribe(int i)
        {
            string imgId = string.Format("b{0}/compressedImage", i);
            OcuLogger.Instance.Logv("Subscribing to Image: " + imgId);
            imageSubId = RaptorConnector.Instance.rosSocket.Subscribe<sensor_msgs.Image>(imgId, ImageSubscriptionHandler);
        }

        protected virtual void ImageSubscriptionHandler(sensor_msgs.Image image)
        {
            camTex = new Texture2D(2, 2);
            imageData = image.data;
            isImgReceived = true;
            camTex.LoadImage(imageData);
            beaconDisplay.GetComponent<RawImage>().texture = camTex;
        }
        #endregion

        #region UITestImage
        public void ImageUITestSetup() {
            camTex = new Texture2D(2, 2);
            StartCoroutine(ImageUITestUpdator());
        }
        IEnumerator ImageUITestUpdator() {
            //Get the path of the Game data folder
            string full_path_start = Application.dataPath;
            string imageFilePath = "\\Resources\\Sprites\\UITestBeaconImg.png";
            //Output the Game data path to the console
            Debug.Log("dataPath : " + full_path_start);
            
            string url = full_path_start+imageFilePath; 

            WWW www = new WWW(url);
            
            Debug.Log("bytes downloaded: "+ www.bytesDownloaded);
            yield return www;
            //LoadImageIntoTexture compresses JPGs by DXT1 and PNGs by DXT5     
            www.LoadImageIntoTexture(camTex);
            beaconDisplay.GetComponent<RawImage>().texture = camTex;
        }

        #endregion


        #region IP Camera
        //NOT USING 
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
            //webRequest.Abort();
        }
        #endregion
    }
}
