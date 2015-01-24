using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ButtonClose : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData)
    {
        RadarManager.Instance.SetOpenedMenu(null);
    }
}
