using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MAObjectContext {
    private static Dictionary<GameObject, MAGOSetting> persistentSettings = new Dictionary<GameObject, MAGOSetting>();

    public GameObject GameObj { get; private set; }

    public MAGOSetting GameObjectSetting { get; private set; }

    private MAGOSetting GetStoredGameObjectSetting() {
        MAGOSetting setting = null;

        if (persistentSettings.ContainsKey(GameObj)) {
            setting = persistentSettings[GameObj];
        } else {
            setting = new MAGOSetting(GameObj);
            persistentSettings.Add(GameObj, setting);
        }

        return setting;
    }

    public void SetContext(Object target) {
        GameObj = ((Transform)target).gameObject;

        if (GameObjectSetting == null) {
            GameObjectSetting = GetStoredGameObjectSetting();
        }
    }
}
