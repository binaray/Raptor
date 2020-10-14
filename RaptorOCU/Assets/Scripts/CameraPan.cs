using Controllable;
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
    private float textXOffset = 0.05f;
    [SerializeField]
    private float textYOffset = 0.03f;

    [SerializeField]
    private float frameOffset = 2f;
    private Vector2 minFrameBound;
    private Vector2 maxFrameBound;
    private bool frameCheck = false;

    Vector2 lowerBound;
    Vector2 upperBound;
    Vector2 newLowerBound;
    float gridlineStep = 2.0f;

    void MoveCam()
    {
        //Get camera screen bounds in world space
        newLowerBound = Camera.main.ScreenToWorldPoint(Vector2.zero);
        newLowerBound.x = Mathf.Floor(newLowerBound.x);
        newLowerBound.x -= (newLowerBound.x % gridlineStep);
        newLowerBound.y = Mathf.Floor(newLowerBound.y);
        newLowerBound.y -= (newLowerBound.y % gridlineStep);
        upperBound = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        upperBound.x = Mathf.Ceil(upperBound.x);
        upperBound.y = Mathf.Ceil(upperBound.y);

        //Update camera min/max bounds from updated beacon positions
        foreach (string bId in OcuManager.Instance.beaconIds)
        {
            Vector2 bPos = OcuManager.Instance.controllableUnits[bId].transform.position;
            if (!frameCheck)
            {
                minFrameBound = bPos;
                maxFrameBound = bPos;
                frameCheck = true;
            }
            else
            {
                if (bPos.x < minFrameBound.x) minFrameBound.x = bPos.x;
                else if (bPos.x > maxFrameBound.x) maxFrameBound.x = bPos.x;
                if (bPos.y < minFrameBound.y) minFrameBound.y = bPos.y;
                else if (bPos.y > maxFrameBound.y) maxFrameBound.y = bPos.y;
            }
        }
        frameCheck = false;

        //Camera pan movement
        if (Input.mousePosition.x > Screen.width - panBorderSize)
        {
            if (upperBound.x < maxFrameBound.x + frameOffset)
            {
                transform.position += new Vector3(panSpeed * Time.deltaTime, 0, 0);
                RenderGridlines();
            }
        }
        else if (Input.mousePosition.x < panBorderSize)
        {
            if (newLowerBound.x > minFrameBound.x - frameOffset)
            {
                transform.position -= new Vector3(panSpeed * Time.deltaTime, 0, 0);
                RenderGridlines();
            }
        }

        if (Input.mousePosition.y > Screen.height - panBorderSize)
        {
            if (upperBound.y < maxFrameBound.y + frameOffset)
            {
                transform.position += new Vector3(0, panSpeed * Time.deltaTime, 0);
                RenderGridlines();
            }
        }
        else if (Input.mousePosition.y < panBorderSize)
        {
            if (newLowerBound.y > minFrameBound.y - frameOffset)
            {
                transform.position -= new Vector3(0, panSpeed * Time.deltaTime, 0);
                RenderGridlines();
            }
        }
    }

    void RenderGridlines(bool refresh = false)
    {
        if (newLowerBound == lowerBound && !refresh) return;
        lowerBound = newLowerBound;

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
                gridTextTransform.GetChild(textCount++).GetComponent<TMPro.TextMeshPro>().text = WorldScaler.WorldToRealPosition(x) + ", " + WorldScaler.WorldToRealPosition(y);
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

    public void ZoomIn() { 
        WorldScaler.worldScale = 2;
        RefreshScale();
    }

    public void ZoomOut() { 
        WorldScaler.worldScale = 1;
        RefreshScale();
    }

    private void RefreshScale()
    {
        MoveCam();
        RenderGridlines(true);
        OcuManager.Instance.RefreshUnitPos();
    }

    void Start()
    {
        RefreshScale();
    }



    void Update()
    {
        MoveCam();
        //RenderGridlines();
    }
}
