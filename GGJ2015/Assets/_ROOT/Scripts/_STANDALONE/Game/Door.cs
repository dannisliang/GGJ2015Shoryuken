using UnityEngine;
using System.Collections;

public class Door : InteractableObject {
    
    public bool Locked = true;

    public DoorHackPoint HackPoint { get { return _hackPoint; } }

	public Vector3 openPosition;
	public Vector3 closePosition;
	private bool opening;

	public Renderer doorRenderer;

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

		if(opening)
		{
			MoveTowardsTarget(openPosition);
		}
		else
		{
			MoveTowardsTarget(closePosition);
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

	private void MoveTowardsTarget(Vector3 targetPosition) {
		//the speed, in units per second, we want to move towards the target
		float speed = 1;
		//move towards the center of the world (or where ever you like)
		
		Vector3 currentPosition = this.transform.position;
		//first, check to see if we're close enough to the target
		if(Vector3.Distance(currentPosition, targetPosition) > .1f) { 
			Vector3 directionOfTravel = targetPosition - currentPosition;
			//now normalize the direction, since we only want the direction information
			directionOfTravel.Normalize();
			//scale the movement on each axis by the directionOfTravel vector components
			
			this.transform.Translate(
				(directionOfTravel.x * speed * Time.deltaTime),
				(directionOfTravel.y * speed * Time.deltaTime),
				(directionOfTravel.z * speed * Time.deltaTime),
				Space.World);
		}
	}

}
