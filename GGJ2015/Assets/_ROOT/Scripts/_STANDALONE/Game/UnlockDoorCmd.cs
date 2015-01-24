using UnityEngine;
using System.Collections;
using Kathulhu;

public class UnlockDoorCmd : EventCommand {

    public string identifier = "";    

	public override void Execute()
    {
        if (!string.IsNullOrEmpty(identifier))
        {

            base.Execute();

            Door door = GameController.Registry.Resolve<Door>( identifier );
            if (door != null)
            {
                door.Unlock();
            }
        } 	    
    }
}
