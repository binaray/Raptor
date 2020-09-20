using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Geometry;
using std_msgs = RosSharp.RosBridgeClient.Messages.Standard;
using nav_msgs = RosSharp.RosBridgeClient.Messages.Navigation;
using rosapi = RosSharp.RosBridgeClient.Services.RosApi;
//using System.Diagnostics;
using RosSharp.RosBridgeClient.Services;
using RosSharp.RosBridgeClient.Messages;

public class RaptorConnector : Singleton<RaptorConnector>
{
    public int Timeout = 10;
    public RosSocket.SerializerEnum Serializer;
    public RosSocket rosSocket;
    public string RosBridgeServerUrl = "ws://192.168.137.185:9090";

    private ManualResetEvent isConnected = new ManualResetEvent(false);
    private OcuLogger ocuLogger;

    string subscriptionId = "";

    void Start()
    {
        ocuLogger = OcuLogger.Instance;
        RosConnectionRoutine();
    }

    /*-- Ros socket initializers and handlers --*/
    public void RosConnectionRoutine() { StartCoroutine(ConnectAndWait()); }
    IEnumerator ConnectAndWait()
    {
        ocuLogger.Logw("--Attempting to connect to ROS--");
        rosSocket = ConnectToRos(RosBridgeServerUrl, OnConnected, OnClosed, Serializer);
        yield return new WaitForSeconds(10f);
        if (!isConnected.WaitOne(0))
            ocuLogger.Loge("Failed to connect to RosBridge: " + RosBridgeServerUrl);
    }

    public static RosSocket ConnectToRos(string serverUrl, System.EventHandler onConnected = null, System.EventHandler onClosed = null, RosSocket.SerializerEnum serializer = RosSocket.SerializerEnum.JSON)
    {
        RosSharp.RosBridgeClient.Protocols.IProtocol protocol = new RosSharp.RosBridgeClient.Protocols.WebSocketSharpProtocol(serverUrl);
        protocol.OnConnected += onConnected;
        protocol.OnClosed += onClosed;
        return new RosSocket(protocol, serializer);
    }
    private void OnConnected(object sender, System.EventArgs e)
    {
        isConnected.Set();
        ocuLogger.Logw("Connected to RosBridge: " + RosBridgeServerUrl);
        //Setup subscriptions and actions here

        //rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol(uri));
        //Subscribe("/chatter");
        //OdomSubscribe("/position");
        CallService();
    }
    private void OnClosed(object sender, System.EventArgs e)
    {
        isConnected.Reset();
        ocuLogger.Logw("Disconnected from RosBridge: " + RosBridgeServerUrl);
    }


    private void OnApplicationQuit()
    {
        rosSocket.Close();
    }

    /*-- Subscription handlers --*/
    public void OdomSubscribe(string id)
    {
        subscriptionId = rosSocket.Subscribe<nav_msgs.Odometry>(id, OdomSubscriptionHandler);
    }
    private void OdomSubscriptionHandler(nav_msgs.Odometry odom)
    {
        //UnityEngine.Debug.Log(string.Format("Unit pos: ({0},{1})\n" +
        //    "orientation: ({2},{3})", odom.pose.pose.position.x, odom.pose.pose.position.y,
        //    odom.pose.pose.orientation.x, odom.pose.pose.orientation.y, odom.pose.pose.orientation.z));
    }

    //public void Subscribe(string id)
    //{
    //    subscriptionId = rosSocket.Subscribe<std_msgs.String>(id, SubscriptionHandler);
    //}
    //private void SubscriptionHandler(std_msgs.String message)
    //{
    //    ocuLogger.Logv("Message received!");
    //    ocuLogger.Logv(message.data);
    //}

    /*-- Service client handlers --*/
    public void CallService()
    {
        ocuLogger.Logv("Calling Service");
        if (!isConnected.WaitOne(0))
        {
            ocuLogger.Loge("Not connected to ROS");
            return;
        }
        nav_msgs.Odometry pos = new nav_msgs.Odometry();
        pos.pose.pose.position.x = 0.05f;
        pos.pose.pose.position.y = 1;
        ocuLogger.Logv(string.Format("Called move to pos: ({0},{1})", pos.pose.pose.position.x, pos.pose.pose.position.y));
        MoveToPosRequest request = new MoveToPosRequest(pos);
        rosSocket.CallService<MoveToPosRequest, MoveToPosResponse>("/move_to_pos", ServiceCallHandler, request);
        //rosSocket.CallService<AddTwoIntsRequest, AddTwoIntsResponse>("/add_two_ints", ServiceCallHandler, request);
        //rosSocket.CallService<rosapi.GetParamRequest, rosapi.GetParamResponse>("/rosapi/get_param", ServiceCallHandler, new rosapi.GetParamRequest("/rosdistro", "default"));
    }
    private void ServiceCallHandler(MoveToPosResponse message)
    {
        //UnityEngine.Debug.Log("ROS distro: " + message.value);
        //UnityEngine.Debug.Log("Sum: "+message.sum);
        ocuLogger.Logv("Status: " + message.status);
    }
}