using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using Kathulhu;
using System.Linq;

[BoltGlobalBehaviour( BoltNetworkModes.Server )]
public class StandaloneBoltCallbacks : GlobalEventListener
{

    public override void Disconnected( BoltConnection connection )
    {
        if ( GameController.ActiveSceneManager.SceneName == "Game" )
        {
            BoltLauncher.Shutdown();
            GameController.LoadScene( "MainMenu" );
        }
    }

    public override void Connected( BoltConnection connection )
    {
        if (GameController.ActiveSceneManager.SceneName == "MainMenu")
        {
            var evt = EnterTheGameScene.Create();
            evt.Send();
        }
    }

    public override void OnEvent( EnterTheGameScene evnt )
    {
        if ( GameController.ActiveSceneManager.SceneName == "MainMenu" )
            GameController.LoadScene( "Game" );
    }

    public override void OnEvent( UnlockDoor evnt )
    {
        if ( string.IsNullOrEmpty( evnt.HackPointIdentifier ) )
            return;

        foreach ( var door in GameController.Registry.ResolveAll<Door>() )
        {
            if ( door.HackPoint != null && door.HackPoint.Identifier == evnt.HackPointIdentifier )
            {
                door.Unlock();
                break;
            }
        } 
    }

}
