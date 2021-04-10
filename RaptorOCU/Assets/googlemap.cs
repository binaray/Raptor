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

public class googlemap : MonoBehaviour
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
    public int width = 800;
    public int height = 600;
    public bool doubleResolution = false;

    private int canvasWidth, canvasHeight;

    void Start()
    {
        //var rectTransform = UiManager.Instance.GetComponent<RectTransform>();
        canvasWidth = width;
        canvasHeight = height;
        if (loadOnStart) Refresh();
    }

    public void Update()
    {
        //CameraPan camerapan = Camera.main.GetComponent<CameraPan>();
        //float cameraOffset = camerapan.Frameoffset;
        //Vector2 minframe = Camera.main.WorldToScreenPoint(new Vector2(camerapan.MinFrameBound.x - cameraOffset * 2, camerapan.MinFrameBound.y - cameraOffset * 2));
        //Vector2 maxframe = Camera.main.WorldToScreenPoint(new Vector2(camerapan.MaxFrameBound.x + cameraOffset * 2, camerapan.MaxFrameBound.y + cameraOffset * 2));

        //Vector2 minframe = new Vector2(camerapan.MinFrameBound.x - cameraOffset, camerapan.MinFrameBound.y - cameraOffset);
        //Vector2 maxframe = new Vector2(camerapan.MaxFrameBound.x + cameraOffset, camerapan.MaxFrameBound.y + cameraOffset);
        //var rectTransform = UiManager.Instance.GetComponent<RectTransform>();

        //canvasWidth = (int)(maxframe.x - minframe.x);
        //canvasHeight = (int)(maxframe.y - minframe.y);

        //RectTransform mapRectTransform = mapTemplate.GetComponent<RectTransform>();
        //mapRectTransform.sizeDelta = new Vector2(canvasWidth*100, canvasHeight*100);
        //mapTemplate.transform.localScale = new Vector2(canvasWidth, canvasHeight);
        //mapTemplate.transform.position = new Vector2(canvasWidth, canvasHeight);
        //mapRectTransform.anchoredPosition3D = new Vector2(minframe.x ,minframe.y+cameraOffset/2);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(mapRectTransform);
        //Debug.Log(string.Format("min frame: x {0}, y {1}", minframe.x, minframe.y));
        //Debug.Log(string.Format("max frame: x {0}, y {1}", maxframe.x, maxframe.y));
        Debug.Log(string.Format("canvas width: {0}, canvas height: {1}", canvasWidth, canvasHeight));
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

}

[System.Serializable]
public class GoogleMapLocation
{
    public string address;
    public float latitude;
    public float longitude;
}

