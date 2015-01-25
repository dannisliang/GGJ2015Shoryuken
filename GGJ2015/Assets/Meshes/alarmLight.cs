using UnityEngine;
using System.Collections;

public class alarmLight : MonoBehaviour {

	float speed = 4;
	Light myLight;

	void Start(){
		myLight = GetComponent<Light> ();
		Play ();
	}

	public void Play(){
		StartCoroutine ("Rotate");
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
