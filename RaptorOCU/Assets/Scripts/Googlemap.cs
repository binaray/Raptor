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
    public int zoom = 20;
    public MapType mapType;
    //public int width = 800;
    //public int height = 600;
    public bool doubleResolution = false;
    private bool loaded;

    private int canvasWidth, canvasHeight;

    void Start()
    {
        //centerLocation = 
        //var rectTransform = UiManager.Instance.GetComponent<RectTransform>();
        canvasWidth = (int)mapTemplate.GetComponent<RectTransform>().rect.width/2;
        canvasHeight = (int)mapTemplate.GetComponent<RectTransform>().rect.height/2;
        //getZoom();
        loaded = false;

        if (loadOnStart) Refresh();
    }

    public void Refresh()
    {
        StartCoroutine(_Refresh());
    }

    IEnumerator _Refresh()
    {
        if (!loaded)
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
                loaded = true;
            }
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
        double meterPerPixel = WorldScaler.worldScale*mapTemplate.GetComponent<RectTransform>().localScale.x/2;
        double zoomCalc = Math.Log(156543.03392 * Math.Cos(centerLocation.latitude * Math.PI / 180) / meterPerPixel) / Math.Log(2);
        zoom = 20;//(int)zoomCalc<=20? (int)zoomCalc: 20;
        Debug.Log(string.Format("Zoom calc: {0}, Zoom: {1}",zoomCalc,zoom));
    }

    public double makeRad(double x) {
        return x * Math.PI / 180;
    }

    public Vector3 getWorldPositionUsingLatLon(double otherLat, double otherLon) {
        Vector3 distanceFromCenter = calcDistanceFromCentreLatLon(otherLat, otherLon);
        Vector3 worldPosition = new Vector3(distanceFromCenter.x,distanceFromCenter.y,0);
        return worldPosition * WorldScaler.worldScale;
    }

    public Vector3 calcDistanceFromCentreLatLon(double otherLat, double otherLon) {
        return calcDistanceBtw2LatLon(centerLocation.latitude, centerLocation.longitude, otherLat, otherLon);
    }

    //Haversine formula
    public Vector3 calcDistanceBtw2LatLon(double lat1, double lon1, double lat2, double lon2) {     
        int R = 6378137; // Earth’s mean radius in meter
        double latDiffMeters = lat2 - lat1;
        double dLat = makeRad(latDiffMeters);
        double dLong = makeRad(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(makeRad(lat1)) * Math.Cos(makeRad(lat2)) *
            Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double d = R * c;
        double horizontalDist = Math.Sqrt(Math.Pow(d, 2) - Math.Pow(latDiffMeters, 2));
        return new Vector3((float)horizontalDist, (float)latDiffMeters,(float)d); // returns the distance in meter
    }
}

[System.Serializable]
public class GoogleMapLocation
{
    public string address;
    public double latitude = 1.34008060106968;
    public double longitude = 103.964607766973;
}

