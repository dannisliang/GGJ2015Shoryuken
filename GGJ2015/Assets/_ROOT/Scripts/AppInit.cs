using UnityEngine;
using System.Collections;
using Kathulhu;

public class AppInit : MonoBehaviour 
{

    public bool host = true;

	
	void Start () {

        GameController.LoadScene(host ? "MainMenu" : "MainMenuRadar");

//#if !UNITY_ANDROID
//#else 
//        GameController.LoadScene( "MainMenuRadar" );
//#endif

        Destroy( gameObject );
	}

}
