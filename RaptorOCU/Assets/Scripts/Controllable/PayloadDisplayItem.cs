using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PayloadDisplayItem : MonoBehaviour
{
    public void SetText(string idText)
    {
        transform.GetChild(1).GetComponent<Text>().text = idText;
    }

    public void SetLifeDisplay(Controllable.Status status)
    {
        transform.GetChild(0).GetComponent<RawImage>().color = (status == Controllable.Status.Alive) ? Color.green : Color.red;
    }

    public void SetSelectionDisplay(bool isSelected)
    {
        if (isSelected) GetComponent<RawImage>().color = new Color(1f, 1f, 1f, 0.5f);
        else GetComponent<RawImage>().color = new Color(0, 0, 0, 0.5f);
    }
}
