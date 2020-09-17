using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridlineRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector2 lowerBound = Camera.main.ScreenToWorldPoint(Vector2.zero);
        Vector2 upperBound = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        print(string.Format("Lower bound: {0} Upper bound: {1}", lowerBound.ToString(), upperBound.ToString()));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
