using UnityEngine;
using System.Collections;
using Bolt;

public class DoorEntity : EntityBehaviour<IDoorState> {

    public bool testUnlock = false;

    private BoltEntity entity;

    public override void Attached()
    {
        base.Attached();

        entity = GetComponent<BoltEntity>();

        state.AddCallback( "Unlocked", UnlockedChanged );
    }

    void UnlockedChanged()
    {
        Debug.Log( "Door:State:Unlocled->" + state.Unlocked );
    }

    public void Unlock()
    {
        if ( !entity.isOwner )
        {
            Debug.Log("Change state to Unlocked");
            state.Unlocked = true;
        }
    }

    void Update()
    {
        if ( testUnlock )
        {
            testUnlock = false;
            Unlock();
        }
    }
}
