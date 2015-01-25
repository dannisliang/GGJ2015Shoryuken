using UnityEngine;
using System.Collections;

public class SoundRifle : MonoBehaviour {

	public CapsuleCollider collider;
	public int munitions;
	public float coolDown;
	public bool inCoolDown = false;

	private bool clicked;

	// Use this for initialization
	void Start () {
		collider.enabled = false;
	}

	float time = 0;
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButton(0) && !inCoolDown && !clicked)
		{
			clicked = true;
			if(munitions > 0){
				munitions--;
				collider.enabled = true;
				inCoolDown = true;
				//Animations
				gameObject.GetComponent<Animation>()["Take 001"].speed = 2;
				gameObject.GetComponent<Animation>().Play("Take 001");
			}
		}

		if(inCoolDown)
		{
			time += Time.deltaTime;
			if(time > 1)
				collider.enabled = false;
			if(time >= coolDown)
			{
				time = 0;
				inCoolDown = false;
			}
		}

		if(clicked){
			if(Input.GetMouseButtonUp(0))
			{
				clicked = false;
			}
		}
	}
}
