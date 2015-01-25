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
	Walking,
	Revenge
}

public class EnemyAi : MonoBehaviour {

	static float EnemyAttackSpeed = 10;
	static float EnemyWalkSpeed = 6;
	static float EnemyAlertedSpeed = 8;
	static float EnemyFleeSpeed = 10;
	static float TimeWaited = 10;

	public EnemyState state;
	public Transform target;
	public GameObject spawnPoint;

	public Transform RevengePoint;

	public EnemyRoundPath roundPath;

	private Seeker m_seeker;
	private AIPath m_path;

	public EnemyAi(Transform targetObj, EnemyState stateAi, GameObject spawn)
	{
		target = targetObj;
		state = stateAi;
		spawnPoint = spawn;
	}


	// Use this for initialization
	void Start () {
		m_seeker = GetComponent<Seeker>();
		m_path = GetComponent<AIPath>();
		if(target != null)
			m_path.target = target;
		m_seeker.StartPath (transform.position, transform.position+transform.forward*m_path.speed);
		spawnPoint = new GameObject();
		spawnPoint.transform.position = transform.position;

		InvokeRepeating("PingPosition", 1, 1);
	}

	float time = 0;
	// Update is called once per frame
	void Update () {
		if(target != m_path.target)
		{
			m_path.target = target;
		}

		switch(state)
		{
			case EnemyState.Flee:
			{
				m_path.speed = EnemyFleeSpeed;
				if(m_path.TargetReached){
					state = EnemyState.Revenge;
					target = RevengePoint;
				}
			}
			break;
			case EnemyState.Attack:
			{
				m_path.speed = EnemyAttackSpeed;
			}
			break;
			case EnemyState.Waiting:
			{
				target = null;
				
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
				if(m_path.TargetReached)
					state = EnemyState.Waiting;
			}
			break;
			case EnemyState.Walking:
			{
				m_path.speed = EnemyWalkSpeed;
				if(roundPath == null || roundPath.PathPoints.Length <= 0)
					break;
				if(target == null)
				{
					target = roundPath.PathPoints[0];
					break;
				}
				
				if(m_path.TargetReached){
					for(int i = 0; i<roundPath.PathPoints.Length; i++)
					{
						if(roundPath.PathPoints[i].position == target.position && Vector3.Distance(target.position, transform.position) < 5)
						{
							if(i == roundPath.PathPoints.Length-1 || roundPath.PathPoints.Length <= 1)
								i = -1;
							Debug.Log(i);
							target = roundPath.PathPoints[i+1];
							break;
						}
					}
				}
			}
			break;
			case EnemyState.Revenge:
			{
				m_path.speed = EnemyAlertedSpeed;
				if(m_path.TargetReached && Vector3.Distance(transform.position, target.position) < 5){
					state = EnemyState.Waiting;
				}
			}
			break;
		}
	}

	private void PingPosition()
	{
		PingMonsterPosition ping = PingMonsterPosition.Create();
		ping.Position = transform.position;
		ping.Send();
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

	public void OnCollisionEnter(Collision other)
	{
		if(other.transform.tag == "Player")
		{
			GameManager.Instance.RevivePlayer();
			GameManager.Instance.PlayerInstance.GetComponentInChildren<SoundRifle>().munitions = 3;
			state = EnemyState.Waiting;
		}
	}
}
