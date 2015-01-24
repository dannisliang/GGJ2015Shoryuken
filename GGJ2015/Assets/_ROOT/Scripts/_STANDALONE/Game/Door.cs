using UnityEngine;
using System.Collections;

public class Door : InteractableObject {

    [SerializeField]
    public bool Locked = true;

    public void Unlock()
    {
        if ( Locked == true )
        {
            Locked = false;

            //outline door

            //etc.
        }
    }

}
