using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HackingSymbolButton : MonoBehaviour {

    [SerializeField]
    private Image _checkMarkImage;

    [SerializeField]
    private Image _symbolImage;

    [SerializeField]
    private Text _text;

    private HackingSymbols _current;

    public Image CheckMark
    {
        get { return _checkMarkImage; }
    }

    public HackingSymbols Symbol
    {
        get { return _current; }
        set
        {
            _current = value;
            Texture2D t = Resources.Load( _current.ToString() ) as Texture2D;
            _symbolImage.sprite = Sprite.Create( t, new Rect( 0, 0, t.width, t.height ), new Vector2( 0.5f, 0.5f ) );
        }
    }
}
