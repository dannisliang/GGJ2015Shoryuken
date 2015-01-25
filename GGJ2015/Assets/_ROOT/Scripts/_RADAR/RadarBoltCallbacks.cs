using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using Kathulhu;

[BoltGlobalBehaviour( BoltNetworkModes.Client )]
public class RadarBoltCallbacks : GlobalEventListener
{

    public override void Disconnected( BoltConnection connection )
    {
            GameController.LoadScene( "MainMenuRadar" );
    }

    public override void OnEvent( EnterTheGameScene evnt )
    {        
        if ( GameController.ActiveSceneManager.SceneName == "MainMenuRadar" )
            GameController.LoadScene("GameRadar");
    }

    public override void OnEvent( RegisterInteractableObjectOnRadar evnt )
    {                
        RadarManager.Instance.AddInteractable( evnt.Type, evnt.Position, evnt.Identifier, evnt.Visible );
    }

    public override void OnEvent( SetInteractableIconVisibility evnt )
    {
        RadarManager.Instance.SetInteractableVisibility( evnt.InteractableIdentifier, evnt.Visible );
    }

    public override void OnEvent( PingMonsterPosition evnt )
    {
        RadarManager.Instance.ShowPing( evnt.Position );
    }
    
}
