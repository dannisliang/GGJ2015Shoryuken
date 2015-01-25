using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class RoomManager : MonoBehaviour {

    [SerializeField]
    private List<InteractableObject> _hiddenInteractableObjects;

    void Awake(){
        if ( _hiddenInteractableObjects == null )
            _hiddenInteractableObjects = new List<InteractableObject>();
    }

    void OnTriggerEnter( Collider other )
    {
        if ( other.tag == "Player" )
            SetInteractablesVisibility(true);
    }

    void OnTriggerExit( Collider other )
    {
        if ( other.tag == "Player" )
            SetInteractablesVisibility(false);
    }

    void SetInteractablesVisibility(bool value)
    {
        foreach ( var item in _hiddenInteractableObjects )
        {
            SetInteractableIconVisibility evt = SetInteractableIconVisibility.Create();
            evt.InteractableIdentifier = item.Identifier;
            evt.Visible = value;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube( transform.position, transform.localScale );
    }

}
