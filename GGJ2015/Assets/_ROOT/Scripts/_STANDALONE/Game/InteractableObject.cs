using UnityEngine;
using System.Collections;
using Kathulhu;

public class InteractableObject : MonoBehaviour {

    public string Identifier { get; private set; }

    public int DottedLinesIndice { get; set; }

    [SerializeField]
    private int _dottedLineIndice = -1;

    public bool IsVisible { 
        get { return _isVisible; } 
        set {

            if ( value != _isVisible )
            {
                _isVisible = value;

                SetInteractableIconVisibility evt = SetInteractableIconVisibility.Create();
                evt.InteractableIdentifier = Identifier;
                evt.Visible = _isVisible;
                evt.DottedLineIndice = DottedLinesIndice;
                evt.Send();
            }
        } 
    }

    [SerializeField]
    private bool _isVisible;

    protected virtual void Awake()
    {
        DottedLinesIndice = _dottedLineIndice;

        Identifier = System.Guid.NewGuid().ToString();

        GameController.Registry.Register<InteractableObject>( this, Identifier );
    }

    

}
