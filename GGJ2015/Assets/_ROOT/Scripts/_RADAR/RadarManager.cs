using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class RadarManager : MonoBehaviour {

    public static RadarManager Instance;

    [SerializeField]
    private Image _mapImage;//reference to the Image component that displays the map, set in the inspector

    //Prefabs    
    public GameObject radarBarPrefab;
    public GameObject radarBlipPrefab;

    //Factors for world to radar position conversions
    public float factorX = 1;
    public float factorZ = 1;

    private bool m_isScanning = true;

    private GameObject m_barGO;
    private Vector2 m_playerPos;
    private List<GameObject> m_targets = new List<GameObject>();
    private GameObject m_openedMenu;

    private const float offsetZ = -2f;

    private Canvas _canvas;

    /// <summary>
    /// Converts a world position to a local position relative to the Map Image
    /// </summary>
    Vector3 WorldToRadar( Vector3 world )
    {        
        return new Vector3(world.x * factorX, world.z * factorZ, offsetZ);
    }

    void Awake()
    {
        _canvas = GetComponent<Canvas>();

        Instance = this;
    }
    
    void Start () 
    {
        SpawnRadarBar(offsetZ);
	}

    public void SetOpenedMenu(GameObject newMenu)
    {
        if (newMenu == null && m_openedMenu != null)
        {
            Destroy(m_openedMenu);
            ToggleScan(true);
        }

        m_openedMenu = newMenu;
    }

    private void SpawnRadarBar(float offsetZ)
    {
        m_playerPos = Vector2.zero;
        m_barGO = Instantiate(radarBarPrefab) as GameObject;
        m_barGO.transform.SetParent(_mapImage.transform, false);
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
	
	void FixedUpdate () {
        if (m_barGO == null || !m_isScanning)
            return;

        m_barGO.transform.Rotate(Vector3.forward, -1.25f);
        float barAngle = m_barGO.transform.rotation.eulerAngles.z;
    }

    public void Parent(GameObject go)
    {
        go.transform.SetParent(_canvas.transform, false);
    }

    public void ShowPing(Vector3 worldPosition)
    {

        Vector3 pos = WorldToRadar(worldPosition);

        GameObject go = Instantiate( radarBlipPrefab, pos, Quaternion.identity ) as GameObject;
        m_targets.Add( go );

    }

    public void MoveMap( Vector2 delta )
    {
        MoveMap( new Vector3( delta.x, delta.y, 0 ) );
    }

    public void MoveMap( Vector3 delta )
    {
        _mapImage.transform.localPosition += delta;
    }

    public void ZoomMap(float delta)
    {
        _mapImage.transform.localScale += delta * Vector3.one;
    }
    
}
