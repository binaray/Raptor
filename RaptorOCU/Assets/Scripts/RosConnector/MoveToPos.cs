/*
This message class is generated automatically with 'ServiceMessageGenerator' of ROS#
*/ 

using Newtonsoft.Json;
using RosSharp.RosBridgeClient.Messages.Geometry;
using RosSharp.RosBridgeClient.Messages.Navigation;
using RosSharp.RosBridgeClient.Messages.Sensor;
using RosSharp.RosBridgeClient.Messages.Standard;
using RosSharp.RosBridgeClient.Messages.Actionlib;

namespace RosSharp.RosBridgeClient.Services
{
public class MoveToPosRequest : Message
{
[JsonIgnore]
public const string RosMessageName = "beginner_tutorials/MoveToPos";

public Odometry pos;

public MoveToPosRequest(Odometry _odom){pos = _odom;
}
}

public class MoveToPosResponse : Message
{
[JsonIgnore]
public const string RosMessageName = "beginner_tutorials/MoveToPos";

public int status;
}
}

