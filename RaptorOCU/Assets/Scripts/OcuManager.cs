using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controllable;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OcuManager : Singleton<OcuManager>
{
    private string _selectedUnit = null;
    public string SelectedUnit
    {
        get { return _selectedUnit; }
        set
        {
            //check previous selection then clear and reset buffers if any
            if (_selectedUnit != null && _selectedUnit != value)
            {
                controllableUnits[_selectedUnit].SetSelectedColors(false);
                IsManualControl = false;
            }
            

            if (value != null)
            {
                controllableUnits[value].SetSelectedColors(true);
                UiManager.Instance.ShowSelectedDisplayFor(controllableUnits[value]);
            }
            else
            {
                UiManager.Instance.ResetSelectedUnitDisplay();
                UiManager.Instance.ShowSelectedDisplayFor(null);
            }
            _selectedUnit = value;
        }
    }
    private bool _isManualControl = false;
    public bool IsManualControl
    {
        get
        {
            return _isManualControl;
        }
        set
        {
            ocuLogger.Log(value ? "Manual control enabled" : "Auto control mode");
            _isManualControl = value;
        }
    }

    //public List<Unit> controllableUnits = new List<Unit>();
    public Dictionary<string, Unit> controllableUnits = new Dictionary<string, Unit>();

    /*Test Values--TO REMOVE ON PRODUCTION*/
    float curSpeed = 5f;
    float rotSpeed = 60f;

    private OcuLogger ocuLogger;
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    IEnumerator MoveUnitToPositionCoroutine(Unit unit, Vector2 target)
    {
        while (Vector2.Distance(unit.position, target) > 1f)
        {
            unit.MoveAndRotateTowards(target, curSpeed * Time.deltaTime, rotSpeed * Time.deltaTime);
            yield return null;
        }
    }

    void Start()
    {
        ocuLogger = OcuLogger.Instance;
        ocuLogger.Log("Initializing--");
        //populate controllableUnits list
        Beacon b = GameObject.Find("beacon").GetComponent<Beacon>();
        Payload p = GameObject.Find("payload").GetComponent<Payload>();
        controllableUnits.Add(b.id, b);
        controllableUnits.Add(p.id, p);
        //create units onscene

        m_Raycaster = UiManager.Instance.GetComponent<GraphicRaycaster>();
        m_EventSystem = GetComponent<EventSystem>();
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //First check if there are any UI element collisions
            m_PointerEventData = new PointerEventData(m_EventSystem);
            m_PointerEventData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            m_Raycaster.Raycast(m_PointerEventData, results);
            if (results.Count > 0)
            {
                Debug.Log("Hit " + results[0].gameObject.layer);
            }
            else
            {
                //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.collider != null)
                {
                    SelectedUnit = hit.collider.gameObject.GetComponent<Unit>().id;
                }
                else SelectedUnit = null;
            }
        }


        if (SelectedUnit != null)
        {
            //movement controls
            if (IsManualControl)
            {
                //controllableUnits[SelectedUnit].GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Lerp(0, Input.GetAxis("Horizontal") * curSpeed, 0.8f),
                //    Mathf.Lerp(0, Input.GetAxis("Vertical") * curSpeed, 0.8f));

                controllableUnits[SelectedUnit].MoveForward(Input.GetAxis("Vertical") * curSpeed * Time.deltaTime);
                controllableUnits[SelectedUnit].Rotate(Input.GetAxis("Horizontal") * rotSpeed * Time.deltaTime * Vector3.back);

                //if (Input.GetKey("up")) //Up arrow key to move forwards
                //    controllableUnits[SelectedUnit].MoveForward(curSpeed * Time.deltaTime);
                //else if (Input.GetKey("down"))//Down arrow key to move backwards
                //    controllableUnits[SelectedUnit].MoveForward(-curSpeed * Time.deltaTime);
                //if (Input.GetKey("right")) //Right arrow key to turn right
                //    controllableUnits[SelectedUnit].Rotate(-Vector3.forward * rotSpeed * Time.deltaTime);
                //else if (Input.GetKey("left"))//Left arrow key to turn left
                //    controllableUnits[SelectedUnit].Rotate(Vector3.forward * rotSpeed * Time.deltaTime);
            }
            else
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    StartCoroutine(MoveUnitToPositionCoroutine(controllableUnits[SelectedUnit], mousePos2D));
                    //print(mousePos2D.ToString());
                }
            }
        }
    }
}

