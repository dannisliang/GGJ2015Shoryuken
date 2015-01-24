using UnityEngine;
using System.Collections;

public class EnemyRoundPath : MonoBehaviour {
	
	public Transform[] PathPoints;

	// Use this for initialization
	void Start () {
		Transform[] TempPathPoints = GetComponentsInChildren<Transform>();
		PathPoints = Transform[TempPathPoints.Length-1];
		for(int i = 0; i<TempPathPoints.Length;i++)
		{
			if(i = 0)
				continue;
			PathPoints[i-1] = TempPathPoints[i];
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
