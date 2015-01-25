using UnityEngine;
using System.Collections;

public class AlarmLight : MonoBehaviour {

	float speed = 4;
	Light myLight;

	void Start(){
		myLight = GetComponent<Light> ();
	}

	public void Play(){
		StartCoroutine ("Rotate");
        Light light = GetComponent<Light>();
        light.enabled = true;
	}

	public void Stop(){
		StopCoroutine ("Rotate");
	}

	IEnumerator Rotate(){
		while (true) {
			myLight.transform.Rotate(Vector3.up,speed);	
			yield return null;
		}
	}

}
