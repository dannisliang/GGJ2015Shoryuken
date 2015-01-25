using UnityEngine;
using System.Collections;
using Kathulhu;

public class RadarGameUIManager : UIManager {

    public void CloseHackingPanel()
    {
        Panels["HackingPanel"].Deactivate();
    }

}
