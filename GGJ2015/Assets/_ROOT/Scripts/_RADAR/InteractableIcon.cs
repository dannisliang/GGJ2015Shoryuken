using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public abstract class InteractableIcon : MonoBehaviour, IPointerClickHandler {

    public string Identifier { get; set; }


    public abstract void OnPointerClick( PointerEventData eventData );
}
