using UnityEngine;
using System.Collections;

public class RadarBarManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<RadarBlipManager>().Trigger();
    }
}
