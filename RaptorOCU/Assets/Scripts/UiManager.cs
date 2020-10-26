using Controllable;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Collections.Generic;

public class UiManager : Singleton<UiManager>
{
    #region Prefabs, scene references and user defined variables
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
    private GameObject selectedUnitDisplay;
    [SerializeField]
    private Transform unitLifeDisplay;
    [SerializeField]
    private Text unitText;
    [SerializeField]
    private Text unitPosition;
    [SerializeField]
    private Text currentActionText;
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
    private InputField reconnectIpInput;
    [SerializeField]
    private Button noButton;
    [SerializeField]
    private Button yesButton;
    #endregion

    #region Runtime variables
    public State currentState
    {
        get;
        private set;
    }

    //DynamicSelectionButtons
    private Transform pointToPointButton;
    private Transform pointToFormationButton;
    private Transform cPointToFormationButton;
    private Transform manualMovementButton;
    #endregion

    #region UI states
    public enum State
    {
        NoSelection,
        SettingsPage,
        UnitSelected,
        PointToPoint,
        PointToFormation,
        PointToCustomFormation,
        ManualMovement
    }

    IEnumerator NoSelectionState()
    {
        helpDispText.text = @"-No Selection-
Left click on any unit on scene for contextual actions";
        selectionButtons.gameObject.SetActive(false);

        unitLifeDisplay.GetComponent<RawImage>().color = Color.white;
        unitLifeDisplay.GetChild(0).GetComponent<Text>().color = Color.white;
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
            helpDispText.text = "-Payload " + selectedUnit.id + " Selected-\n" +
"ESC key to deselect";
            unitText.text = "Payload " + selectedUnit.id;
            pointToPointButton = selectionButtons.GetChild(0);
            pointToFormationButton = selectionButtons.GetChild(1);
            cPointToFormationButton = selectionButtons.GetChild(2);
            manualMovementButton = selectionButtons.GetChild(3);

            SetSelectionButtonType(pointToPointButton, State.PointToPoint);
            SetSelectionButtonType(pointToFormationButton, State.PointToFormation);
            SetSelectionButtonType(cPointToFormationButton, State.PointToCustomFormation);
            SetSelectionButtonType(manualMovementButton, State.ManualMovement);
        }
        else if (selectedUnit is Beacon)
        {
            helpDispText.text = "-Beacon " + OcuManager.Instance.SelectedUnit.id + " Selected-\n" +
"ESC key to deselect";
            unitText.text = "Beacon " + selectedUnit.id;
            pointToPointButton = selectionButtons.GetChild(0);
            manualMovementButton = selectionButtons.GetChild(1);
            selectionButtons.GetChild(2).gameObject.SetActive(false);
            selectionButtons.GetChild(3).gameObject.SetActive(false);

            SetSelectionButtonType(pointToPointButton, State.PointToPoint);
            SetSelectionButtonType(manualMovementButton, State.ManualMovement);
        }
        else if (selectedUnit is PlannerUnit)
        {
            helpDispText.text = "-Payload Planner" + selectedUnit.id + " Selected-\n" +
"ESC key to deselect";
            unitText.text = "Payload Planner" + selectedUnit.id;
            pointToPointButton = selectionButtons.GetChild(0);
            pointToFormationButton = selectionButtons.GetChild(1);
            manualMovementButton = selectionButtons.GetChild(2);
            selectionButtons.GetChild(3).gameObject.SetActive(false);

            SetSelectionButtonType(pointToPointButton, State.PointToPoint);
            SetSelectionButtonType(pointToFormationButton, State.PointToFormation);
            SetSelectionButtonType(manualMovementButton, State.ManualMovement);
        }

        //Turn off all buttons in case
        SetButtonState(pointToPointButton, false);
        SetButtonState(pointToFormationButton, false);
        SetButtonState(manualMovementButton, false);

        while (currentState == State.UnitSelected)
        {
            yield return null;
        }
    }

    IEnumerator PointToPointState()
    {
        helpDispText.text = @"-Point to Point Movement Mode-
Selected unit will move to selected position

Q/W: Rotate Left/Right
Right click: Confirm";

        SetButtonState(pointToPointButton, true);

        OcuManager.Instance.ShowSingleProjectionUnit();
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
All payloads will form formation at selected position

Q/W: Rotate Left/Right   A/S: Scale down/up
Right click: Confirm";

        SetButtonState(pointToFormationButton, true);

        OcuManager.Instance.RefreshProjectionUnits();
        OcuManager.Instance.projectionRend.gameObject.SetActive(true);
        while (currentState == State.PointToFormation)
        {
            yield return null;
        }
        SetButtonState(pointToFormationButton, false);
        if (currentState!=State.PointToPoint) OcuManager.Instance.projectionRend.gameObject.SetActive(false);
    }

    IEnumerator PointToCustomFormationState()
    {
        helpDispText.text = @"-Point to Custom Formation Movement Mode-
All payloads will form formation at selected position

Q/W: Rotate Left/Right
Right click: Confirm";

        SetButtonState(cPointToFormationButton, true);

        OcuManager.Instance.RefreshProjectionUnits();
        OcuManager.Instance.projectionRend.gameObject.SetActive(true);
        while (currentState == State.PointToCustomFormation)
        {
            yield return null;
        }
        SetButtonState(cPointToFormationButton, false);
        if (currentState != State.PointToPoint) OcuManager.Instance.projectionRend.gameObject.SetActive(false);
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
    #endregion

    #region UI state Changing methods
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
    #endregion

    #region Settings Page button functions
    public void SettingsChangeRosAddressButton()
    {
        //TODO: REMOVE OR FIX THIS
        //RaptorConnector.Instance.rosSocket.Close();
        //PlayerPrefs.SetString(PlayerPrefsConstants.ROS_BRIDGE_IP, ipAddressText.text);
        //RaptorConnector.Instance.RosConnectionRoutine();
    }
    #endregion

    #region Selection and global button methods
    private void SetSelectionButtonType(Transform buttonTransform, State state)
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
            case State.PointToCustomFormation:
                buttonTransform.GetChild(0).GetComponent<Text>().text = "Point to\nCustom Formation";
                buttonTransform.GetChild(1).GetComponent<RawImage>().texture = Resources.Load<Texture>("Sprites/p_to_cf");
                buttonTransform.GetComponent<Button>().onClick.AddListener(delegate { OnPointToCustomFormationButtonClick(); });
                break;
            case State.ManualMovement:
                buttonTransform.GetChild(0).GetComponent<Text>().text = "Manual\nMovement";
                buttonTransform.GetChild(1).GetComponent<RawImage>().texture = Resources.Load<Texture>("Sprites/man_");
                buttonTransform.GetComponent<Button>().onClick.AddListener(delegate { OnManualMovementButtonClick(); });
                break;
        }
        buttonTransform.gameObject.SetActive(true);
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
    public void OnPointToCustomFormationButtonClick()
    {
        if (currentState == State.PointToCustomFormation)
        {
            ChangeState(State.UnitSelected);
        }
        else
        {
            StartCoroutine(ShowLoadCustomFormationDialogCoroutine());
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
    public void SetIsPlannerMode(bool isPlannerMode)
    {
        OcuManager.Instance.IsPlannerMode = isPlannerMode;
        plannerOverlay.SetActive(OcuManager.Instance.IsPlannerMode);
        SetButtonState(plannerModeButtonTransform, OcuManager.Instance.IsPlannerMode,
            (OcuManager.Instance.IsPlannerMode) ? "Planner Mode: On" : "Planner Mode: Off");
    }
    #endregion

    #region Reconnect Dialog
    public void ReconnectDialog()
    {
        reconnectIpInput.text = PlayerPrefs.GetString(PlayerPrefsConstants.ROS_BRIDGE_IP, null);
        confirmationDialogue.SetActive(true);
    }

    public void ReconnectWithNewIp()
    {
        RaptorConnector.Instance.SetRosIp(reconnectIpInput.text);
        RaptorConnector.Instance.RosConnectionRoutine();
        confirmationDialogue.SetActive(false);
    }
    #endregion

    #region Save and load plan dialogs
    IEnumerator ShowSaveDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Allow multiple selection: true
        // Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        if (OcuManager.Instance.IsPlannerMode)
            yield return FileBrowser.WaitForSaveDialog(false, false, PlayerPrefs.GetString(PlayerPrefsConstants.DEFAULT_SAVE_PATH, null), 
                "Save Payload Positions as Plan", "Save");
        else
            yield return FileBrowser.WaitForSaveDialog(false, false, PlayerPrefs.GetString(PlayerPrefsConstants.DEFAULT_SAVE_PATH, null), 
                "Save Planned Payload Positions as Plan", "Save");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            string path = "";
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
            {
                Debug.Log(FileBrowser.Result[i]);
                path = FileBrowser.Result[i];
            }
            PlayerPrefs.SetString(PlayerPrefsConstants.DEFAULT_SAVE_PATH, System.IO.Path.GetDirectoryName(path));

            RaptorPlanData data = new RaptorPlanData();
            if (OcuManager.Instance.IsPlannerMode)
            {
                foreach (PlannerUnit u in OcuManager.Instance.plannerUnits)
                {
                    PayloadData p = new PayloadData();
                    p.Init(u.realPosition, u.realRotation);
                    data.payloadDatas.Add(p);
                }
            }
            else
            {
                foreach (KeyValuePair<string, Unit> u in OcuManager.Instance.controllableUnits)
                {
                    if (u.Value is Payload)
                    {
                        PayloadData p = new PayloadData();
                        p.Init(u.Value.realPosition, u.Value.realRotation);
                        data.payloadDatas.Add(p);
                    }
                    else if (u.Value is Beacon)
                    {
                        BeaconData b = new BeaconData();
                        b.Init(u.Value.realPosition, u.Value.realRotation);
                        data.beaconDatas.Add(b);
                    }
                }
            }
            string json = data.ToJson();
            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);
            System.IO.File.WriteAllBytes(path, byteArray);
        }
    }
    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Allow multiple selection: true
        // Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(false, true, PlayerPrefs.GetString(PlayerPrefsConstants.DEFAULT_SAVE_PATH, null), "Load Plan", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                PlayerPrefs.SetString(PlayerPrefsConstants.DEFAULT_SAVE_PATH, System.IO.Path.GetDirectoryName(FileBrowser.Result[i]));

            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            RaptorPlanData data = JsonUtility.FromJson<RaptorPlanData>(System.Text.Encoding.UTF8.GetString(bytes));

            SetIsPlannerMode(true);
            for (int i=0; i<data.payloadDatas.Count && i< OcuManager.Instance.plannerUnits.Count; i++)
            {
                OcuManager.Instance.plannerUnits[i].LoadPayloadData(data.payloadDatas[i]);
            }
            //print(res);
        }
    }
    IEnumerator ShowLoadCustomFormationDialogCoroutine()
    {
        yield return FileBrowser.WaitForLoadDialog(false, true, PlayerPrefs.GetString(PlayerPrefsConstants.DEFAULT_SAVE_PATH, null), "Load Custom Formation", "Load");

        if (FileBrowser.Success)
        {
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                PlayerPrefs.SetString(PlayerPrefsConstants.DEFAULT_SAVE_PATH, System.IO.Path.GetDirectoryName(FileBrowser.Result[i]));
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            RaptorPlanData data = JsonUtility.FromJson<RaptorPlanData>(System.Text.Encoding.UTF8.GetString(bytes));

            List<PayloadData> customFormationData = new List<PayloadData>();
            PayloadData pivot = new PayloadData();
            pivot.position = Vector3.zero;
            pivot.rotation = data.payloadDatas[0].rotation;
            customFormationData.Add(pivot);
            for (int i = 1; i < data.payloadDatas.Count; i++)
            {
                PayloadData pd = new PayloadData();
                pd.position = data.payloadDatas[i].position - data.payloadDatas[0].position;
                pd.position = WorldScaler.RealToWorldPosition(pd.position);
                pd.rotation = data.payloadDatas[i].rotation;
                customFormationData.Add(pd);
            }
            OcuManager.Instance.customFormationData = customFormationData;

            print(OcuManager.Instance.customFormationData.Count);
            ChangeState(State.PointToCustomFormation);
        }
        else
        {
            if (OcuManager.Instance.SelectedUnit != null) ChangeState(State.UnitSelected);
            else ChangeState(State.NoSelection);
        }
    }
    #endregion

    #region Unity runtime
    private void Start()
    {
        executePlanButton.onClick.AddListener(() => { OcuManager.Instance.ExecutePlanForAllUnits(); });
        plannerModeButtonTransform.GetComponent<Button>().onClick.AddListener(() => { SetIsPlannerMode(!OcuManager.Instance.IsPlannerMode); });
        stopAllButton.onClick.AddListener(() => { OcuManager.Instance.StopAllUnits(); });

        FileBrowser.SetFilters(true, new FileBrowser.Filter("Raptor Plan",".json"));//, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Text Files", ".txt", ".pdf"));
        FileBrowser.SetDefaultFilter(".json");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");
        FileBrowser.AddQuickLink("Users", "", null);
        savePlanButton.onClick.AddListener(() => { StartCoroutine(ShowSaveDialogCoroutine()); });
        loadPlanButton.onClick.AddListener(() => { StartCoroutine(ShowLoadDialogCoroutine()); });
    }
    private void Update()
    {
        if (OcuManager.Instance.SelectedUnit != null)
        {
            Unit selectedUnit = OcuManager.Instance.SelectedUnit;
            if ((selectedUnit.status == Controllable.Status.Alive)) {
                unitLifeDisplay.GetComponent<RawImage>().color = Color.green;
                unitLifeDisplay.GetChild(0).GetComponent<Text>().color = Color.green;
            }
            else
            {
                unitLifeDisplay.GetComponent<RawImage>().color = Color.red;
                unitLifeDisplay.GetChild(0).GetComponent<Text>().color = Color.red;
            }
            unitLifeDisplay.GetChild(0).GetComponent<Text>().text = selectedUnit.status.ToString();
            unitPosition.text = string.Format("{0}, {1}", selectedUnit.realPosition.x.ToString("0.00"), selectedUnit.realPosition.y.ToString("0.00")); //((Vector2)selectedUnit.realPosition).ToString();
            currentActionText.text = (selectedUnit is PlannerUnit) ? ((PlannerUnit)selectedUnit).parentUnit.GetActionStatus()
                : selectedUnit.GetActionStatus();
        }
    }
    #endregion
}
