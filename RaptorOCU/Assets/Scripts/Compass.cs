using RosSharp.RosBridgeClient.Messages.Sensor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using std_msgs = RosSharp.RosBridgeClient.Messages.Standard;

public class Compass : Singleton<Compass>
{
    public bool isCalibrating;
    public bool isCalibrated;
    public float offset;
    public Text GpsPosition;
    private string imuId;
    private bool isMessageReceived = false;

    #region IMU compass offset
    public void ImuAngleOffsetSubscribe(int raptorNum)
    {
        isCalibrating = true;
        GpsSubscribe("bp_gps/fix");
        string topicId = string.Format("raptor{0}/ini_ori_est_angle", raptorNum);
        OcuLogger.Instance.Logv("Subscribing to IMU: " + topicId);
        imuId = RaptorConnector.Instance.rosSocket.Subscribe<std_msgs.Float64>(topicId, ImuSubscriptionHandler);
    }
    protected virtual void ImuSubscriptionHandler(std_msgs.Float64 offset)
    {
        Compass.Instance.offset = (float)offset.data;
        isMessageReceived = true;
    }
    #endregion

    #region GPS
    //-GPS data-
    //message type details: http://docs.ros.org/en/api/sensor_msgs/html/msg/NavSatFix.html
    private string gpsSubId;
    public double lati;
    public double longi;
    private bool isNatSatReceived = false;
    public void GpsSubscribe(string id)
    {
        OcuLogger.Instance.Logv("Subscribing to GPS: " + id);
        NavSatFix natSatData = new NavSatFix();
        gpsSubId = RaptorConnector.Instance.rosSocket.Subscribe<NavSatFix>(id, NatSatSubscriptionHandler);
    }
    protected virtual void NatSatSubscriptionHandler(NavSatFix natSat)
    {
        lati = natSat.latitude;
        longi = natSat.longitude;
        isNatSatReceived = true;
    }
    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        isCalibrating = false;
        isCalibrated = false;
        offset = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMessageReceived && !isCalibrated)
        {
            GetComponent<RectTransform>().Rotate(new Vector3(0, 0, -offset));
            isCalibrated = true;
        }
        if (isNatSatReceived) GpsPosition.text = string.Format("lat: {0}, long: {1}", lati.ToString("0.0000"), longi.ToString("0.0000"));
    }
}
