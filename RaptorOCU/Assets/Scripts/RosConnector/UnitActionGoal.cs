using Newtonsoft.Json;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Actionlib;
using RosSharp.RosBridgeClient.Messages.Standard;

public class UnitActionGoal : Message
{
    [JsonIgnore]
    public const string RosMessageName = "actionlib_tutorials/FibonacciActionGoal";
    public Header header;
    public GoalID goal_id;
    public UnitGoal goal;
}
