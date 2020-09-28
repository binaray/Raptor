using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Messages;
using RosSharp.RosBridgeClient.Messages.Geometry;
using UnityEngine;

public enum Status{
    PENDING = 0,    // The goal has yet to be processed by the action server
    ACTIVE = 1,     // The goal is currently being processed by the action server
    PREEMPTED = 2,  // The goal received a cancel request after it started executing
                    //   and has since completed its execution (Terminal State)
    SUCCEEDED = 3,  // The goal was achieved successfully by the action server (Terminal State)
    ABORTED = 4,    // The goal was aborted during execution by the action server due
                    //    to some failure (Terminal State)
    REJECTED = 5,   // The goal was rejected by the action server without being processed,
                    //    because the goal was unattainable or invalid (Terminal State)
    PREEMPTING = 6, // The goal received a cancel request after it started executing
                    //    and has not yet completed execution
    RECALLING = 7,  // The goal received a cancel request before it started executing,
                    //    but the action server has not yet confirmed that the goal is canceled
    RECALLED = 8,   // The goal received a cancel request before it started executing
                    //    and was successfully cancelled (Terminal State)
    LOST = 9        // An action client can determine that a goal is LOST. This should not be
                    //    sent over the wire by an action server
}

public class MoveBaseActionClient : MonoBehaviour
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

    bool dataRc = false;
    private void Update()
    {
        if (dataRc)
        {
            print(ActionFeedback.feedback.base_position.pose.position.x);
            print(((Status)ActionResult.status.status).ToString());
        }
    }

    public void SetupAction(int raptorNo)
    {
        raptorNum = raptorNo;
        string actionName = string.Format("raptor{0}/move_base", raptorNum);
        CancelPublicationId = RaptorConnector.Instance.rosSocket.Advertise<RosSharp.RosBridgeClient.Messages.Actionlib.GoalID>(actionName + "/cancel");
        GoalPublicationId = RaptorConnector.Instance.rosSocket.Advertise<MoveBaseActionGoal>(actionName + "/goal");

        RaptorConnector.Instance.rosSocket.Subscribe<RosSharp.RosBridgeClient.Messages.Actionlib.GoalStatusArray>(actionName + "/status", StatusCallback, (int)(TimeStep * 1000));
        RaptorConnector.Instance.rosSocket.Subscribe<MoveBaseActionFeedback>(actionName + "/feedback", FeedbackCallback, (int)(TimeStep * 1000));
        RaptorConnector.Instance.rosSocket.Subscribe<MoveBaseActionResult>(actionName + "/result", ResultCallback, (int)(TimeStep * 1000));
    }
    public void SetTargetPoseAndSendGoal(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
    {
        CancelGoal();
        PoseStamped pose = new PoseStamped();
        pose.header.frame_id = string.Format("raptor{0}/base_link", raptorNum);
        pose.pose.position.x = position.x;
        pose.pose.position.y = position.y;
        pose.pose.position.z = position.z;

        pose.pose.orientation.x = rotation.x;
        pose.pose.orientation.y = rotation.y;
        pose.pose.orientation.z = rotation.z;
        pose.pose.orientation.w = rotation.w;
        targetPose = pose;
        SendGoal();
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
        dataRc = true;
    }
    void ResultCallback(MoveBaseActionResult result)
    {
        ActionResult = result;
        dataRc = true;
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
