using Microsoft.Maps.Unity;
using UnityEngine;


[RequireComponent(typeof(MapRenderer))]
public class orthoCameraMapDimensionSynchronizer : MonoBehaviour
{
    private Camera _camera = null;
    private MapRenderer _mapRenderer = null;
    private Renderer _renderer = null;
    /*
    private void Awake()
    {
        _camera = Camera.main;
        _mapRenderer = GetComponent<MapRenderer>();
    }*/

    private void Start() {
        _camera = Camera.main;
        _mapRenderer = GetComponent<MapRenderer>();
       
        //this is just random vals
        LatLonWrapper currentLatLon = new LatLonWrapper();
        currentLatLon.Latitude = (double)1.3;
        currentLatLon.Longitude = (double)103.79f;
        _mapRenderer.SetMapScene(new MapSceneOfLocationAndZoomLevel(currentLatLon.ToLatLon(), 20));

        var cameraOrthoSize = _camera.orthographicSize;
        var cameraOrthoHeight = 2 * cameraOrthoSize;
        var cameraOrthoWidth = cameraOrthoHeight * Screen.width / Screen.height;

        transform.localScale = new Vector3(cameraOrthoWidth, 1, cameraOrthoHeight);
        /*
         _renderer = GetComponent<Renderer>();

        _renderer.sortingLayerName = "Background";
        _renderer.sortingOrder = 0;*/

    }

    //the local scale for this is not good enough for the mini camera 
    private void Update()
    {
        var cameraOrthoSize = _camera.orthographicSize;
        var cameraOrthoHeight = 2 * cameraOrthoSize;
        var cameraOrthoWidth = cameraOrthoHeight * Screen.width / Screen.height;

        transform.localScale = new Vector3( cameraOrthoWidth,  1, cameraOrthoHeight);
    }



}