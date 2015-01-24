using UnityEngine;
using System.Collections;
using Kathulhu;

public class AutoLoadScene : MonoBehaviour {

    public string sceneName = "MainMenu";

	void Start () {

        GameController.LoadScene( sceneName );

	}
	
}
