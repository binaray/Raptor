// modified from https://github.com/Axel-P/StaticGoogleMapsUnity/blob/master/Assets/Scripts/GoogleMap.cs

/*
 * url format
    https://maps.googleapis.com/maps/api/staticmap?center=Brooklyn+Bridge,New+York,NY&zoom=13&size=600x300&maptype=roadmap
    &markers=color:blue%7Clabel:S%7C40.702147,-74.015794&markers=color:green%7Clabel:G%7C40.711614,-74.012318
    &markers=color:red%7Clabel:C%7C40.718217,-73.998284
    &key=YOUR_API_KEY
 */

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public class Googlemap : MonoBehaviour
{
    [SerializeField]
    private GameObject mapTemplate;

    public enum MapType
    {
        RoadMap,
        Satellite
    }

    public string GoogleApiKey;
    public bool loadOnStart = true;
    public GoogleMapLocation centerLocation;
    public int zoom = 13;
    public MapType mapType;
    //public int width = 800;
    //public int height = 600;
    public bool doubleResolution = false;

    private int canvasWidth, canvasHeight;

    void Start()
    {
        //var rectTransform = UiManager.Instance.GetComponent<RectTransform>();
        canvasWidth = (int)mapTemplate.GetComponent<RectTransform>().rect.width;
        canvasHeight = (int)mapTemplate.GetComponent<RectTransform>().rect.height;
        if (loadOnStart) Refresh();
        getZoom();
    }

    public void Update()
    {
        //Debug.Log(string.Format("canvas width: {0}, canvas height: {1}", canvasWidth, canvasHeight));
    }

    public void Refresh()
    {
        StartCoroutine(_Refresh());
    }

    IEnumerator _Refresh()
    {
        string url = "https://maps.googleapis.com/maps/api/staticmap";
        string qs = "";

        if (centerLocation.address != "")
        {
            qs += "center=" + UnityWebRequest.UnEscapeURL(centerLocation.address);
        }
        else
        {
            qs += "center=" + UnityWebRequest.UnEscapeURL(string.Format("{0},{1}", centerLocation.latitude, centerLocation.longitude));
        }

        qs += "&zoom=" + zoom.ToString();
        //Debug.Log(string.Format("canvas w {0} h {1}",canvasWidth,canvasHeight));
        qs += "&size=" + UnityWebRequest.UnEscapeURL(string.Format("{0}x{1}", canvasWidth, canvasHeight));
        qs += "&scale=" + (doubleResolution ? "2" : "1");
        qs += "&maptype=" + mapType.ToString().ToLower();

        qs += "&sensor=false";

        qs += "&key=" + UnityWebRequest.UnEscapeURL(GoogleApiKey);
        string requestUrl = url + "?" + qs;
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(requestUrl);
        Debug.Log(requestUrl);

        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            //mapTemplate.GetComponent<SpriteRenderer>().material.mainTexture = texture;

            //mapTemplate.GetComponent<SpriteRenderer>().material.shader = Shader.Find("Sprites/Default");
            mapTemplate.GetComponent<RawImage>().texture = texture;
        }
    }

    public void updateLatLong(double latitude, double longitude)
    {
        centerLocation.latitude = latitude;
        centerLocation.longitude = longitude;
        getZoom();
    }

    public void getZoom() {
        //double metersPerPixel = 156543.03392 * Math.Cos(centerLocation.latitude * Math.PI / 180) / Math.Pow(2, zoom);
        double meterPerPixel = WorldScaler.worldScale;
        double zoomCalc = Math.Log(156543.03392 * Math.Cos(centerLocation.latitude * Math.PI / 180) / meterPerPixel) / Math.Log(2); 
        zoom = (int)zoomCalc;
        Debug.Log(string.Format("Zoom calc: {0}, Zoom: {1}",zoomCalc,zoom));
    }

}

[System.Serializable]
public class GoogleMapLocation
{
    public string address;
    public double latitude;
    public double longitude;
}

