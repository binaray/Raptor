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
public class MoveBaseFeedback : Message
{
[JsonIgnore]
public const string RosMessageName = "beginner_tutorials/MoveBaseFeedback";

public PoseStamped base_position;

public MoveBaseFeedback()
{
base_position = new PoseStamped();
}
}
}

