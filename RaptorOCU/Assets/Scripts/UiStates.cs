using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class UiStates : MonoBehaviour
{
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
        UiManager.Instance.helpDispText.text = @"-Point to Point Movement Mode-
Units will follow the selected payload movement
Right click anywhere on map to move";
        while (currentState == State.NoSelection)
        {
            yield return null;
        }
        UiManager.Instance.helpDispText.text = "";
    }

    IEnumerator PayloadAutoState()
    {
        UiManager.Instance.helpDispText.text = @"-Point to Formation Movement Mode-
Units will form formation at selected position
Q/W: Rotate left/right  A/S: Scale down/up
Right click at desired position on map to confirm";
        while (currentState == State.PayloadAuto)
        {
            yield return null;
        }
        UiManager.Instance.helpDispText.text = "";
    }

    public void ChangeState(State newState)
    {
        currentState = newState;
        StartCoroutine(newState.ToString() + "State");
        Debug.Log("Current state: " + currentState);
    }
}