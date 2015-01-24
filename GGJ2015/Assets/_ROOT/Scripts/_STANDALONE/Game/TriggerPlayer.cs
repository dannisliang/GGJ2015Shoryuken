using UnityEngine;
using System.Collections;

public class TriggerPlayer : MonoBehaviour {

	public GameObject action;

	public void OnTriggerEnter(Collider other)
	{
		if(other.name == "Player")
			action.SendMessage("TriggerAction", other.gameObject, SendMessageOptions.DontRequireReceiver);
	}
}
