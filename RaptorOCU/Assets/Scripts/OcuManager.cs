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
            ocuLogger.Logv(string.Format("{0} selected", value));
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
            ocuLogger.Logv(value ? "Manual control enabled" : "Auto control mode");
            _isManualControl = value;
        }
    }

    [SerializeField]
    private Beacon beaconPrefab;
    [SerializeField]
    private Payload payloadPrefab;
    //public List<Unit> controllableUnits = new List<Unit>();
    public Dictionary<string, Unit> controllableUnits = new Dictionary<string, Unit>();

    /*Test Values--TO REMOVE ON PRODUCTION*/
    [SerializeField]
    private float curSpeed = 5f;
    [SerializeField]
    private float rotSpeed = 60f;
    [SerializeField]
    private int beaconCount = 4;
    [SerializeField]
    private int payloadCount = 10;

    private OcuLogger ocuLogger;
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    IEnumerator MoveUnitToPositionCoroutine(Unit unit, Vector2 target)
    {
        while (Vector2.Distance(unit.transform.position, target) > .2f)
        {
            unit.MoveAndRotateTowards(target, curSpeed * Time.deltaTime, rotSpeed * Time.deltaTime);
            yield return null;
        }
    }

    void Start()
    {
        ocuLogger = OcuLogger.Instance;
        ocuLogger.Logv("Initializing--");

        //populate controllableUnits list
        //create units onscene
        for (int i=0; i < beaconCount; i++)
        {
            string id = string.Format("b{0}", i);
            Beacon b = Instantiate<Beacon>(beaconPrefab);
            b.id = id;
            b.num = i;
            b.realPosition = new Vector3(i%2*5, i/2*5, 0);
            ocuLogger.Logv(string.Format("Beacon of id {0} added at {1}", id, b.realPosition));
            controllableUnits.Add(id, b);
        }
        for (int i = 0; i < payloadCount; i++)
        {
            string id = string.Format("p{0}", i);
            Payload p = Instantiate<Payload>(payloadPrefab);
            p.id = id;
            p.num = i;
            p.realPosition = new Vector3(i, 3, 0);
            ocuLogger.Logv(string.Format("Payload of id {0} added at {1}", id, p.realPosition));
            controllableUnits.Add(id, p);
        }

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

