using UnityEngine;
using System.Collections;
using Kathulhu;

public class HackingPanel : UIPanel {

    public HackingPanelGameCommand command;

    protected override void OnDeactivate()
    {
        if ( command != null && command.State == CommandState.Running )
        {
            command.FailHacking();
        }
    }
}
