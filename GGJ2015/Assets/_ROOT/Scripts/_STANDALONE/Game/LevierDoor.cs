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
<<<<<<< HEAD
			if(Input.GetKeyDown(KeyCode.E))
			{
				door.Locked = !door.Locked;
				if(!door.Locked)
					MasterAudio.PlaySound3DAtTransform( "SFX_World_Hack_Access_Granted", transform);
			}
=======
			door.Locked = !door.Locked;
            InteracSupp.SendMessage("LevierTriggered", SendMessageOptions.DontRequireReceiver);

>>>>>>> 3f65c90ce53ce27cfc296dfdbd433d9fc8b37fed
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
