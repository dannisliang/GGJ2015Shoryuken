using UnityEngine;
using System.Collections;
using Kathulhu;

public class ReturnToMenuCmd : EventCommand {

    public override void Execute()
    {
        base.Execute();

        BoltLauncher.Shutdown();

        if (UIManager.Current == null)
            return;        

        UIManager.Current.Panels["WaitingPanel"].Deactivate();
        UIManager.Current.Panels["MainMenuPanel"].Activate();
        
    }
}
