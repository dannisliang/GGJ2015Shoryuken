using UnityEngine;
using System.Collections;

public class SoundPlayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
		MasterAudio.PlaySound3DFollowTransform( "SFX_Amb_SpaceShip_V2", transform);
	}

	Vector3 oldPosition = Vector3.zero;
	// Update is called once per frame
	float time = 0;
	void Update () {
		time += Time.deltaTime;
		if(oldPosition != transform.position && transform.position.y < 1){
			int rand = Random.Range(1,7);
			if(time >= 0.4f){
				MasterAudio.PlaySound3DFollowTransformAndForget("SFX_Foley_Footsteps_Metal_0"+rand, transform);
				time = 0;
			}
		}
		oldPosition = transform.position;
	}
}
