using UnityEngine;
using System.Collections;

public class PlayerPositionPing : MonoBehaviour {

	
	IEnumerator Start () {
        yield return new WaitForSeconds( 0.5f );

        SetPlayerPosition ping = SetPlayerPosition.Create();
        ping.WorldPosition = transform.position;
        ping.Send();
	
	}
	
}
