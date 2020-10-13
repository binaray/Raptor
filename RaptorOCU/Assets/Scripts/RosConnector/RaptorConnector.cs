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

/*-- Built with reference to RosConnector script; Changes: unit initialization and using coroutines --*/
public class RaptorConnector : Singleton<RaptorConnector>
{
    public int Timeout = 10;
    public RosSocket.SerializerEnum Serializer;
    public RosSocket rosSocket;
    public string RosBridgeServerUrl = "ws://192.168.137.185:9090";
    public enum BuildMode { UiTest, Prodution }
    public BuildMode buildMode;

    private ManualResetEvent isConnected = new ManualResetEvent(false);
    private OcuLogger ocuLogger;

    void Start()
    {
        ocuLogger = OcuLogger.Instance;
        string ip = PlayerPrefs.GetString(PlayerPrefsConstants.ROS_BRIDGE_IP, null);
        if (ip != null)
        {
            SetRosIp(ip);
        }
        if (buildMode==BuildMode.UiTest)
            OcuManager.Instance.InitUnits(true);
        else if (buildMode==BuildMode.Prodution)
            RosConnectionRoutine();
    }

    /*-- Ros socket initializers and handlers --*/
    public void SetRosIp(string ip)
    {
        PlayerPrefs.SetString(PlayerPrefsConstants.ROS_BRIDGE_IP, ip);
        RosBridgeServerUrl = "ws://" + ip + ":9090";
    }
    public void RosConnectionRoutine() { StartCoroutine(ConnectAndWait()); }
    IEnumerator ConnectAndWait()
    {
        ocuLogger.Logw("--Attempting to connect to ROS--");
        //RosBridgeServerUrl = PlayerPrefs.GetString(PlayerPrefsConstants.ROS_BRIDGE_URL, null);
        try
        {
            rosSocket = ConnectToRos(RosBridgeServerUrl, OnConnected, OnClosed, Serializer);
        }
        catch(System.Exception e)
        {
            ocuLogger.Loge(e.Message);
            UiManager.Instance.ReconnectDialog();
        }
        yield return new WaitForSeconds(2f);
        if (!isConnected.WaitOne(0))
        {
            ocuLogger.Loge("Failed to connect to RosBridge: " + RosBridgeServerUrl);
            UiManager.Instance.ReconnectDialog();
        }
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


    #region Test functions
    /*-- Service client handlers (working test) --*/
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
    #endregion
}