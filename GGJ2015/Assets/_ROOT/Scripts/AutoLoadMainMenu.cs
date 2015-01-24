using UnityEngine;
using System.Collections;

public class AutoLoadMainMenu : MonoBehaviour {
	
	void Start () {
        Kathulhu.GameController.LoadScene( "MainMenu" );
	}
	
}
