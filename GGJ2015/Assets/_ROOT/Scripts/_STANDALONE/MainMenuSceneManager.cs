using UnityEngine;
using System.Collections;
using Kathulhu;

public class MainMenuSceneManager : SceneManager {



    public void StartGame()
    {
        BoltLauncher.StartServer( UdpKit.UdpEndPoint.Parse( "127.0.0.1:27000" ) );
        
        GameController.LoadScene( "Game" );
    }

}
