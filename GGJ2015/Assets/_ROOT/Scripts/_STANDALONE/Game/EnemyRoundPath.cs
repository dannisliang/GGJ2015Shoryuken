using UnityEngine;
using System.Collections;

public class EnemyRoundPath : MonoBehaviour {
	
	public Transform[] PathPoints;

	// Use this for initialization
	void Start () {
		PathPoints = GetComponentsInChildren<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
