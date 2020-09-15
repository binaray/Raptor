using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Controllable;

public class UiManager : Singleton<UiManager>
{
    //ContextualHelp
    public Text helpDispText;

    //Ruler

    //Settings
    [SerializeField]
    private Button settingsButton;

    //GlobalCmds
    [SerializeField]
    private Button stopAllButton;

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
    private Button movementModeButton;
    [SerializeField]
    private Transform selectionButtons;

    //LogDisp



    public enum State
    {
        NoSelection,
        PayloadAuto,
        PayloadManual,
        BeaconAuto,
        BeaconManual
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
        movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Auto";
        while (currentState == State.NoSelection)
        {
            yield return null;
        }
    }

    IEnumerator PayloadAutoState()
    {
        helpDispText.text = @"-Point to Formation Movement Mode-
Units will form formation at selected position

Q/W: Rotate Left/Right   A/S: Scale down/up
Right click: Confirm";
        selectionButtons.gameObject.SetActive(true);
        unitText.text = "Payload " + OcuManager.Instance.SelectedUnit.id;
        movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Auto";
        OcuManager.Instance.projectionRend.gameObject.SetActive(true);
        while (currentState == State.PayloadAuto)
        {
            yield return null;
        }
        OcuManager.Instance.projectionRend.gameObject.SetActive(false);
        helpDispText.text = "";
    }

    IEnumerator PayloadManualState()
    {
        helpDispText.text = @"-Manual Movement Mode-
WASD or up, down, left, right keys or joystick to move";
        movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Manual";
        while (currentState == State.PayloadManual)
        {
            yield return null;
        }
        helpDispText.text = "";
    }

    IEnumerator BeaconAutoState()
    {
        helpDispText.text = @"-Point to Point Movement Mode-
Beacon will move to selected position

Right click: Confirm";
        selectionButtons.gameObject.SetActive(true);
        unitText.text = "Beacon " + OcuManager.Instance.SelectedUnit.id;
        movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Auto";
        OcuManager.Instance.projectionRend.gameObject.SetActive(true);
        while (currentState == State.BeaconAuto)
        {
            yield return null;
        }
        OcuManager.Instance.projectionRend.gameObject.SetActive(false);
        helpDispText.text = "";
    }

    IEnumerator BeaconManualState()
    {
        helpDispText.text = @"-Manual Movement Mode-
WASD or up, down, left, right keys or joystick to move";
        movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Manual";
        while (currentState == State.BeaconManual)
        {
            yield return null;
        }
        helpDispText.text = "";
    }

    public void ChangeState(State newState)
    {
        currentState = newState;
        StartCoroutine(newState.ToString() + "State");
        Debug.Log("Current state: " + currentState);
    }

    private void Update()
    {
        if (OcuManager.Instance.SelectedUnit != null)
        {
            unitLifeDisplay.GetChild(0).GetComponent<Text>().text = "ALIVE";
            unitPosition.text = ((Vector2)OcuManager.Instance.SelectedUnit.realPosition).ToString();
        }
    }

    public void OnMovementModeButtonClick()
    {
        if (currentState == State.BeaconAuto)
        {
            ChangeState(State.BeaconManual);
        }
        else if (currentState == State.BeaconManual)
        {
            ChangeState(State.BeaconAuto);
        }
        else if (currentState == State.PayloadAuto)
        {
            ChangeState(State.PayloadManual);
        }
        else if (currentState == State.PayloadManual)
        {
            ChangeState(State.PayloadAuto);
        }
    }
}
