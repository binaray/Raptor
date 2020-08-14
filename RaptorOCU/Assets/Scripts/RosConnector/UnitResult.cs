using Newtonsoft.Json;
using RosSharp.RosBridgeClient;


public class UnitResult : Message
{
    [JsonIgnore]
    public const string RosMessageName = "actionlib_tutorials/FibonacciResult";
    public int[] sequence;

    public UnitResult() { }
}
