using Newtonsoft.Json;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages.Actionlib;
using RosSharp.RosBridgeClient.Messages.Standard;


public class UnitActionResult : Message
{
    [JsonIgnore]
    public const string RosMessageName = "actionlib_tutorials/FibonacciActionResult";
    public Header header;
    public GoalStatus status;
    public UnitResult result;

    public UnitActionResult() { }
}
