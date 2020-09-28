using Controllable;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : Singleton<UiManager>
{
    //Planner gui graphic overlay
    [SerializeField]
    private GameObject plannerOverlay;
    [SerializeField]
    private Button executePlanButton;

    //ContextualHelp
    public Text helpDispText;

    //Ruler

    //Settings
    [SerializeField]
    private GameObject settingsPage;
    [SerializeField]
    private InputField ipAddressText;

    //GlobalCmds
    [SerializeField]
    private Button stopAllButton;
    [SerializeField]
    private Transform plannerModeButtonTransform;
    [SerializeField]
    private Button savePlanButton;
    [SerializeField]
    private Button loadPlanButton;

    //SelectedUnitDisp
    [SerializeField]
    private GameObject selectedUnitDisplay; //unused atm- to remove?
    [SerializeField]
    private Transform unitLifeDisplay;
    [SerializeField]
    private Text unitText;
    [SerializeField]
    private Text unitPosition;
    [SerializeField]
    private Transform selectionButtons;

    //ConfirmationDialogueBox
    [SerializeField]
    private GameObject confirmationDialogue;
    [SerializeField]
    private Text dialogueHeader;
    [SerializeField]
    private Text dialogueText;
    [SerializeField]
    private Button noButton;
    [SerializeField]
    private Button yesButton;

    //DynamicSelectionButtons
    private Transform pointToPointButton;
    private Transform pointToFormationButton;
    private Transform manualMovementButton;

    //LogDisp



    public enum State
    {
        NoSelection,
        SettingsPage,
        UnitSelected,
        PointToPoint,
        PointToFormation,
        ManualMovement
    }
    public State currentState
    {
        get;
        private set;
    }

    IEnumerator NoSelectionState()
    {
        helpDispText.text = @"-No Selection-
Left click on any unit on scene for contextual actions";
        selectionButtons.gameObject.SetActive(false);
        unitLifeDisplay.GetChild(0).GetComponent<Text>().text = "-";
        unitText.text = "Nothing Selected";
        unitPosition.text = "-";
        //movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Auto";
        while (currentState == State.NoSelection)
        {
            yield return null;
        }
    }

    IEnumerator SettingsPageState()
    {
        helpDispText.text = @"-No Selection-
Left click on any unit on scene for contextual actions";
        selectionButtons.gameObject.SetActive(false);
        unitLifeDisplay.GetChild(0).GetComponent<Text>().text = "-";
        unitText.text = "Nothing Selected";
        unitPosition.text = "-";
        //movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Auto";
        settingsPage.SetActive(true);
        ipAddressText.text = RaptorConnector.Instance.RosBridgeServerUrl;
        while (currentState == State.SettingsPage)
        {
            yield return null;
        }
        settingsPage.SetActive(false);
    }

    IEnumerator UnitSelectedState()
    {
        Unit selectedUnit = OcuManager.Instance.SelectedUnit;
        selectionButtons.gameObject.SetActive(true);
        if (selectedUnit is Payload)
        {
            helpDispText.text = @"-Payload " + selectedUnit.id + " Selected-";
            unitText.text = "Payload " + selectedUnit.id;
            pointToPointButton = selectionButtons.GetChild(0);
            pointToFormationButton = selectionButtons.GetChild(1);
            manualMovementButton = selectionButtons.GetChild(2);

            SetButtonType(pointToPointButton, State.PointToPoint);
            SetButtonType(pointToFormationButton, State.PointToFormation);
            SetButtonType(manualMovementButton, State.ManualMovement);
        }
        else if (selectedUnit is Beacon)
        {
            helpDispText.text = @"-Beacon " + OcuManager.Instance.SelectedUnit.id + " Selected-";
            unitText.text = "Beacon " + selectedUnit.id;
            pointToPointButton = selectionButtons.GetChild(0);
            manualMovementButton = selectionButtons.GetChild(1);
            selectionButtons.GetChild(2).gameObject.SetActive(false);

            SetButtonType(pointToPointButton, State.PointToPoint);
            SetButtonType(manualMovementButton, State.ManualMovement);
        }
        else if (selectedUnit is PlannerUnit)
        {
            helpDispText.text = @"-Payload Planner" + selectedUnit.id + " Selected-";
            unitText.text = "Payload Planner" + selectedUnit.id;
            pointToPointButton = selectionButtons.GetChild(0);
            pointToFormationButton = selectionButtons.GetChild(1);
            manualMovementButton = selectionButtons.GetChild(2);

            SetButtonType(pointToPointButton, State.PointToPoint);
            SetButtonType(pointToFormationButton, State.PointToFormation);
            SetButtonType(manualMovementButton, State.ManualMovement);
        }

        while (currentState == State.UnitSelected)
        {
            yield return null;
        }
    }

    IEnumerator PointToPointState()
    {
        helpDispText.text = @"-Point to Point Movement Mode-
Beacon will move to selected position

Right click: Confirm";

        SetButtonState(pointToPointButton, true);

        OcuManager.Instance.projectionRend.gameObject.SetActive(true);
        while (currentState == State.PointToPoint)
        {
            yield return null;
        }
        SetButtonState(pointToPointButton, false);
        if (currentState!=State.PointToFormation) OcuManager.Instance.projectionRend.gameObject.SetActive(false);
    }

    IEnumerator PointToFormationState()
    {
        helpDispText.text = @"-Point to Formation Movement Mode-
Units will form formation at selected position

Q/W: Rotate Left/Right   A/S: Scale down/up
Right click: Confirm";

        SetButtonState(pointToFormationButton, true);
        OcuManager.Instance.projectionRend.gameObject.SetActive(true);
        while (currentState == State.PointToFormation)
        {
            yield return null;
        }
        SetButtonState(pointToFormationButton, false);
        if (currentState!=State.PointToPoint) OcuManager.Instance.projectionRend.gameObject.SetActive(false);
    }

    IEnumerator ManualMovementState()
    {
        helpDispText.text = @"-Manual Movement Mode-
WASD or up, down, left, right keys or joystick to move";

        SetButtonState(manualMovementButton, true);
        while (currentState == State.ManualMovement)
        {
            yield return null;
        }
        SetButtonState(manualMovementButton, false);
    }

    /*-- State Changing methods --*/
    public void ChangeState(State newState)
    {
        currentState = newState;
        StartCoroutine(newState.ToString() + "State");
        Debug.Log("Current state: " + currentState);
    }

    public void DefaultNoSelectionState()
    {
        ChangeState(State.NoSelection);
    }

    public void OpenSettingsPageState()
    {
        ChangeState(State.SettingsPage);
    }

    /*-- Realtime Ui updates --*/
    private void Start()
    {
        executePlanButton.onClick.AddListener(() => { OcuManager.Instance.ExecutePlanForAllUnits(); });
        plannerModeButtonTransform.GetComponent<Button>().onClick.AddListener(()=>
        {
            OcuManager.Instance.IsPlannerMode = !OcuManager.Instance.IsPlannerMode;
            plannerOverlay.SetActive(OcuManager.Instance.IsPlannerMode);
            SetButtonState(plannerModeButtonTransform, OcuManager.Instance.IsPlannerMode,
                (OcuManager.Instance.IsPlannerMode) ? "Planner Mode: On" : "Planner Mode: Off");
        });
        stopAllButton.onClick.AddListener(() => { OcuManager.Instance.StopAllUnits(); });
    }
    private void Update()
    {
        if (OcuManager.Instance.SelectedUnit != null)
        {
            unitLifeDisplay.GetChild(0).GetComponent<Text>().text = "ALIVE";
            unitPosition.text = ((Vector2)OcuManager.Instance.SelectedUnit.realPosition).ToString();
        }
    }

    public void SettingsChangeRosAddressButton()
    {
        RaptorConnector.Instance.rosSocket.Close();
        RaptorConnector.Instance.RosBridgeServerUrl = ipAddressText.text;
        RaptorConnector.Instance.RosConnectionRoutine();
        //new System.Threading.Thread(RaptorConnector.Instance.ConnectAndWait).Start();
    }

    /*-- Helper and accessor methods --*/
    public void SetButtonType(Transform buttonTransform, State state)
    {
        buttonTransform.GetComponent<Button>().onClick.RemoveAllListeners();
        switch (state)
        {
            case State.PointToPoint:
                buttonTransform.GetChild(0).GetComponent<Text>().text = "Point to\nPoint";
                buttonTransform.GetChild(1).GetComponent<RawImage>().texture = Resources.Load<Texture>("Sprites/p_to_p");
                buttonTransform.GetComponent<Button>().onClick.AddListener(delegate { OnPointToPointButtonClick(); });
                break;
            case State.PointToFormation:
                buttonTransform.GetChild(0).GetComponent<Text>().text = "Point to\nFormation";
                buttonTransform.GetChild(1).GetComponent<RawImage>().texture = Resources.Load<Texture>("Sprites/p_to_f");
                buttonTransform.GetComponent<Button>().onClick.AddListener(delegate { OnPointToFormationButtonClick(); });
                break;
            case State.ManualMovement:
                buttonTransform.GetChild(0).GetComponent<Text>().text = "Manual\nMovement";
                buttonTransform.GetChild(1).GetComponent<RawImage>().texture = Resources.Load<Texture>("Sprites/man_");
                buttonTransform.GetComponent<Button>().onClick.AddListener(delegate { OnManualMovementButtonClick(); });
                break;
        }
    }
    public void SetButtonState(Transform buttonTransform, bool state, string newText = null)
    {
        if (newText != null) buttonTransform.transform.GetChild(0).GetComponent<Text>().text = newText;
            if (state)
        {
            buttonTransform.GetComponent<Image>().color = Color.white;
            buttonTransform.transform.GetChild(0).GetComponent<Text>().color = Color.black;
            if (buttonTransform.childCount > 1) 
                buttonTransform.transform.GetChild(1).GetComponent<RawImage>().color = Color.black;
        }
        else
        {
            buttonTransform.GetComponent<Image>().color = Color.black;
            buttonTransform.transform.GetChild(0).GetComponent<Text>().color = Color.white;
            if (buttonTransform.childCount > 1) 
                buttonTransform.transform.GetChild(1).GetComponent<RawImage>().color = Color.white;
        }
    }

    /*-- Button Events --*/
    public void OnPointToPointButtonClick()
    {
        if (currentState == State.PointToPoint)
        {
            ChangeState(State.UnitSelected);
        }
        else
        {
            ChangeState(State.PointToPoint);
        }
    }
    public void OnPointToFormationButtonClick()
    {
        if (currentState == State.PointToFormation)
        {
            ChangeState(State.UnitSelected);
        }
        else
        {
            ChangeState(State.PointToFormation);
        }
    }
    public void OnManualMovementButtonClick()
    {
        if (currentState == State.UnitSelected)
        {
            ChangeState(State.ManualMovement);
        }
        else if (currentState == State.ManualMovement)
        {
            ChangeState(State.UnitSelected);
        }
    }
}
