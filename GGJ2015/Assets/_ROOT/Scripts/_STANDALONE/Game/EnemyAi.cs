using UnityEngine;
using System.Collections;
using Pathfinding;

public class EnemyAi : MonoBehaviour {

	public float speed = 10;

	// Use this for initialization
	void Start () {
		Seeker seeker = GetComponent<Seeker>();
		seeker.StartPath (transform.position, transform.position+transform.forward*speed, OnPathComplete);
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt(GameObject.Find("Player").transform.position);
	}

	public void OnPathComplete (Path p) {
		//We got our path back
		if (p.error) {
			//Nooo, a valid path couldn't be found
		} else {
			//Yey, now we can get a Vector3 representation of the path
			//from p.vectorPath
		}
	}
}
