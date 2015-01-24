using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using Kathulhu;

[BoltGlobalBehaviour( BoltNetworkModes.Client )]
public class RadarBoltCallbacks : GlobalEventListener
{

    public override void OnEvent( EnterTheGameScene evnt )
    {        
        if ( GameController.ActiveSceneManager.SceneName == "MainMenuRadar" )
            GameController.LoadScene("GameRadar");
    }
    
}
