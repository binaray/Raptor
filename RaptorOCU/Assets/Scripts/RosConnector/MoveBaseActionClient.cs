using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages;
using RosSharp.RosBridgeClient.Messages.Geometry;

public class MoveBaseActionClient
{
    public int raptorNum;
    public float TimeStep;
    public PoseStamped targetPose;

    private string CancelPublicationId;
    private string GoalPublicationId;
    private RosSharp.RosBridgeClient.Messages.Actionlib.GoalID ActionGoalId;
    private RosSharp.RosBridgeClient.Messages.Actionlib.GoalStatusArray ActionStatus;

    protected ActionServer<MoveBaseActionGoal, MoveBaseActionFeedback, MoveBaseActionResult>.ActionStates ActionState;
    protected MoveBaseActionGoal ActionGoal;
    protected MoveBaseActionFeedback ActionFeedback;
    protected MoveBaseActionResult ActionResult;

    public void SetupAction()
    {
        string actionName = "raptor1/move_base";
        CancelPublicationId = RaptorConnector.Instance.rosSocket.Advertise<RosSharp.RosBridgeClient.Messages.Actionlib.GoalID>(actionName + "/cancel");
        GoalPublicationId = RaptorConnector.Instance.rosSocket.Advertise<MoveBaseActionGoal>(actionName + "/goal");

        RaptorConnector.Instance.rosSocket.Subscribe<RosSharp.RosBridgeClient.Messages.Actionlib.GoalStatusArray>(actionName + "/status", StatusCallback, (int)(TimeStep * 1000));
        RaptorConnector.Instance.rosSocket.Subscribe<MoveBaseActionFeedback>(actionName + "/feedback", FeedbackCallback, (int)(TimeStep * 1000));
        RaptorConnector.Instance.rosSocket.Subscribe<MoveBaseActionResult>(actionName + "/result", ResultCallback, (int)(TimeStep * 1000));
    }
    public void SendGoal()
    {
        RaptorConnector.Instance.rosSocket.Publish(GoalPublicationId, new MoveBaseActionGoal() { goal = new MoveBaseGoal { target_pose = targetPose } });
    }
    public void CancelGoal()
    {
        ActionGoalId = new RosSharp.RosBridgeClient.Messages.Actionlib.GoalID();
        RaptorConnector.Instance.rosSocket.Publish(CancelPublicationId, ActionGoalId);
    }
    void FeedbackCallback(MoveBaseActionFeedback feedback)
    {
        ActionFeedback = feedback;
    }
    void ResultCallback(MoveBaseActionResult result)
    {
        ActionResult = result;
    }
    void StatusCallback(RosSharp.RosBridgeClient.Messages.Actionlib.GoalStatusArray actionStatus)
    {
        ActionStatus = actionStatus;
        ActionState = (ActionServer<MoveBaseActionGoal, MoveBaseActionFeedback, MoveBaseActionResult>.ActionStates)ActionStatus.status_list[0].status;
    }

    public string PrintFeedback()
    {
        if (ActionFeedback == null)
            return "-";

        return PrintPoseStamped(ActionFeedback.feedback.base_position);
    }

    public string PrintResult()
    {
        if (ActionResult == null)
            return "-";

        return "Completed movement";
    }

    public string PrintStatus()
    {
        return ActionState.ToString();
    }

    private static string PrintPoseStamped(PoseStamped poseStamped)
    {
        return string.Format("({0}, {1})", poseStamped.pose.position.x, poseStamped.pose.position.y);
    }
}
