using UnityEngine;
using System.Collections;

public enum AlarmType
{
	Repulsive,
	Attractive
}

public class Alarm : MonoBehaviour {

	public bool m_isActive;
	
	public AlarmType m_type;
	public SphereCollider collider;
	public  float m_radiusMax;
	public float m_maxTime;

	// Use this for initialization
	void Start () {
	
	}

	float time = 0;
	// Update is called once per frame
	void Update () {
		if(IsActive)
		{
			if(collider.radius < m_radiusMax)
				collider.radius += Time.deltaTime*10;
			else
				collider.radius = 0;

			time += Time.deltaTime;
			if(time >= m_maxTime)
			{
				time = 0;
				IsActive = false;
			}
		}
		else
		{
			collider.radius = 0;
		}


	}

	public void OnTriggerEnter(Collider other)
	{
		if(IsActive){
			if(other.tag == "Enemy")
			{
				EnemyAi enemy = other.GetComponent<EnemyAi>();
				if(m_type == AlarmType.Attractive)
				{
					enemy.target = transform;
					enemy.state = EnemyState.Alerted;
				}
				else if(m_type == AlarmType.Repulsive)
				{
					enemy.target = enemy.spawnPoint.transform;
					enemy.state = EnemyState.Flee;
					enemy.RevengePoint = transform;
				}
			}
		}
	}

	public AlarmType Type {
		get {
			return m_type;
		}
		set {
			m_type = value;
		}
	}

	public float RadiusMax {
		get {
			return m_radiusMax;
		}
		set {
			m_radiusMax = value;
		}
	}

	public bool IsActive {
		get {
			return m_isActive;
		}
		set {
			m_isActive = value;
		}
	}
}
