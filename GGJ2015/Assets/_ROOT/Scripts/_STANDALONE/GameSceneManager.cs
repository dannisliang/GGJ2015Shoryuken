using UnityEngine;
using System.Collections;
using Kathulhu;

public class GameSceneManager : SceneManager {

    public override IEnumerator Load()
    {
        LoadingProgressUpdateEvent evt = new LoadingProgressUpdateEvent();

        evt.progress = 0f;
        evt.message = "Entering game scene.";
        EventDispatcher.Event( evt );
        yield return new WaitForSeconds( 0.5f );
        evt.progress = 0f;
        evt.message = "Entering game scene..";
        EventDispatcher.Event( evt );
        yield return new WaitForSeconds( 0.5f );
        evt.progress = 0f;
        evt.message = "Entering game scene...";
        EventDispatcher.Event( evt );
        yield return new WaitForSeconds( 0.5f );
        evt.progress = 1f;
        evt.message = "Loading completed!";
        EventDispatcher.Event( evt );
        yield return new WaitForSeconds( 0.5f );
        
    }

}
