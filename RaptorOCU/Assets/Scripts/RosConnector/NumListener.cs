using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Geometry;
using std_msgs = RosSharp.RosBridgeClient.Messages.Standard;
using nav_msgs = RosSharp.RosBridgeClient.Messages.Navigation;
using rosapi = RosSharp.RosBridgeClient.Services.RosApi;
using System.Diagnostics;
using RosSharp.RosBridgeClient.Services;

public class NumListener : MonoBehaviour
{

    public string uri = "ws://192.168.137.185:9090";
    private RosSocket rosSocket;
    string subscriptionId = "";

    void Start()
    {
        rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol(uri));
        //Subscribe("/chatter");
        OdomSubscribe("/position");
        //CallService();
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
        StartCoroutine(WaitForKey());
    }

    private IEnumerator WaitForKey()
    {
        UnityEngine.Debug.Log("Press any key to close...");

        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        UnityEngine.Debug.Log("Closed");
        rosSocket.Close();
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