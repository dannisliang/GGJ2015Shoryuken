using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DottedLines : MonoBehaviour {

    public static DottedLines Instance { get; private set; }

    [SerializeField]
    private List<Image> _dottedLines;

    void Awake()
    {
        Instance = this;

        if ( _dottedLines == null )
            _dottedLines = new List<Image>();
    }

    public void SetVisibility( int indice, bool visible )
    {
        if ( indice > _dottedLines.Count )
            return;

        _dottedLines[indice].enabled = visible;
    }
}
