using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using Kathulhu;

public abstract class InteractableIcon : MonoBehaviour, IPointerClickHandler {
    
    public string Identifier { get; set; }

    public bool IsVisible { 
        get { return gameObject.activeInHierarchy; } 
        set { gameObject.SetActive( value ); } 
    }


    public abstract void OnPointerClick( PointerEventData eventData );

    protected virtual void Start()
    {
        GameController.Registry.Register<InteractableIcon>( this, Identifier );
    }

}
