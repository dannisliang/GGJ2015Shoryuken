using UnityEngine;
using System.Collections;

public class DoorHackPointIcon : InteractableIcon
{


    public override void OnPointerClick( UnityEngine.EventSystems.PointerEventData eventData )
    {
        //do something
        UnlockDoor unlock = UnlockDoor.Create();
        unlock.HackPointIdentifier = Identifier;
        unlock.Send();
    }

}
