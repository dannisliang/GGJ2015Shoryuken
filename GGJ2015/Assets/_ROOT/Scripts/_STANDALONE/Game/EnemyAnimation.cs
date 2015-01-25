using UnityEngine;
using System.Collections;

public class EnemyAnimation : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Animation>()["Take 001"].speed = 10;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
