using UnityEngine;
using System.Collections;
using Kathulhu;

public class AppInit : MonoBehaviour 
{

    public bool host = true;

	
	void Start () {

#if UNITY_ANDROID
        GameController.LoadScene( "MainMenuRadar" );
#else
        GameController.LoadScene( host ? "MainMenu" : "MainMenuRadar" );
#endif

        Destroy( gameObject );
	}

}
