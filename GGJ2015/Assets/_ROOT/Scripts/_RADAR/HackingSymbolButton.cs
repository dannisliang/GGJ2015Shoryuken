using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
            //_symbolImage.sprite = symbol

            _text.text = _current.ToString();
        }
    }
}
