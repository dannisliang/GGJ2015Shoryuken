using UnityEngine;
using System.Collections;

public class SoundBall : MonoBehaviour {

	public GameObject Player;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Enemy")
		{
			EnemyAi enemy = other.GetComponent<EnemyAi>();
			enemy.state = EnemyState.Flee;
			GameObject go = new GameObject();
			go.transform.position = Player.transform.position;
			enemy.RevengePoint = go.transform;
			int rand = Random.Range(1,5);
			MasterAudio.PlaySound3DFollowTransformAndForget( "SFX_Enemy_Fear_0"+rand, other.transform );
		}
	}
}
