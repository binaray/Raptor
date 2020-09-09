using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controllable;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Specialized;

public class OcuManager : Singleton<OcuManager>
{
    /* Selected unit id */
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
            
            //if a unit is selected
            if (value != null)
            {
                controllableUnits[value].SetSelectedColors(true);
                UiManager.Instance.ShowSelectedDisplayFor(controllableUnits[value]);
            }
            else //background is selected
            {
                UiManager.Instance.ResetSelectedUnitDisplay();
                UiManager.Instance.ShowSelectedDisplayFor(null);
            }
            _selectedUnit = value;
        }
    }

    /* Manual movement control flag for unit */
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

    /* Unit prefabs and reference container */
    [SerializeField]
    private Beacon beaconPrefab;
    [SerializeField]
    private Payload payloadPrefab;
    //public List<Unit> controllableUnits = new List<Unit>();
    public Dictionary<string, Unit> controllableUnits = new Dictionary<string, Unit>();

    /* Test Values--TO REMOVE ON PRODUCTION */
    [SerializeField]
    private float curSpeed = 5f;
    [SerializeField]
    private float rotSpeed = 60f;
    [SerializeField]
    private int beaconCount = 4;
    [SerializeField]
    private int payloadCount = 10;

    /* Scene graphical displays */
    [SerializeField]
    private LineRenderer boundaryLineRenderer;
    
    /* Logger */
    private OcuLogger ocuLogger;

    /* Raycasting variables for unit selection on scene */
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

    /* Convex hull algorithm for boundary drawer */
    HashSet<string> beaconIds = new HashSet<string>();
    Stack<Vector2> boundaryDrawPoints;
    Vector2 minPoint = new Vector2();
    
    // A utility function to find next to top in a stack 
    Vector2 NextToTop(Stack<Vector2> S)
    {
        Vector2 p = S.Pop();
        Vector2 res = S.Peek();
        S.Push(p);
        return res;
    }
    
    // To find orientation of ordered triplet (p, q, r). 
    // The function returns following values 
    // 0 --> p, q and r are colinear 
    // 1 --> Clockwise 
    // 2 --> Counterclockwise 
    int Orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        int val = System.Convert.ToInt32(
                    (q.y - p.y) * (r.x - q.x) -
                    (q.x - p.x) * (r.y - q.y));
        if (val == 0) return 0;  // colinear 
        return (val > 0) ? 1 : 2; // clock or counterclock wise 
    }

    // A function used by library function qsort() to sort an array of 
    // points with respect to the first point 
    int ComparePolarAngle(Vector2 p1, Vector2 p2) 
    { 
        // Find orientation 
        int o = Orientation(minPoint, p1, p2); 
        if (o == 0) 
            return (Vector2.Distance(minPoint, p2) >= Vector2.Distance(minPoint, p1))? -1 : 1; 
        return (o == 2)? -1: 1; 
    }

    void GrahamScanBeacons()
    {
        List<Vector2> points = new List<Vector2>();

        //1. Push min point to the first index of list
        bool firstIteration = true;
        foreach (string id in beaconIds)
        {
            if (firstIteration)
            {
                minPoint = controllableUnits[id].realPosition;
                firstIteration = false;
            }
            else
            {
                Vector2 beaconPos = controllableUnits[id].realPosition;
                if (beaconPos.y < minPoint.y || beaconPos.y == minPoint.y && beaconPos.x < minPoint.y)
                {
                    points.Add(minPoint);
                    minPoint = beaconPos;
                }
                else
                    points.Add(beaconPos);
            }
        }
        
        //2. Sort by polar angles; drop furthest if the same
        points.Sort(ComparePolarAngle);

        //3. Drop near points which have the same angles; leaving only the furthest
        List<int> toRemove = new List<int>();
        for (int i = 1; i < points.Count; i++)
        {
            // Keep removing i while angle of i and i+1 is same 
            // with respect to p0 
            while (i < points.Count - 1 && Orientation(minPoint, points[i], points[i + 1]) == 0)
            {
                toRemove.Add(i);
                i++;
            }
        }
        for (int i = toRemove.Count - 1; i > -1; i--)
        {
            points.RemoveAt(toRemove[i]);
        }

        //4. Push points onto a stack
        if (points.Count < 2) return;
        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(minPoint);
        stack.Push(points[0]);
        stack.Push(points[1]);
        // Process remaining n-3 points 
        for (int i = 2; i < points.Count; i++)
        {
            // Keep removing top while the angle formed by 
            // points next-to-top, top, and points[i] makes 
            // a non-left turn 
            while (Orientation(NextToTop(stack), stack.Peek(), points[i]) != 2)
                stack.Pop();
            stack.Push(points[i]);
        }

        boundaryLineRenderer.positionCount = stack.Count;
        int c = 0;
        while (stack.Count > 0)
        {
            Vector2 p = stack.Pop();
            boundaryLineRenderer.SetPosition(c++, p);
            print(p.ToString());
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
            beaconIds.Add(id);
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

    bool pointToFormationMovement = true;
    float formationProjScale = 1;
    float degreeOffset = 0;
    int operationalRobotCount = 5;
    public Transform projectionRend;

    void Update()
    {
        //TODO: case by states
        if (pointToFormationMovement)
        {
            Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //equation of circle: (x – h)^2 + (y – k)^2 = r^2, where (h, k) is the center of the circle
            double d = 360.0 / operationalRobotCount;
            for (int n = 0; n < operationalRobotCount; n++)
            {
                double angle = Math.PI * (degreeOffset + d * n) / 180.0;

                float s = (float)Math.Sin(angle);
                float c = (float)Math.Cos(angle);

                // translate point back to origin:
                Vector3 p = new Vector2(mousePos2D.x, formationProjScale);
                p.x -= mousePos2D.x;
                p.y -= mousePos2D.y;

                // rotate point
                float xnew = p.x * c - p.y * s;
                float ynew = p.x * s + p.y * c;

                // translate point back:
                p.x = xnew + mousePos2D.x;
                p.y = ynew + mousePos2D.y;
                projectionRend.GetChild(n).position = p;
            }
        }

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

        // Bg Graphic updates
        GrahamScanBeacons();
    }
}

