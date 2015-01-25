using UnityEngine;
using System.Collections;

public class fx_bullet : MonoBehaviour {

	private Light myLight;
	private float mySpeed;

	public void Flash(float speed){
		mySpeed = speed;
		myLight = this.GetComponent<Light>();
		myLight.intensity = 1;
		StartCoroutine ("Anim");
	}
	
	IEnumerator Anim(){
		while(myLight.intensity > 0){
			myLight.intensity = Mathf.Lerp(1,0,Time.deltaTime * mySpeed);
			yield return new WaitForSeconds(mySpeed/2f);
			Destroy(this.gameObject);
		}
	}

}
