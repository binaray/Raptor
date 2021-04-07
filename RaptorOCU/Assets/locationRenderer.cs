using Microsoft.Maps.Unity;
using Microsoft.Maps.Unity.Search;
using Microsoft.Geospatial;
using UnityEngine;

[RequireComponent(typeof(MapRenderer))]
[RequireComponent(typeof(MapInteractionController))]
public class locationRenderer : MonoBehaviour
{
    public async void renderBingMap(float latitude, float longitude)
    {
        if (MapSession.Current == null || string.IsNullOrWhiteSpace(MapSession.Current.DeveloperKey)) return;

        var currentLatLon = new LatLonWrapper();
        currentLatLon.Latitude = latitude;
        currentLatLon.Longitude = longitude;

        var result = await MapLocationFinder.FindLocationsAt(currentLatLon.ToLatLon());

        if (result.Locations.Count > 0)
        {
            var location = result.Locations[0];
            var mapRenderer = GetComponent<MapRenderer>();
            mapRenderer.SetMapScene(new MapSceneOfLocationAndZoomLevel(location.Point, 20));
        }
    }

    public void transformMap(float xPos, float yPos, int direction) {
        var mapInteractionController = GetComponent<MapInteractionController>();

        switch (direction)
        {
            case 1: //north
                mapInteractionController.Pan(new Vector2(0, 0.5f), false);
                break;
            case 2: //south
                mapInteractionController.Pan(new Vector2(0, -0.5f), false);
                break;
            case 3: //east
                mapInteractionController.Pan(new Vector2(0.5f, 0), false);
                break;
            default: //west
                mapInteractionController.Pan(new Vector2(-0.5f, 0), false);
                break;
        }
        
        transform.position = new Vector3(xPos, yPos, 2);

    }
}
