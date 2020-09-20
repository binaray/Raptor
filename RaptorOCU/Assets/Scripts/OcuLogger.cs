using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcuLogger : Singleton<OcuLogger>
{
    [SerializeField]
    private GameObject textTemplate;
    private List<GameObject> textItems = new List<GameObject>();


    //verbose
    public void Logv(string message)
    {
        print(message);
        _LogText(message, Color.white);
    }

    //warning
    public void Logw(string message)
    {
        Debug.LogWarning(message);
        _LogText(message, Color.yellow);
    }

    //error
    public void Loge(string message)
    {
        Debug.LogError(message);
        _LogText(message, Color.red);
    }

    private void _LogText(string message, Color color)
    {
        if (textItems.Count >= 50)
        {
            GameObject tempText = textItems[0];
            Destroy(tempText.gameObject);
            textItems.Remove(tempText);
        }
        GameObject newText = Instantiate(textTemplate);
        newText.SetActive(true);
        newText.GetComponent<OcuLogItem>().SetText(message, color);
        newText.transform.SetParent(textTemplate.transform.parent, false);
        textItems.Add(newText.gameObject);
    }
}
