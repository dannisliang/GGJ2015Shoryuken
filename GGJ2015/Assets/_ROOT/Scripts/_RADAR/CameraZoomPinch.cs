using UnityEngine;
using System.Collections;

public class CameraZoomPinch : MonoBehaviour
{
    private float speed = 0.1f;
    private float minPinchSpeed = 100.0F;
    private float varianceInDistances = 0.0f;
    private float minDistDelta = 1.0f;
    private float touchDelta = 0.0F;
    
    private Vector2 prevDist = new Vector2(0, 0);
    private Vector2 curDist = new Vector2(0, 0);
    private float speedTouch0 = 0.0F;
    private float speedTouch1 = 0.0F;

    
    void Update()
    {

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 deltaPos = Input.GetTouch(0).deltaPosition;

            RadarManager.Instance.MoveMap( deltaPos );
        }

        else if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(1).phase == TouchPhase.Moved)
        {

            curDist = Input.GetTouch(0).position - Input.GetTouch(1).position; //current distance between finger touches
            prevDist = ((Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition) - (Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition)); //difference in previous locations using delta positions
            touchDelta = curDist.magnitude - prevDist.magnitude;
            speedTouch0 = Input.GetTouch(0).deltaPosition.magnitude / Input.GetTouch(0).deltaTime;
            speedTouch1 = Input.GetTouch(1).deltaPosition.magnitude / Input.GetTouch(1).deltaTime;

            
            if (Mathf.Abs(touchDelta) < minDistDelta)
                return;
            
            //Debug.Log("curDist : " + curDist + " - touchDelta : " + touchDelta + "speedTouch : " + speedTouch0);

            if ((touchDelta + varianceInDistances <= 0) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
                RadarManager.Instance.ZoomMap( speed );

            if ((touchDelta + varianceInDistances > 0) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
                RadarManager.Instance.ZoomMap( -speed );

        }
    }

}