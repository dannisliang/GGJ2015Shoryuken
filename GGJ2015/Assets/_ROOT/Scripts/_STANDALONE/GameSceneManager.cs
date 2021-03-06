﻿using UnityEngine;
using System.Collections;
using Kathulhu;

public class GameSceneManager : SceneManager {

    public override IEnumerator Load()
    {
        LoadingProgressUpdateEvent evt = new LoadingProgressUpdateEvent();

        evt.progress = 0f;
        evt.message = "Scene setup...";
        EventDispatcher.Event( evt );

        yield return new WaitForSeconds( 1 ); ;
        
        foreach ( var item in GameController.Registry.ResolveAll<InteractableObject>() )
        {
            if (item == null)
                continue;

            RegisterInteractableObjectOnRadar registerEvt = RegisterInteractableObjectOnRadar.Create();
            registerEvt.Identifier = item.Identifier;
            registerEvt.Type = item.GetType().ToString();
            registerEvt.Position = item.transform.position;
            registerEvt.Visible = item.IsVisible;

            registerEvt.Send();
        }

        yield return new WaitForSeconds( 0.5f );
        
        evt.progress = 1f;
        evt.message = "Loading completed!";
        EventDispatcher.Event( evt );

        yield return new WaitForSeconds( 0.25f );

    }

}
