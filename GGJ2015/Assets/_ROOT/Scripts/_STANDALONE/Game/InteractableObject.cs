using UnityEngine;
using System.Collections;
using Kathulhu;

public class InteractableObject : MonoBehaviour {

    public string Identifier { get; private set; }

    public bool IsVisible { 
        get { return _isVisible; } 
        set {

            if ( value != _isVisible )
            {
                _isVisible = value;

                SetInteractableIconVisibility evt = SetInteractableIconVisibility.Create();
                evt.InteractableIdentifier = Identifier;
                evt.Visible = _isVisible;
                evt.Send();
            }
        } 
    }

    [SerializeField]
    private bool _isVisible;

    protected virtual void Awake()
    {
        Identifier = System.Guid.NewGuid().ToString();

        GameController.Registry.Register<InteractableObject>( this, Identifier );
    }

    

}
