using RosSharp.RosBridgeClient;
using Newtonsoft.Json;

public class UnitGoal : Message
{
    [JsonIgnore]
    public const string RosMessageName = "actionlib_tutorials/FibonacciGoal";
    public int order;
}

