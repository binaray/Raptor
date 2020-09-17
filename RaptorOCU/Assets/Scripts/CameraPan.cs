using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        newLowerBound.y = Mathf.Floor(newLowerBound.y);
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
        for (float y = lowerBound.y; y < upperBound.y; y += gridlineStep)
        {
            drawPoints[0] = new Vector2(lowerBound.x, y);
            drawPoints[1] = new Vector2(upperBound.x, y);
            if (childNo <= gridlineRenderTransform.childCount) Instantiate(lineRenderer, gridlineRenderTransform);
            gridlineRenderTransform.GetChild(childNo++).GetComponent<LineRenderer>().SetPositions(drawPoints);
        }

        //Destroy unused lines
        //while (gridlineRenderTransform.childCount > childNo)
        //{
        //    Destroy(gridlineRenderTransform.GetChild(gridlineRenderTransform.childCount - 1));
        //}
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
