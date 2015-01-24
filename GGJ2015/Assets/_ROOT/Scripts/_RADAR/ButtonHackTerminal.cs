using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ButtonHackTerminal : MonoBehaviour, IPointerClickHandler {

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Hack Terminal!!!!!!!!!!!!!!");
    }
}
