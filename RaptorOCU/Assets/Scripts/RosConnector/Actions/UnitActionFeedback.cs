using Newtonsoft.Json;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Actionlib;
using RosSharp.RosBridgeClient.Messages.Standard;

public class UnitActionFeedback : Message
{
    [JsonIgnore]
    public const string RosMessageName = "actionlib_tutorials/FibonacciActionFeedback";
    public Header header;
    public GoalStatus status;
    public UnitFeedback feedback;

    public UnitActionFeedback() { }
}
