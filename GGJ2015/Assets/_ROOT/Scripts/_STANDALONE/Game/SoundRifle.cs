using UnityEngine;
using System.Collections;

public class SoundRifle : MonoBehaviour {

	public CapsuleCollider collider;
	public int munitions;


	// Use this for initialization
	void Start () {
		collider.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButton(0))
		{
			if(munitions > 0){
				munitions--;
				collider.enabled = true;
				//Animations
			}
		}
	}
}
