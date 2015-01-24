using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class RadarMap : MonoBehaviour, IScrollHandler {

    public float zoomSpeed = 0.1f;

    public void OnScroll( PointerEventData eventData )
    {
        RadarManager.Instance.ZoomMap( eventData.scrollDelta.y * zoomSpeed );
    }
}
