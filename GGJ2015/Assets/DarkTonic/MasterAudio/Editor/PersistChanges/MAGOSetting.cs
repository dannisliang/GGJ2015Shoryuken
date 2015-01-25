using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MAGOSetting {
	private const string IgnoredComponentNames = ";Transform;AudioSource;SoundGroupVariationUpdater;";

    private List<MAComponentPatch> componentSettings;

    public MAGOSetting(GameObject gameObj) {
        this.GameObj = gameObj;

        CreateComponentSettings();
    }

    public GameObject GameObj { get; set; }

    public List<MAComponentPatch> ComponentSettings {
        get { return componentSettings; }
    }

    public void CreateComponentSettings() {
        componentSettings = new List<MAComponentPatch>();

        Component[] components = GameObj.GetComponents(typeof(Component));

        foreach (Component c in components) {
            MAComponentPatch setting = new MAComponentPatch(c);

            if (c == null) {
                continue;
            }

            if (IgnoredComponentNames.Contains(";" + setting.ComponentName + ";")) {
                continue;
            }

            componentSettings.Add(setting);
        }
    }

    public void StoreAllSelectedSettings() {
        componentSettings.ForEach(setting => setting.StoreSettings());
    }

    public List<Component> RestoreAllSelectedSettings() {
        List<Component> listOfChangedComponents = new List<Component>();
        Component resultChangedComponent = null;
        foreach (MAComponentPatch setting in componentSettings) {
            resultChangedComponent = setting.RestoreSettings();
            if (resultChangedComponent != null) {
                listOfChangedComponents.Add(resultChangedComponent);
            }
        }

        return listOfChangedComponents;
    }
}
