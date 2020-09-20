/*
This message class is generated automatically with 'SimpleMessageGenerator' of ROS#
*/ 

using Newtonsoft.Json;
using RosSharp.RosBridgeClient.Messages.Geometry;
using RosSharp.RosBridgeClient.Messages.Navigation;
using RosSharp.RosBridgeClient.Messages.Sensor;
using RosSharp.RosBridgeClient.Messages.Standard;
using RosSharp.RosBridgeClient.Messages.Actionlib;

namespace RosSharp.RosBridgeClient.Messages
{
public class MoveBaseActionResult : Message
{
[JsonIgnore]
public const string RosMessageName = "beginner_tutorials/MoveBaseActionResult";

public Header header;
public GoalStatus status;
public MoveBaseResult result;

public MoveBaseActionResult()
{
header = new Header();
status = new GoalStatus();
result = new MoveBaseResult();
}
}
}

