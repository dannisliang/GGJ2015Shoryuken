using UnityEngine;
using System.Collections;
using Pathfinding;
using System.Collections.Generic;

public enum EnemyState
{
	Waiting,
	Alerted,
	Attack,
	Flee,
	Walking
}

public class EnemyAi : MonoBehaviour {
	

	static float EnemyAttackSpeed = 10;
	static float EnemyWalkSpeed = 6;
	static float EnemyAlertedSpeed = 8;
	static float EnemyFleeSpeed = 10;
	static float TimeWaited = 10;

	public EnemyState state;
	public GameObject target = new GameObject();
	public GameObject spawnPoint;

	public List<GameObject> trajectoryPoints = new List<GameObject>();

	private Seeker m_seeker;
	private AIPath m_path;

	public EnemyAi(GameObject targetObj, EnemyState stateAi, GameObject spawn, List<GameObject> pathPoints)
	{
		target = targetObj;
		state = stateAi;
		spawnPoint = spawn;
		trajectoryPoints = pathPoints;
	}


	// Use this for initialization
	void Start () {
		m_seeker = GetComponent<Seeker>();
		m_path = GetComponent<AIPath>();
		m_path.target = target.transform;
		m_seeker.StartPath (transform.position, transform.position+transform.forward*m_path.speed);
		spawnPoint = new GameObject();
		spawnPoint.transform.position = transform.position;
	}

	float time = 0;
	// Update is called once per frame
	void Update () {
		if(target.transform != m_path.target.transform)
		{
			m_path.target = target.transform;
			m_seeker.StartPath (transform.position, transform.position+transform.forward*m_path.speed);
		}

		switch(state)
		{
			case EnemyState.Attack:
			{
				m_path.speed = EnemyAttackSpeed;
				m_path.canMove = true;
			}
			break;
			case EnemyState.Waiting:
			{
				m_path.canMove = false;
				rigidbody.velocity = Vector3.zero;
				time += Time.deltaTime;
				if(time > TimeWaited){
					time = 0;
					state = EnemyState.Walking;
				}
			}
			break;
			case EnemyState.Alerted:
			{
				m_path.speed = EnemyAlertedSpeed;
				m_path.canMove = true;
				if(m_path.TargetReached)
					state = EnemyState.Waiting;
			}
			break;
			case EnemyState.Flee:
			{
				m_path.speed = EnemyFleeSpeed;
				m_path.canMove = true;
				if(m_path.TargetReached)
					state = EnemyState.Waiting;
			}
			break;
			case EnemyState.Walking:
			{
				m_path.speed = EnemyWalkSpeed;
				m_path.canMove = true;
				if(trajectoryPoints.Count > 0)
					break;
				if(target == null)
				{
					target.transform.position = trajectoryPoints[0].transform.position;
					m_seeker.StartPath (transform.position, transform.position+transform.forward*m_path.speed);
					break;
				}
				
				if(m_path.TargetReached){
					for(int i = 0; i<trajectoryPoints.Count; i++)
					{
						if(trajectoryPoints[i].transform.position == target.transform.position)
						{
							if(i-1 == trajectoryPoints.Count)
								i = 0;
							target.transform.position = trajectoryPoints[i].transform.position;
							m_seeker.StartPath (transform.position, transform.position+transform.forward*m_path.speed);
							break;
						}
							
					}
				}
			}
			break;
		}
	}



	public Seeker Seeker {
		get {
			return m_seeker;
		}
		set {
			m_seeker = value;
		}
	}

	public AIPath Path {
		get {
			return m_path;
		}
		set {
			m_path = value;
		}
	}
}
