using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Geometry;
using std_msgs = RosSharp.RosBridgeClient.Messages.Standard;
using nav_msgs = RosSharp.RosBridgeClient.Messages.Navigation;
using rosapi = RosSharp.RosBridgeClient.Services.RosApi;
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
        yield return new WaitForSeconds(2f);
        if (!isConnected.WaitOne(0))
            ocuLogger.Loge("Failed to connect to RosBridge: " + RosBridgeServerUrl);
        else
        {
            OcuManager.Instance.InitUnits();
        }
        yield return null;
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
        
        //CallService();
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
        ocuLogger.Logv("Subscribed to " + subscriptionId);
    }
    private void OdomSubscriptionHandler(nav_msgs.Odometry odom)
    {
        position = GetPosition(odom);
        rotation = GetRotation(odom);
        isMessageReceived = true;
    }

    UnityEngine.Vector3 position;
    UnityEngine.Quaternion rotation;
    private bool isMessageReceived;
    private void Update()
    {
        //if (isMessageReceived) ProcessMessage();
    }

    void ProcessMessage()
    {
        ocuLogger.Logv(string.Format("Unit pos: {0}\n" +
            "orientation: {1}", position, rotation));
    }

    private UnityEngine.Vector3 GetPosition(nav_msgs.Odometry message)
    {
        return new UnityEngine.Vector3(
            message.pose.pose.position.x,
            message.pose.pose.position.y,
            message.pose.pose.position.z);
    }
    private UnityEngine.Quaternion GetRotation(nav_msgs.Odometry message)
    {
        return new UnityEngine.Quaternion(
            message.pose.pose.orientation.x,
            message.pose.pose.orientation.y,
            message.pose.pose.orientation.z,
            message.pose.pose.orientation.w);
    }


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