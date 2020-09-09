using RosSharp.RosBridgeClient;

public class UnitActionClient : ActionClient<UnitActionGoal,
                                                      UnitActionFeedback,
                                                      UnitActionResult>
{
    public int Order;

    public override UnitActionGoal GetGoal()
    {
        return new UnitActionGoal() { goal = new UnitGoal { order = Order } };
    }

    public string PrintFeedback()
    {
        if (ActionFeedback == null)
            return "-";

        return PrintSequence(ActionFeedback.feedback.sequence);
    }

    public string PrintResult()
    {
        if (ActionResult == null)
            return "-";

        return PrintSequence(ActionResult.result.sequence);
    }

    public string PrintStatus()
    {
        return ActionState.ToString();
    }

    private static string PrintSequence(int[] intArray)
    {
        string result = "";
        for (int i = 0; i < intArray.Length; i++)
            result += " " + intArray[i];
        return result;
    }
}
