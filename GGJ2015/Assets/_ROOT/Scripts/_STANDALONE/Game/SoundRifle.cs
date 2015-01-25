using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundRifle : MonoBehaviour {

	public CapsuleCollider collider;
	public int munitions;
	public float coolDown;
	public bool inCoolDown = false;


	//public TextMesh munitionsText;
	//public TextMesh coolDownText;
	public List<Material> battery = new List<Material>();
	public GameObject currentBattery;

	public GameObject bulletPrefab;
	public GameObject launchPosition;

	private bool clicked;

	// Use this for initialization
	void Start () {
		collider.enabled = false;
	}

	float time = 0;
	int count = 5;
	// Update is called once per frame
	void Update () {
		currentBattery.renderer.material = battery[munitions];
		//munitionsText.text = munitions.ToString();

		if((Input.GetMouseButton(0) && munitions <= 0 && !clicked))
		{
			MasterAudio.PlaySound3DAtTransformAndForget( "SFX_Gun_Unready", transform);
		}

		if(Input.GetMouseButton(0) && !inCoolDown && !clicked)
		{
			clicked = true;
			if(munitions > 0){
				MasterAudio.PlaySound3DAtTransformAndForget("SFX_Gun_Shot", transform);
				munitions--;
				//coolDownText.text = "5";
				collider.enabled = true;
				inCoolDown = true;
				//Animations
				gameObject.GetComponent<Animation>()["Take 001"].speed = 2;
				gameObject.GetComponent<Animation>().Play("Take 001");
				GameObject bullet =(GameObject) Instantiate(bulletPrefab, transform.position, Quaternion.identity);
				bullet.transform.rotation = Quaternion.LookRotation(GameManager.Instance.PlayerInstance.transform.forward);
				bullet.GetComponent<fx_bullet>().Flash(1);
			}
		}

		if(Input.GetMouseButton(0) && inCoolDown && !clicked)
		{
			MasterAudio.PlaySound3DAtTransformAndForget( "SFX_Gun_Unready", transform);
		}




		if(inCoolDown)
		{
			time += Time.deltaTime;
			if(time > 1){
				time = 0;
				count--;
				//coolDownText.text = count.ToString();
				collider.enabled = false;
				if(count <= 0){
					inCoolDown = false;
					count = 5;
				}
			}
		}

		if(clicked){
			if(Input.GetMouseButtonUp(0))
			{
				clicked = false;
			}
		}
	}
}
