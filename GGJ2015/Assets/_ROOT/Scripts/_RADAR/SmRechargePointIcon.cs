using UnityEngine;
using System.Collections;
using Kathulhu;

public class SmRechargePointIcon : InteractableIcon
{


    public override void OnPointerClick( UnityEngine.EventSystems.PointerEventData eventData )
    {
        /*Debug.Log("Click");
        HackingPanelGameCommand cmd = new HackingPanelGameCommand();
        cmd.OnHackingResult += HackResult;
        GameController.Execute( cmd );*/        
    }

    void HackResult(HackingPanelGameCommand cmd)
    {
        /*if ( cmd.State == CommandState.Completed )
        {
            ExtinguishFire extinguishFire = ExtinguishFire.Create();
            extinguishFire.Identifier = Identifier;
            extinguishFire.Send();
        }*/
    }

}
