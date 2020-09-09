using Newtonsoft.Json;
using RosSharp.RosBridgeClient;

public class UnitFeedback : Message
{
    [JsonIgnore]
    public const string RosMessageName = "actionlib_tutorials/FibonacciFeedback";
    public int[] sequence;

    public UnitFeedback() { }
}
