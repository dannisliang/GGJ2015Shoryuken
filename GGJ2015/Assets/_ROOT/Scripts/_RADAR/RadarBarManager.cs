using UnityEngine;
using System.Collections;

public class RadarBarManager : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<RadarBlipManager>().Trigger();
    }
}
