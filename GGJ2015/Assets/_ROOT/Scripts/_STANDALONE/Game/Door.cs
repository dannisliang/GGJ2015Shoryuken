using UnityEngine;
using System.Collections;

public class Door : InteractableObject {
    
    public bool Locked = true;

    public DoorHackPoint HackPoint { get { return _hackPoint; } }

	private bool opening;
	private bool opened = false;
	private bool closeed = true;

	public Renderer doorRenderer;

	public Collider doorCollider;

	public Animation animationDoor;

    [SerializeField]
    private DoorHackPoint _hackPoint;

    public void Unlock()
    {
        if ( Locked == true )
        {
            Debug.Log( "Door " + Identifier + " unlocked!" );
            Locked = false;

            //outline door

            //etc.
        }
    }

	float time = 0;
	public void Update()
	{
        if ( doorRenderer != null )
        {
            if ( Locked )
            {
                doorRenderer.material.SetColor( "_OutlineColor", Color.red );
            }
            else
                doorRenderer.material.SetColor( "_OutlineColor", Color.green );
        }

		if(opening && !opened)
		{
			//Animation
			animationDoor["Take 001"].time = 0;
			animationDoor["Take 001"].speed = 3;
			animationDoor.Play("Take 001");
			doorCollider.enabled = false;
			opened = true;
			closeed = false;
		}
		else if(!opening && !closeed)
		{
			animationDoor["Take 001"].time = animationDoor["Take 001"].length;
			animationDoor["Take 001"].speed = -3;
			animationDoor.Play("Take 001");
			doorCollider.enabled = true;
			opened = false;
			closeed = true;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Player")
		{
			if(!Locked)
			{
				opening = true;
			}
		}
	}

	public void OnTriggerStay(Collider other)
	{
		if(other.tag == "Player")
		{
			if(!Locked)
			{
				opening = true;
			}
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if(other.tag == "Player")
		{
			if(!Locked)
			{
				opening = false;
			}
		}
	}
}
