using UnityEngine;
using System.Collections.Generic;

public class RadarManager : MonoBehaviour {

    public static RadarManager Instance;
    private bool m_isScanning = true;

    public GameObject m_radar;
    public GameObject m_radarBar;
    public GameObject m_radarBlip;

    private GameObject m_barGO;
    private Vector2 m_playerPos;
    private List<GameObject> m_targets = new List<GameObject>();
    private GameObject m_openedMenu;
	// Use this for initialization
	
    void Awake()
    {
        Instance = this;
    }
    
    void Start () {

        float offsetZ = -2.0f;
        //int size = 15;
        //for (int i = 0; i < size; i++)
        //{            
        //    Vector3 pos = new Vector3(Random.Range(0, 450), Random.Range(0, 250), offsetZ);
        //    ShowPing(pos);            
        //}

        SpawnRadarBar(offsetZ);
	}

    public void SetOpenedMenu(GameObject newMenu)
    {
        if (newMenu == null && m_openedMenu != null)
        {
            Destroy(m_openedMenu);
            RadarManager.Instance.ToggleScan(true);
        }

        m_openedMenu = newMenu;
    }

    private void SpawnRadarBar(float offsetZ)
    {
        m_playerPos = Vector2.zero;
        m_barGO = Object.Instantiate(m_radarBar) as GameObject;
        m_barGO.transform.SetParent(m_radar.transform, false);
        m_barGO.transform.localPosition = new Vector3(m_playerPos.x, m_playerPos.y, offsetZ);
        m_barGO.transform.rotation = Quaternion.identity;
    }

    public void ToggleScan(bool isScanning)
    {
        m_isScanning = isScanning;

        if(m_isScanning == true)
        {
            m_barGO.SetActive(true);
        }
        else
        {
            m_barGO.SetActive(false);
        }
    }

	// Update is called once per frame
	void FixedUpdate () {
        if (m_barGO == null || !m_isScanning)
            return;

        m_barGO.transform.Rotate(Vector3.forward, -1.25f);
        float barAngle = m_barGO.transform.rotation.eulerAngles.z;
    }

    public void Parent(GameObject go)
    {
        go.transform.SetParent(m_radar.transform, false);
    }

    public void ShowPing(Vector3 worldPosition)
    {

        //TODO : convert worldPosition to radar position
        Vector3 pos = worldPosition;

        GameObject go = Object.Instantiate( m_radarBlip, pos, Quaternion.identity ) as GameObject;
        m_targets.Add( go );

    }

}
