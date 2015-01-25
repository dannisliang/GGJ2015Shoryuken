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

    //Factors for world to radar position conversions
    private float worldToLocalScaleFactorX = 10f;
    private float worldToLocalScaleFactorZ = 10f;

    private float worldToLocalScaleOffsetX = 33f;  // + ->
    private float worldToLocalScaleOffsetZ = -49f; // + /\

    private float m_iconScale = 2.0f;

    private bool m_isScanning = true;

    private GameObject m_barGO;
    private Vector2 m_playerPos;    
    private GameObject m_openedMenu;

    private float _minScale = 1;
    private float _maxScale = 3;
    private float _scaleMaxRatio = 3;
    private const float offsetZ = -2f;

    private Dictionary<Type, GameObject> _interactableIconsPrefabs = new Dictionary<Type, GameObject>();

    private Canvas _canvas;
    private ScrollRect _scrollRect;
    private CanvasScaler _canvasScaler;

    /// <summary>
    /// Converts a world position to a local position relative to the Map Image
    /// </summary>
    Vector3 WorldToRadar( Vector3 world )
    {
        Debug.Log(worldToLocalScaleFactorX + " " + worldToLocalScaleFactorZ);
        return new Vector3(((world.x + worldToLocalScaleOffsetX) * worldToLocalScaleFactorX), ((world.z + worldToLocalScaleOffsetZ) * worldToLocalScaleFactorZ), 0);
    }

    void Awake()
    {
        //cache components
        _canvas = GetComponent<Canvas>();
        _scrollRect = GetComponentInChildren<ScrollRect>();
        _canvasScaler = GetComponent<CanvasScaler>();

        //setup Map's Image component
        _mapImage.SetNativeSize();        
        _minScale = _canvasScaler.referenceResolution.y / _mapImage.rectTransform.sizeDelta.y;
        _maxScale = _minScale * _scaleMaxRatio;
        _mapImage.rectTransform.localScale = _minScale * Vector3.one;

        //Map interactable icons types
        _interactableIconsPrefabs.Add( Type.GetType( "Door" ), Resources.Load("DoorIcon") as GameObject );
        _interactableIconsPrefabs.Add( Type.GetType( "DoorHackPoint" ), Resources.Load( "DoorHackPointIcon" ) as GameObject );
        _interactableIconsPrefabs.Add( Type.GetType( "LevierDoor" ), Resources.Load("LeverIcon") as GameObject);

        //set Singleton reference
        Instance = this;
    }
    
    void Start () 
    {
        SpawnRadarBar();
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

    private void SpawnRadarBar()
    {
        m_playerPos = Vector2.zero;
        m_barGO = Instantiate(radarBarPrefab) as GameObject;
        m_barGO.transform.SetParent(_mapImage.transform, false);
        m_barGO.transform.localPosition = new Vector3(0, 0, 0);
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

    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0) // forward
        {
            ZoomMap(Input.GetAxis("Mouse ScrollWheel"));
        }
    }

	void FixedUpdate () {
        if (m_barGO == null || !m_isScanning)
            return;

        m_barGO.transform.Rotate(Vector3.forward, -1.25f);
    }

    public void Parent(GameObject go)
    {
        go.transform.SetParent(_canvas.transform, false);
    }

    public void ShowPing(Vector3 worldPosition)
    {
        Vector3 pos = WorldToRadar(worldPosition);

        GameObject go = Kathulhu.PoolsManager.Instance.Spawn( "RadarBlip" );
        go.transform.SetParent( _mapImage.transform, false );
        go.transform.localPosition = pos;
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
        //float scale = Mathf.Max( _mapImage.transform.localScale.y + delta, _minScale);
        float scale = Mathf.Clamp(_mapImage.transform.localScale.y + delta, _minScale, _maxScale);
        _mapImage.transform.localScale = scale * Vector3.one;
    }
    
    public void AddInteractable(string type, Vector3 worldPosition, string identifier)
    {
        Type t = Type.GetType( type );
        if ( _interactableIconsPrefabs.ContainsKey( t ) )
        {
            GameObject icon = Instantiate( _interactableIconsPrefabs[t] ) as GameObject;
            icon.transform.SetParent( _mapImage.transform, false );
            icon.transform.localPosition = WorldToRadar( worldPosition );
            icon.transform.localScale = m_iconScale * Vector3.one;
            icon.GetComponent<InteractableIcon>().Identifier = identifier;
        }
    }
    
}
