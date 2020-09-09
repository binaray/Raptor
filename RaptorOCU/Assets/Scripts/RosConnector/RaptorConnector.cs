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

public class RaptorConnector : MonoBehaviour
{
    public int Timeout = 10;
    public RosSocket.SerializerEnum Serializer;
    private RosSocket rosSocket;
    public string RosBridgeServerUrl = "ws://192.168.137.185:9090";

    private ManualResetEvent isConnected = new ManualResetEvent(false);

    string subscriptionId = "";

    public void Awake()
    {
        new Thread(ConnectAndWait).Start();
    }
    private void ConnectAndWait()
    {
        rosSocket = ConnectToRos(RosBridgeServerUrl, OnConnected, OnClosed, Serializer);

        if (!isConnected.WaitOne(Timeout * 1000))
            Debug.LogWarning("Failed to connect to RosBridge at: " + RosBridgeServerUrl);
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
        Debug.Log("Connected to RosBridge: " + RosBridgeServerUrl);
    }
    private void OnClosed(object sender, System.EventArgs e)
    {
        isConnected.Reset();
        Debug.Log("Disconnected from RosBridge: " + RosBridgeServerUrl);
    }

    void Start()
    {
        //rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol(uri));
        //Subscribe("/chatter");
        OdomSubscribe("/position");
        //CallService();
    }

    private void OnApplicationQuit()
    {
        rosSocket.Close();
    }

    public void OdomSubscribe(string id)
    {
        subscriptionId = rosSocket.Subscribe<nav_msgs.Odometry>(id, OdomSubscriptionHandler);
    }

    private void OdomSubscriptionHandler(nav_msgs.Odometry odom)
    {
        UnityEngine.Debug.Log("Message received!");
        UnityEngine.Debug.Log("x: "+odom.pose.pose.position.x);
        UnityEngine.Debug.Log("y: " + odom.pose.pose.position.y);
    }

    public void Subscribe(string id)
    {
        subscriptionId = rosSocket.Subscribe<std_msgs.String>(id, SubscriptionHandler);
    }

    private void SubscriptionHandler(std_msgs.String message)
    {
        UnityEngine.Debug.Log("Message received!");
        UnityEngine.Debug.Log(message.data);
    }

    public void CallService()
    {
        UnityEngine.Debug.Log("Calling Service");
        nav_msgs.Odometry pos = new nav_msgs.Odometry();
        pos.pose.pose.position.x = 0.05f;
        pos.pose.pose.position.y = 1;
        pos.pose.pose.position.y = 10;
        UnityEngine.Debug.Log(pos.pose.pose.position.x);
        MoveToPosRequest request = new MoveToPosRequest(pos);
        rosSocket.CallService<MoveToPosRequest, MoveToPosResponse>("/move_to_pos", ServiceCallHandler, request);
        //rosSocket.CallService<AddTwoIntsRequest, AddTwoIntsResponse>("/add_two_ints", ServiceCallHandler, request);
        //rosSocket.CallService<rosapi.GetParamRequest, rosapi.GetParamResponse>("/rosapi/get_param", ServiceCallHandler, new rosapi.GetParamRequest("/rosdistro", "default"));
    }

    private void ServiceCallHandler(MoveToPosResponse message)
    {
        //UnityEngine.Debug.Log("ROS distro: " + message.value);
        //UnityEngine.Debug.Log("Sum: "+message.sum);
        UnityEngine.Debug.Log("Status: " + message.status);
    }
}