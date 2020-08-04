using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Controllable;

public class UiManager : Singleton<UiManager>
{
    //LogDisp

    //SelectedUnitDisp
    [SerializeField]
    private GameObject selectedUnitDisplay;
    [SerializeField]
    private Button movementModeButton;


    public void ShowSelectedDisplayFor(Unit unit)
    {
        if (unit is Beacon) { }
        else if (unit is Payload) { }
        else { }
    }

    public void OnMovementModeButtonClick()
    {
        bool setManual = !OcuManager.Instance.IsManualControl;
        OcuManager.Instance.IsManualControl = setManual;
        MovementModeButtonSetManual(setManual);
    }

    private void MovementModeButtonSetManual(bool setManual)
    {
        if (setManual)
        {
            movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Manual";
        }
        else
        {
            movementModeButton.transform.GetChild(0).GetComponent<Text>().text = "Auto";
        }
    }

    public void ResetSelectedUnitDisplay()
    {
        MovementModeButtonSetManual(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
