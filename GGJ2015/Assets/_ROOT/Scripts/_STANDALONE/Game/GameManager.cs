using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public static GameManager Instance;

    public GameObject PlayerPrefab;
    private GameObject m_lastCheckpoint;

    private GameObject m_playerInstance;
    public GameObject PlayerInstance
    {
        get
        {
            return m_playerInstance;
        }
        set
        {
            m_playerInstance = value;
        }
    }

	// Use this for initialization
	void Start () {
        Instance = this;
        SpawnPlayer();
	}

    void SpawnPlayer()
    {
        GameObject startingPoint = FindObjectOfType<StartingPoint>().gameObject;
        m_playerInstance = Object.Instantiate(PlayerPrefab, startingPoint.transform.position, Quaternion.identity) as GameObject;
        m_lastCheckpoint = startingPoint;
    }

	// Update is called once per frame
    void Update()
    {
        Screen.lockCursor = true;
        Screen.showCursor = false;
	}

    public void SetCheckpoint(GameObject newCheckpoint)
    {
        m_lastCheckpoint = newCheckpoint;
    }

    public void RevivePlayer()
    {
        m_playerInstance.transform.position = m_lastCheckpoint.transform.position;
    }
}
