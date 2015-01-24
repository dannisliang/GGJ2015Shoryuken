using UnityEngine;
using System.Collections;
using Kathulhu;

public class InteractableObject : MonoBehaviour {

    public string Identifier { get; private set; }

    protected virtual void Awake()
    {
        Identifier = System.Guid.NewGuid().ToString();

        GameController.Registry.Register<InteractableObject>( this, Identifier );
    }

    

}
