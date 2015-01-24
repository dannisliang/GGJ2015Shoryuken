using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ButtonHackManager : MonoBehaviour, IPointerClickHandler {

    public GameObject m_contextMenu;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject go = Object.Instantiate(m_contextMenu) as GameObject;
        RadarManager.Instance.Parent(go);
        go.transform.localPosition = go.transform.localPosition;
        RadarManager.Instance.SetOpenedMenu(go);
        RadarManager.Instance.ToggleScan(false);
    }
}
