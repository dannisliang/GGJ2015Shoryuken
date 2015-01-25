using UnityEngine;
using System.Collections;
using Kathulhu;

public class RadarGameUIManager : UIManager {

    public void CloseHackingPanel()
    {
        Panels["HackingPanel"].Deactivate();
    }


    void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Space ) )
        {
            HackingPanelGameCommand cmd = new HackingPanelGameCommand();
            GameController.Execute( cmd );
        }
    }
}
