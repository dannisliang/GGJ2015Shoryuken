using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Kathulhu;

public enum HackingSymbols
{
    symbolun,
    symboldeux,
    symboltrois,
    symbolquatre,
    symbolcinq,
    symbolsix,
    symbolsept,
    symbolhuit,
    symbolneuf,
    symboldix
}  

public class HackingPanel : UIPanel {

    public HackingPanelGameCommand command;

    [SerializeField]
    private HackingSymbolButton[] symbolButtons;

    private CanvasGroup _canvasGroup;      

    private List<HackingSymbols>[] _symbols = new List<HackingSymbols>[] 
                    { 
                        new List<HackingSymbols>(), 
                        new List<HackingSymbols>(), 
                        new List<HackingSymbols>() 
                    };

    protected override void Awake()
    {
        base.Awake();

        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        _canvasGroup.interactable = true;

        foreach ( var item in symbolButtons )
            item.CheckMark.enabled = false;

        ShuffleSymbols();
    }

    void ShuffleSymbols()
    {
        foreach ( var item in _symbols )
            item.Clear();

        List<HackingSymbols> list = new List<HackingSymbols>();
        foreach ( HackingSymbols symbol in System.Enum.GetValues( typeof( HackingSymbols ) ) )
            list.Add( symbol );

        //sucky shuffling algorithm
        System.Random rng = new System.Random();
        int n = list.Count;
        while ( n > 1 )
        {
            n--;
            int k = rng.Next( n + 1 );
            HackingSymbols value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        //add invalid symbols to lists
        while ( list.Count > 1 )
        {
            if ( _symbols[0].Count < 4 )
                _symbols[0].Add( list[0] );
            else if ( _symbols[1].Count < 3 )
                _symbols[1].Add( list[0] );
            else if ( _symbols[2].Count < 2 )
                _symbols[2].Add( list[0] );

            list.RemoveAt( 0 );
        }

        //insert valid symbol at random position
        foreach ( var aSymbolList in _symbols )
            aSymbolList.Insert( Random.Range( 0, aSymbolList.Count ), list[0] );            

        //hack to make sure the default answer isn't the correct answer
        if ( symbolButtons[0].Symbol == symbolButtons[1].Symbol && symbolButtons[1].Symbol == symbolButtons[2].Symbol )
        {
            _symbols[1].Add( _symbols[1][0] );
            _symbols[1].RemoveAt( 0 );

            _symbols[2].Add( _symbols[2][0] );
            _symbols[2].RemoveAt( 0 );
        }

        //init buttons
        symbolButtons[0].Symbol = _symbols[0][0];
        symbolButtons[1].Symbol = _symbols[1][0];
        symbolButtons[2].Symbol = _symbols[2][0];

    }

    protected override void OnDeactivate()
    {
        if ( command != null && command.State == CommandState.Running )
        {
            command.FailHacking();
            command = null;
        }
    }

    public void OnSymbolButtonClick( HackingSymbolButton btn )
    {
        if ( btn.CheckMark.enabled )
            return;

        //toggle button's symbol
        for ( int i = 0; i < 3; i++ )
        {
            if ( btn == symbolButtons[i] )
            {
                _symbols[i].Add( _symbols[i][0] );
                _symbols[i].RemoveAt( 0 );
                btn.Symbol = _symbols[i][0];
                break;
            }
        }

        //Check if correct answer
        if ( symbolButtons[0].Symbol == symbolButtons[1].Symbol && symbolButtons[1].Symbol == symbolButtons[2].Symbol )
        {
            foreach ( var item in symbolButtons )
                item.CheckMark.enabled = true;

            if ( command != null )
                command.CompleteHacking();
        }
    }
}
