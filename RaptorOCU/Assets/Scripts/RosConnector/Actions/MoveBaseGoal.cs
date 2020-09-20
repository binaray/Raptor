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
public class MoveBaseGoal : Message
{
[JsonIgnore]
public const string RosMessageName = "beginner_tutorials/MoveBaseGoal";

public PoseStamped target_pose;

public MoveBaseGoal()
{
target_pose = new PoseStamped();
}
}
}

