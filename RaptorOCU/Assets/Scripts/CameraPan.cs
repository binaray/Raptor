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

    void MoveCam()
    {
        if (Input.mousePosition.x > Screen.width - panBorderSize)
        {
            transform.position += new Vector3(panSpeed * Time.deltaTime, 0, 0);
        }
        else if (Input.mousePosition.x < panBorderSize)
        {
            transform.position -= new Vector3(panSpeed * Time.deltaTime, 0, 0);
        }


        if (Input.mousePosition.y > Screen.height - panBorderSize)
        {
            transform.position += new Vector3(0, panSpeed * Time.deltaTime, 0);
        }
        else if (Input.mousePosition.y < panBorderSize)
        {
            transform.position -= new Vector3(0, panSpeed * Time.deltaTime, 0);
        }
    }


    // Update is called once per frame
    void Update()
    {
        MoveCam();
    }
}
