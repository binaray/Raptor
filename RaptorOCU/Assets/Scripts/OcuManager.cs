using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controllable;

public class OcuManager : MonoBehaviour
{
    private string _selectedUnit = null;
    public string SelectedUnit
    {
        get { return _selectedUnit; }
        set
        {
            //check previous selection then clear and reset buffers if any
            if (_selectedUnit != null)
            {
                controllableUnits[_selectedUnit].SetSelectedColors(false);
                IsManualControl = false;
            }

            if (value == null)
            {
                //close gui
            }
            else
            {
                //open and instantiate gui
                controllableUnits[value].SetSelectedColors(true);
            }
            _selectedUnit = value;
        }
    }

    private bool _isManualControl = false;
    public bool IsManualControl
    {
        get { return _isManualControl; }
        set
        {
            //turn on or off gui
            _isManualControl = value;
        }
    }

    //public List<Unit> controllableUnits = new List<Unit>();
    public Dictionary<string, Unit> controllableUnits = new Dictionary<string, Unit>();

    /*Test Values--TO REMOVE ON PRODUCTION*/
    float curSpeed = 10f;

    void Start()
    {
        //populate controllableUnits list
        Beacon b = GameObject.Find("beacon").GetComponent<Beacon>();
        controllableUnits.Add(b.id, b);
        //create units onscene
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                Debug.Log(hit.collider.gameObject.name);
                SelectedUnit = hit.collider.gameObject.GetComponent<Unit>().id;
            }
            else SelectedUnit = null;
        }


        if (SelectedUnit != null)
        {
            //movement controls
            if (IsManualControl)
            {
                controllableUnits[SelectedUnit].GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Lerp(0, Input.GetAxis("Horizontal") * curSpeed, 0.8f),
                    Mathf.Lerp(0, Input.GetAxis("Vertical") * curSpeed, 0.8f));
            }
        }
    }
}

