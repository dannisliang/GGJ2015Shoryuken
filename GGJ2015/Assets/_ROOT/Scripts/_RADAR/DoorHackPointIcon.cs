using UnityEngine;
using System.Collections;
using Kathulhu;

public class DoorHackPointIcon : InteractableIcon
{


    public override void OnPointerClick( UnityEngine.EventSystems.PointerEventData eventData )
    {

        HackingPanelGameCommand cmd = new HackingPanelGameCommand();
        GameController.Execute( cmd );

        //UnlockDoor unlock = UnlockDoor.Create();
        //unlock.HackPointIdentifier = Identifier;
        //unlock.Send();
    }

}
