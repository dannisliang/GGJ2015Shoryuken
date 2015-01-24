using UnityEngine;
using System.Collections;
using Pathfinding;

public enum EnemyState
{
	Waiting,
	Alerted,
	Attack,
	Flee,
	Walking
}

public class EnemyAi : MonoBehaviour {
	

	static float EnemyAttackSpeed = 100;
	static float EnemyWalkSpeed = 25;
	static float EnemyAlertedSpeed = 75;
	static float EnemyFleeSpeed = 100;

	public EnemyState state;
	public GameObject target = new GameObject();
	public GameObject spawnPoint;


	private Seeker m_seeker;
	private AIPath m_path;

	public EnemyAi(GameObject targetObj, EnemyState stateAi, GameObject spawn)
	{
		target = targetObj;
		state = stateAi;
		spawnPoint = spawn;
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
	
	// Update is called once per frame
	void Update () {
		if(target.GetInstanceID() != m_path.GetInstanceID())
		{
			m_path.target = target.transform;
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
