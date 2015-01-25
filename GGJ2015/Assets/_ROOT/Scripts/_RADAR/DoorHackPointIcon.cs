using UnityEngine;
using System.Collections;

public class DoorHackPointIcon : InteractableIcon
{


    public override void OnPointerClick( UnityEngine.EventSystems.PointerEventData eventData )
    {

        HackingPanelGameCommand cmd = new HackingPanelGameCommand();
        cmd.Execute();

        //UnlockDoor unlock = UnlockDoor.Create();
        //unlock.HackPointIdentifier = Identifier;
        //unlock.Send();
    }

}
