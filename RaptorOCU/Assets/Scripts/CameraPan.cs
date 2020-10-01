using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraPan : MonoBehaviour
{

    [SerializeField]
    private int panBorderSize = 30;
    [SerializeField]
    private float panSpeed = 10f;
    bool isCamMoving = false;

    [SerializeField]
    private GameObject lineRenderer;
    [SerializeField]
    private Transform gridlineRenderTransform;
    [SerializeField]
    private GameObject gridText;
    [SerializeField]
    private Transform gridTextTransform;
    [SerializeField]
    private float textXOffset = 0;
    [SerializeField]
    private float textYOffset = 0;

    Vector2 lowerBound;
    Vector2 upperBound;
    float gridlineStep = 2.0f;

    void MoveCam()
    {
        if (Input.mousePosition.x > Screen.width - panBorderSize)
        {
            transform.position += new Vector3(panSpeed * Time.deltaTime, 0, 0);
            RenderGridlines();
        }
        else if (Input.mousePosition.x < panBorderSize)
        {
            transform.position -= new Vector3(panSpeed * Time.deltaTime, 0, 0);
            RenderGridlines();
        }


        if (Input.mousePosition.y > Screen.height - panBorderSize)
        {
            transform.position += new Vector3(0, panSpeed * Time.deltaTime, 0);
            RenderGridlines();
        }
        else if (Input.mousePosition.y < panBorderSize)
        {
            transform.position -= new Vector3(0, panSpeed * Time.deltaTime, 0);
            RenderGridlines();
        }
    }

    void RenderGridlines()
    {
        Vector2 newLowerBound = Camera.main.ScreenToWorldPoint(Vector2.zero);
        newLowerBound.x = Mathf.Floor(newLowerBound.x);
        newLowerBound.x = newLowerBound.x - (newLowerBound.x % gridlineStep);
        newLowerBound.y = Mathf.Floor(newLowerBound.y);
        newLowerBound.y = newLowerBound.y - (newLowerBound.y % gridlineStep);
        if (newLowerBound == lowerBound) return;

        lowerBound = newLowerBound;
        upperBound = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        upperBound.x = Mathf.Ceil(upperBound.x);
        upperBound.y = Mathf.Ceil(upperBound.y);

        int childNo = 0;

        Vector3[] drawPoints = new Vector3[2];
        for (float x = lowerBound.x; x < upperBound.x; x += gridlineStep)
        {
            drawPoints[0] = new Vector2(x, lowerBound.y);
            drawPoints[1] = new Vector2(x, upperBound.y);
            if (childNo <= gridlineRenderTransform.childCount) Instantiate(lineRenderer, gridlineRenderTransform);
            gridlineRenderTransform.GetChild(childNo++).GetComponent<LineRenderer>().SetPositions(drawPoints);
        }
        int vertLineCount = childNo;

        for (float y = lowerBound.y; y < upperBound.y; y += gridlineStep)
        {
            drawPoints[0] = new Vector2(lowerBound.x, y);
            drawPoints[1] = new Vector2(upperBound.x, y);
            if (childNo <= gridlineRenderTransform.childCount) Instantiate(lineRenderer, gridlineRenderTransform);
            gridlineRenderTransform.GetChild(childNo++).GetComponent<LineRenderer>().SetPositions(drawPoints);
        }
        //int vertLineCount = horLineCount - childNo;

        int textCount = 0;
        for (int v = 0; v < vertLineCount; v++)
        {
            float x = gridlineRenderTransform.GetChild(v).GetComponent<LineRenderer>().GetPosition(0).x;
            for (int h = vertLineCount; h < childNo; h++)
            {
                float y = gridlineRenderTransform.GetChild(h).GetComponent<LineRenderer>().GetPosition(0).y;
                if (textCount <= gridTextTransform.childCount) Instantiate(gridText, gridTextTransform);
                gridTextTransform.GetChild(textCount).position = new Vector3(x + textXOffset, y + textYOffset, 0);
                gridTextTransform.GetChild(textCount++).GetComponent<TMPro.TextMeshPro>().text = x + ", " + y;
            }
        }

        //Destroy unused lines
        for (int i = gridlineRenderTransform.childCount; i > childNo; i--)
        {
            Destroy(gridlineRenderTransform.GetChild(i - 1).gameObject);
        }
        for (int i = gridTextTransform.childCount; i > textCount; i--)
        {
            Destroy(gridTextTransform.GetChild(i - 1).gameObject);
        }
    }

    private void Start()
    {
        RenderGridlines();
    }

    void Update()
    {
        MoveCam();
    }
}
