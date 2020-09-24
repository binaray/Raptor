using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controllable;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class OcuManager : Singleton<OcuManager>
{
    /* Selected unit id */
    private Unit _selectedUnit = null;
    public Unit SelectedUnit
    {
        get { return _selectedUnit; }
        set
        {
            //check previous selection then clear and reset buffers if any
            if (_selectedUnit != null && _selectedUnit != value)
            {
                _selectedUnit.SetSelectedColors(false);
            }

            _selectedUnit = value;
            //if a unit is selected
            if (value != null)
            {
                ocuLogger.Logv(string.Format("{0} selected", value.id));
                value.SetSelectedColors(true);
                UiManager.Instance.ChangeState(UiManager.State.UnitSelected);
            }
            else //background is selected
            {
                ocuLogger.Logv("Nothing selected");
                UiManager.Instance.ChangeState(UiManager.State.NoSelection);
            }
        }
    }

    /* Unit prefabs and reference container */
    [SerializeField]
    private Beacon beaconPrefab;
    [SerializeField]
    private Payload payloadPrefab;
    public Dictionary<string, Unit> controllableUnits = new Dictionary<string, Unit>();
    [SerializeField]
    private GameObject payloadDispTemplate;
    [SerializeField]
    private GameObject beaconGuiTemplate;

    /* Test Values--TO REMOVE ON PRODUCTION */
    [SerializeField]
    private float curSpeed = 5f;
    [SerializeField]
    private float rotSpeed = 60f;
    [SerializeField]
    private int beaconCount = 4;
    [SerializeField]
    private int payloadCount = 5;

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
            //print(p.ToString());
        }
    }

    void Start()
    {
        ocuLogger = OcuLogger.Instance;

        //Clear UI from any test selections on scene
        //UiManager.Instance.ShowSelectedDisplayFor(null);
        UiManager.Instance.ChangeState(UiManager.State.NoSelection);

        m_Raycaster = UiManager.Instance.GetComponent<GraphicRaycaster>();
        m_EventSystem = GetComponent<EventSystem>();
    }

    public void InitUnits(bool uiTestMode = false)
    {
        ocuLogger.Logv("Initializing units from available Odometry data..");
        //populate controllableUnits list
        //create units onscene

        if (uiTestMode)
        {
            for (int i = 0; i < beaconCount; i++)
            {
                string id = string.Format("b{0}", i);
                Beacon b = Instantiate(beaconPrefab);
                b.Init(id, i, new Vector3(i % 2 * 6, i / 2 * 6, 0));
                ocuLogger.Logv(string.Format("Beacon of id {0} added at {1}", id, b.realPosition));
                controllableUnits.Add(id, b);
                beaconIds.Add(id);
            }

            for (int i = 0; i < payloadCount; i++)
            {
                string id = string.Format("p{0}", i);
                Payload p = Instantiate(payloadPrefab);
                GameObject newPayloadDisp = Instantiate(payloadDispTemplate);
                newPayloadDisp.SetActive(true);
                newPayloadDisp.transform.SetParent(payloadDispTemplate.transform.parent, false);
                p.payloadDisplay = newPayloadDisp;
                p.Init(id, i, new Vector3(i + 1, 3, 0));

                ocuLogger.Logv(string.Format("Payload of id {0} added at {1}", id, p.realPosition));
                controllableUnits.Add(id, p);
            }
            return;
        }

        for (int i = 0; i < beaconCount; i++)
        {
            string id = string.Format("b{0}", i);
            Beacon b = Instantiate(beaconPrefab);
            b.Init(id, i, new Vector3(i % 2 * 6, i / 2 * 6, 0));
            ocuLogger.Logv(string.Format("Beacon of id {0} added at {1}", id, b.realPosition));
            controllableUnits.Add(id, b);
            beaconIds.Add(id);
        }

        for (int i = 0; i < payloadCount; i++)
        {
            string id = string.Format("p{0}", i);
            Payload p = Instantiate(payloadPrefab);
            GameObject newPayloadDisp = Instantiate(payloadDispTemplate);
            newPayloadDisp.SetActive(true);
            newPayloadDisp.transform.SetParent(payloadDispTemplate.transform.parent, false);
            p.payloadDisplay = newPayloadDisp;
            p.Init(id, i, new Vector3(i + 1, 3, 0));
            p.OdomSubscribe("/position");
            //p.OdomSubscribe(string.Format("raptor{0}/odom", i + 1));
            ocuLogger.Logv(string.Format("Payload of id {0} added at {1}", id, p.realPosition));
            controllableUnits.Add(id, p);
        }
    }

    float formationProjScale = 1;
    float degreeOffset = 0;
    int operationalRobotCount = 5;  //TODO replace test value
    Vector2[] projectedPositions = new Vector2[10];
    public Transform projectionRend;
    void ProjectFormation()
    {
        if (Input.GetKey("w"))
            degreeOffset = (--degreeOffset < 0) ? (360 + degreeOffset) : degreeOffset;
        else if (Input.GetKey("q"))
            degreeOffset = (degreeOffset + 1) % 360;

        if (Input.GetKey("a"))
        {
            formationProjScale = (formationProjScale > 0.5f) ? (formationProjScale - 0.1f) : 0.5f;
        }
        else if (Input.GetKey("s"))
        {
            formationProjScale += 0.1f;
            Mathf.Clamp(formationProjScale, 0.5f, 5);
        }

        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Draw points evenly on perimeter of circle
        double d = 360.0 / operationalRobotCount;
        for (int n = 0; n < operationalRobotCount; n++)
        {
            double angle = Math.PI * (d * n + degreeOffset) / 180.0;

            float s = (float)Math.Sin(angle);
            float c = (float)Math.Cos(angle);

            // translate point back to origin:
            Vector3 p = new Vector2(mousePos2D.x, mousePos2D.y + formationProjScale);
            p.x -= mousePos2D.x;
            p.y -= mousePos2D.y;

            // rotate point
            float xnew = p.x * c - p.y * s;
            float ynew = p.x * s + p.y * c;

            // translate point back:
            p.x = xnew + mousePos2D.x;
            p.y = ynew + mousePos2D.y;
            projectionRend.GetChild(n).position = p;
            projectedPositions[n] = p;
        }
    }
    void ProjectPoint()
    {
        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //TODO: replace with own ghost
        projectionRend.GetChild(1).position = mousePos2D;
    }

    void Update()
    {
        //left mouse button as selector button to select or deselect at anytime
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
                //If no UI elements are selected- proceed to scene interaction
                Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                print("Position in world: " + mousePos2D.ToString());
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.collider != null)
                {
                    //SelectedUnitId = hit.collider.gameObject.GetComponent<Unit>().id;
                    SelectedUnit = hit.collider.gameObject.GetComponent<Unit>();
                }
                else 
                { 
                    //SelectedUnitId = null;
                    SelectedUnit = null;
                }
            }
        }


        if (SelectedUnit != null)
        {
            //movement controls
            if (UiManager.Instance.currentState == UiManager.State.ManualMovement)
            {
                //controllableUnits[SelectedUnitId].MoveForward(Input.GetAxis("Vertical") * curSpeed * Time.deltaTime);
                //controllableUnits[SelectedUnitId].Rotate(Input.GetAxis("Horizontal") * rotSpeed * Time.deltaTime * Vector3.back);
                SelectedUnit.MoveForward(Input.GetAxis("Vertical") * curSpeed * Time.deltaTime);
                SelectedUnit.Rotate(Input.GetAxis("Horizontal") * rotSpeed * Time.deltaTime * Vector3.back);
            }
            else
            {
                if (UiManager.Instance.currentState == UiManager.State.PointToFormation)
                {
                    ProjectFormation();
                    if (Input.GetMouseButtonDown(1))
                    {
                        for (int i = 0; i < operationalRobotCount; i++) //TODO: change operational robot count
                        {
                            StartCoroutine(MoveUnitToPositionCoroutine(controllableUnits["p" + i], projectedPositions[i]));
                            ocuLogger.Logv(string.Format("p{0} moving to point {1}", i, projectedPositions[i].ToString()));
                        }
                    }
                }
                else if (UiManager.Instance.currentState == UiManager.State.PointToPoint)
                { 
                    ProjectPoint();
                    if (Input.GetMouseButtonDown(1))
                    {
                        Vector2 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        StartCoroutine(MoveUnitToPositionCoroutine(SelectedUnit, mousePos2D));
                    }
                }
            }
        }

        // Bg Graphic updates
        GrahamScanBeacons();
    }
}

