using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Controllable;
using TMPro;

public class UiManager : Singleton<UiManager>
{
    //ContextualHelp
    public Text helpDispText;

    //Ruler

    //Settings
    [SerializeField]
    private GameObject settingsPage;

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
    private Transform selectionButtons;
    [SerializeField]
    private GameObject pointToPointButton;
    [SerializeField]
    private GameObject manualMovementButton;

    //LogDisp



    public enum State
    {
        NoSelection,
        SettingsPage,
        PayloadSelected,
        PayloadAuto,
        PayloadManual,
        BeaconSelected,
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
        while (currentState == State.SettingsPage)
        {
            yield return null;
        }
        settingsPage.SetActive(false);
    }

    IEnumerator PayloadSelectedState()
    {
        helpDispText.text = @"-Payload " + OcuManager.Instance.SelectedUnit.id +" Selected-";
        selectionButtons.gameObject.SetActive(true);
        unitText.text = "Payload " + OcuManager.Instance.SelectedUnit.id;
        pointToPointButton.transform.GetChild(0).GetComponent<Text>().text = "Point to\nFormation";
        pointToPointButton.transform.GetChild(1).GetComponent<RawImage>().texture = Resources.Load<Texture>("Sprites/p_to_f");

        selectionButtons.gameObject.SetActive(true);
        while (currentState == State.PayloadSelected)
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

        ChangeButtonState(pointToPointButton, true);
        OcuManager.Instance.projectionRend.gameObject.SetActive(true);
        while (currentState == State.PayloadAuto)
        {
            yield return null;
        }
        ChangeButtonState(pointToPointButton, false);
        OcuManager.Instance.projectionRend.gameObject.SetActive(false);
    }

    IEnumerator PayloadManualState()
    {
        helpDispText.text = @"-Manual Movement Mode-
WASD or up, down, left, right keys or joystick to move";
        //movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Manual";

        ChangeButtonState(manualMovementButton, true);
        while (currentState == State.PayloadManual)
        {
            yield return null;
        }
        ChangeButtonState(manualMovementButton, false);
    }

    IEnumerator BeaconSelectedState()
    {
        helpDispText.text = @"-Beacon " + OcuManager.Instance.SelectedUnit.id + " Selected-";
        selectionButtons.gameObject.SetActive(true);
        unitText.text = "Beacon " + OcuManager.Instance.SelectedUnit.id;
        pointToPointButton.transform.GetChild(0).GetComponent<Text>().text = "Point to\nPoint";
        pointToPointButton.transform.GetChild(1).GetComponent<RawImage>().texture = Resources.Load<Texture>("Sprites/p_to_p");

        selectionButtons.gameObject.SetActive(true);
        while (currentState == State.BeaconSelected)
        {
            yield return null;
        }
    }

    IEnumerator BeaconAutoState()
    {
        helpDispText.text = @"-Point to Point Movement Mode-
Beacon will move to selected position

Right click: Confirm";

        ChangeButtonState(pointToPointButton, true);
        OcuManager.Instance.projectionRend.gameObject.SetActive(true);
        while (currentState == State.BeaconAuto)
        {
            yield return null;
        }
        ChangeButtonState(pointToPointButton, false);
        OcuManager.Instance.projectionRend.gameObject.SetActive(false);
    }

    IEnumerator BeaconManualState()
    {
        helpDispText.text = @"-Manual Movement Mode-
WASD or up, down, left, right keys or joystick to move";
        ChangeButtonState(manualMovementButton, true);
        while (currentState == State.BeaconManual)
        {
            yield return null;
        }
        ChangeButtonState(manualMovementButton, false);
    }

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

    private void Update()
    {
        if (OcuManager.Instance.SelectedUnit != null)
        {
            unitLifeDisplay.GetChild(0).GetComponent<Text>().text = "ALIVE";
            unitPosition.text = ((Vector2)OcuManager.Instance.SelectedUnit.realPosition).ToString();
        }
    }

    public void ChangeButtonState(GameObject button, bool state)
    {
        if (state)
        {
            button.GetComponent<Image>().color = Color.white;
            button.transform.GetChild(0).GetComponent<Text>().color = Color.black;
            button.transform.GetChild(1).GetComponent<RawImage>().color = Color.black;
        }
        else
        {
            button.GetComponent<Image>().color = Color.black;
            button.transform.GetChild(0).GetComponent<Text>().color = Color.white;
            button.transform.GetChild(1).GetComponent<RawImage>().color = Color.white;
        }
    }

    public void OnPointToPointButtonClick()
    {
        if (currentState == State.BeaconAuto)
        {
            ChangeState(State.BeaconSelected);
        }
        else if (currentState == State.BeaconSelected)
        {
            ChangeState(State.BeaconAuto);
        }
        else if (currentState == State.PayloadAuto)
        {
            ChangeState(State.PayloadSelected);
        }
        else if (currentState == State.PayloadSelected)
        {
            ChangeState(State.PayloadAuto);
        }
    }

    public void OnManualMovementModeButtonClick()
    {
        if (currentState == State.BeaconSelected)
        {
            ChangeState(State.BeaconManual);
        }
        else if (currentState == State.BeaconManual)
        {
            ChangeState(State.BeaconSelected);
        }
        else if (currentState == State.PayloadSelected)
        {
            ChangeState(State.PayloadManual);
        }
        else if (currentState == State.PayloadManual)
        {
            ChangeState(State.PayloadSelected);
        }
    }
}
