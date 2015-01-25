using UnityEngine;
using System.Collections;
using Kathulhu;

public class LeverIcon : InteractableIcon
{
    public override void OnPointerClick( UnityEngine.EventSystems.PointerEventData eventData )
    {
        //HackingPanelGameCommand cmd = new HackingPanelGameCommand();
        //cmd.OnHackingResult += HackResult;
        //GameController.Execute( cmd );        
    }

    /*void HackResult(HackingPanelGameCommand cmd)
    {
        if ( cmd.State == CommandState.Completed )
        {
            UnlockDoor unlock = UnlockDoor.Create();
            unlock.HackPointIdentifier = Identifier;
            unlock.Send();
        }
    }*/

}
