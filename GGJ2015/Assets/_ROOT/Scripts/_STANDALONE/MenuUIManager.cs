using UnityEngine;
using System.Collections;
using Kathulhu;

public class MenuUIManager : UIManager {


    public void StartGame()
    {
        BoltLauncher.StartServer( UdpKit.UdpEndPoint.Parse( "127.0.0.1:27000" ) );

        Panels["MainMenuPanel"].Deactivate();
        Panels["WaitingPanel"].Activate();
    }

    public void CancelGame()
    {
        BoltLauncher.Shutdown();

        Panels["MainMenuPanel"].Activate();
        Panels["WaitingPanel"].Deactivate();
    }

}
