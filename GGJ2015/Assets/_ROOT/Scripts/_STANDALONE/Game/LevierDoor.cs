using UnityEngine;
using System.Collections;

public class LevierDoor : MonoBehaviour {
	
	private bool inZone;

	public Door door;
    public GameObject InteracSupp;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(inZone && Input.GetKeyDown(KeyCode.E))
		{
			door.Locked = !door.Locked;
            InteracSupp.SendMessage("LevierTriggered", SendMessageOptions.DontRequireReceiver);

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
