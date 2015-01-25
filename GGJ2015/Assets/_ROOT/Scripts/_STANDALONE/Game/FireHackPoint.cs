using UnityEngine;
using System.Collections;

public class FireHackPoint : InteractableObject
{
    public void StopFire()
    {
        Debug.Log("StopFire");
        FireZoneManager fzm = GetComponent<FireZoneManager>();
        fzm.TriggerFires(false);
    }
}
