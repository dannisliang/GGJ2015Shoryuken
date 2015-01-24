using UnityEngine;
using System.Collections;

public class SpawnEnemy : MonoBehaviour {

	public int numberToSpawn;
	public GameObject enemyPrefab;
	public EnemyState stateSpawn;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void TriggerAction(GameObject collider)
	{
		for(int i = 0; i<numberToSpawn; i++)
		{
			GameObject obj = (GameObject)GameObject.Instantiate(enemyPrefab, transform.position, Quaternion.identity);
			EnemyAi enemy  = obj.GetComponent<EnemyAi>();
			enemy.state = stateSpawn;
			enemy.target = collider;
			enemy.spawnPoint = gameObject;
		}
	}
}
