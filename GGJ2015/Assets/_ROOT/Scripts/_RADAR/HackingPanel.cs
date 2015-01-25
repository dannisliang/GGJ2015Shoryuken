using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Kathulhu;

public class HackingPanel : UIPanel {

    public HackingPanelGameCommand command;

    [SerializeField]
    private Text[] symbolTexts;

    private enum HackingSymbols {
        un,
        deux,
        trois,
        quatre,
        cinq,
        six,
        sept,
        huit,
        neuf,
        dix
    }    

    private List<HackingSymbols>[] _symbols = new List<HackingSymbols>[] 
                    { 
                        new List<HackingSymbols>(), 
                        new List<HackingSymbols>(), 
                        new List<HackingSymbols>() 
                    };

    void OnEnable()
    {
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

        foreach ( var aSymbolList in _symbols )
        {
            string s = "symbols : ";
            aSymbolList.Insert( Random.Range( 0, aSymbolList.Count ), list[0] );
            foreach ( var item in aSymbolList )
                s += item.ToString() + " ";

            Debug.Log( s );
        }


    }

    protected override void OnDeactivate()
    {
        if ( command != null && command.State == CommandState.Running )
        {
            command.FailHacking();
            command = null;
        }
    }

    public void OnSymbolButtonClick( Text txt )
    {
        Debug.Log("Click on " + txt.gameObject);
        for ( int i = 0; i < 3; i++ )
        {
            if ( txt == symbolTexts[i] )
            {
                _symbols[i].Add( _symbols[i][0] );
                _symbols[i].RemoveAt( 0 );
                txt.text = _symbols[i][0].ToString();
                break;
            }
        }

        if ( symbolTexts[0].text == symbolTexts[1].text && symbolTexts[1].text == symbolTexts[2].text )
        {
            if ( command != null )
            {
                command.CompleteHacking();
            }
        }
    }
}
