using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Controllable;

public class UiManager : Singleton<UiManager>
{
    //ContextualHelp
    private Text helpDispText;

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


    private void Update()
    {
        if (selectedUnitRef != null)
        {
            unitLifeDisplay.GetChild(0).GetComponent<Text>().text = "ALIVE";
            unitPosition.text = ((Vector2)selectedUnitRef.realPosition).ToString();
        }
    }

    private Unit selectedUnitRef = null;
    public void ShowSelectedDisplayFor(Unit unit)
    {
        selectedUnitRef = unit;
        if (unit is Beacon)
        {
            selectionButtons.gameObject.SetActive(true);
            unitText.text = "Beacon " + unit.id;
        }
        else if (unit is Payload)
        {
            selectionButtons.gameObject.SetActive(true);
            unitText.text = "Payload " + unit.id;
        }
        else
        {
            selectionButtons.gameObject.SetActive(false);
            unitLifeDisplay.GetChild(0).GetComponent<Text>().text = "-";
            unitText.text = "Nothing Selected";
            unitPosition.text = "-";
        }
    }

    public void OnMovementModeButtonClick()
    {
        bool setManual = !OcuManager.Instance.IsManualControl;
        OcuManager.ControlModes controlMode =
            (OcuManager.Instance.CurrentMode != OcuManager.ControlModes.Auto) 
            ? OcuManager.ControlModes.Auto 
            : OcuManager.ControlModes.Manual;
        SetMovementControlMode(controlMode);
        OcuManager.Instance.CurrentMode = controlMode;
    }

    private void SetMovementControlMode(OcuManager.ControlModes mode)
    {
        switch (mode)
        {
            case OcuManager.ControlModes.Auto:
                movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Auto";
                break;
            case OcuManager.ControlModes.Manual:
                movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Manual";
                break;
        }
    }

    public void ResetSelectedUnitDisplay()
    {
        SetMovementControlMode(OcuManager.ControlModes.Auto);
    }
}
