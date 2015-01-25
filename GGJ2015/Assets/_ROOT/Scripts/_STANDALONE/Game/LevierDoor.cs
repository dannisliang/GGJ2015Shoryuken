using UnityEngine;
using System.Collections;

public class LevierDoor : MonoBehaviour {
	
	public bool inZone;

	public Door door;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(inZone)
		{
			if(Input.GetKeyDown(KeyCode.E))
			{
				door.Locked = !door.Locked;
			}
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player")
		{
			inZone = true;
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if(other.tag == "Player")
		{
			inZone = false;
		}
	}
}
