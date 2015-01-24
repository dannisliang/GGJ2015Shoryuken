using UnityEngine;
using System.Collections;

public class TriggerEnemy : MonoBehaviour {

	public EnemyAi enemy;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player" && enemy.state != EnemyState.Flee){
			enemy.state = EnemyState.Attack;
			enemy.target = other.transform;
		}
	}
}
