using UnityEngine;
using System.Collections;
using Kathulhu;
using UdpKit;

public class MenuUIManager : UIManager {

    private ReturnToMenuCmd returnToMenu = new ReturnToMenuCmd();
    private CreateGameCmd createGameCmd = new CreateGameCmd();


    public void StartGame()
    {        
        createGameCmd.Execute();
    }

    public void CancelGame()
    {        
        returnToMenu.Execute();
    }

}
