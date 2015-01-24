using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using Kathulhu;

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
        //GameController.Registry.Register<BoltConnection>( connection, "Client" );
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

}
