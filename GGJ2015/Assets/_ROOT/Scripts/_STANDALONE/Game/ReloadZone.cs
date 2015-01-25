using UnityEngine;
using System.Collections;

public class ReloadZone : MonoBehaviour {
	
	public bool inZone;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(inZone)
		{
			if(Input.GetKeyDown(KeyCode.E))
			{
				GameManager.Instance.PlayerInstance.GetComponentInChildren<SoundRifle>().munitions = 3;
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
