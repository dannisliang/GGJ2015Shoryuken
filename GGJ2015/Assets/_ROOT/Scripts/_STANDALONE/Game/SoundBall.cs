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
			enemy.RevengePoint = Player.transform;
		}
	}
}
