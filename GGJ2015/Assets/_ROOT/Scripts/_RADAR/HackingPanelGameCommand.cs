using UnityEngine;
using System.Collections;
using Kathulhu;
using System;

public class HackingPanelGameCommand : GameCommand {

    public Action<HackingPanelGameCommand> OnHackingResult;


    protected override void OnExecute()
    {
        HackingPanel hPnl = UIManager.Current.Panels["HackingPanel"] as HackingPanel;
        hPnl.command = this;
        hPnl.Activate();
    }

    protected override void OnAbort()
    {
        Debug.Log( "Hacking aborted!" );
    }

    protected override void OnComplete()
    {
        base.OnComplete();

        Debug.Log( "Hacking completed!" );

        HackingPanel hPnl = UIManager.Current.Panels["HackingPanel"] as HackingPanel;
        hPnl.Deactivate();
    }

    /// <summary>
    /// Method to complete succesfully the hacking challenge
    /// </summary>
    public void CompleteHacking()
    {
        HackingPanel hPnl = UIManager.Current.Panels["HackingPanel"] as HackingPanel;
        hPnl.GetComponent<CanvasGroup>().interactable = false;

        GameController.Instance.StartCoroutine( DelayedClose() );
    }

    IEnumerator DelayedClose()
    {
        yield return new WaitForSeconds( 2 );

        Complete();

        if ( OnHackingResult != null )
            OnHackingResult( this );  
    }

    /// <summary>
    /// Method to fail the hacking challenge
    /// </summary>
    public void FailHacking()
    {
        Abort();

        if ( OnHackingResult != null )
            OnHackingResult( this );
    }
}
