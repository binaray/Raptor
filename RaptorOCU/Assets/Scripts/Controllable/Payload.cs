using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient.Messages.Sensor;
using geometry_msgs = RosSharp.RosBridgeClient.Messages.Geometry; 


namespace Controllable
{
    public class Payload : Unit
    {
        public GameObject payloadDisplay;
        private TMPro.TextMeshPro posText;
        private string gpsSubId, geomTwistId;
        //public Vector2 latLong;
        private LatitudeLongitude latLong = new LatitudeLongitude();
        private bool isNatSatReceived = false;
        private bool isGeomTwistReceived = false;

        [SerializeField]
        private Googlemap googlemap;

        

        private void Awake()
        {
            posText = transform.GetChild(2).GetChild(0).GetComponent<TMPro.TextMeshPro>();
            posText.text = "UNKNOWN";
        }

        private void Update()
        {
            //print(((Vector2)realPosition).ToString() + " " + realRotation.ToString());
            if (isMessageReceived )
            {
                if (!Compass.Instance.isCalibrating) Compass.Instance.ImuAngleOffsetSubscribe(num);
                OdomUpdate();
                posText.text = string.Format("{0}, {1}", realPosition.x.ToString("0.00"), realPosition.y.ToString("0.00")); //((Vector2)realPosition).ToString();
            }
            if (isNatSatReceived) {
                //googlemap.updateLatLong(latLong.lat, latLong.lon);
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

        public override string GetActionStatus()
        {
            return GetComponent<MoveBaseActionClient>().GetActionStatus();
        }

        #region ROS Subscriptions
        //-GPS data-
        //message type details: http://docs.ros.org/en/api/sensor_msgs/html/msg/NavSatFix.html
        public void GpsSubscribe(int i)
        {
            string gpsId = string.Format("raptor{0}/sensors/filtered", i);
            OcuLogger.Instance.Logv("Subscribing to GPS: " + gpsId);
            NavSatFix natSatData = new NavSatFix();
            gpsSubId = RaptorConnector.Instance.rosSocket.Subscribe<NavSatFix>(gpsId, NatSatSubscriptionHandler);
        }

        protected virtual void NatSatSubscriptionHandler(NavSatFix natSat)
        {
            latLong.lat = natSat.latitude;
            latLong.lon = natSat.longitude;
            isNatSatReceived = true;
            //timeElapsed = 0f;
        }

        /*
        public void GeomTwistSubscribe(int i)
        {
            string geomid = string.Format("raptor{0}/sensors/filtered", i);
            OcuLogger.Instance.Logv("Subscribing to Geometry Twist: " + geomid);
            geomTwistId = RaptorConnector.Instance.rosSocket.Subscribe<>(geomid, GeomTwistSubscriptionHandler);
        }

        protected virtual void GeomTwistSubscriptionHandler( geomTwist)
        {
            isGeomTwistReceived = true;
        }
        */
        
        #endregion

        private void OnDestroy()
        {
            Destroy(payloadDisplay);
        }


        private class LatitudeLongitude
        {
            public double lat { get; set; }
            public double lon { get; set; }
        };
    }
}
