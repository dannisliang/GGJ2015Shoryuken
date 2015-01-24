using UnityEngine;
using System.Collections;
using Kathulhu;

public class RadarMenuSceneManager : SceneManager {

    public void JoinGame()
    {
        BoltConsole.Write("[Trying to join game]");

        BoltLauncher.StartClient();
        BoltNetwork.Connect( UdpKit.UdpEndPoint.Parse( "127.0.0.1:27000" ) );

    }

}
