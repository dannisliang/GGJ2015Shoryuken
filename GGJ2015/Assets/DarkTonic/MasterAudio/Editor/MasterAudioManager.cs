using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class MasterAudioManager : EditorWindow {
	private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Window/Master Audio Manager")]
    static void Init() {
        EditorWindow.GetWindow(typeof(MasterAudioManager));
    }

    void OnGUI() {
        scrollPos = GUI.BeginScrollView(
                new Rect(0, 0, position.width, position.height),
                scrollPos, 
                new Rect(0, 0, 550, 362)
        );

        PlaylistController.Instances = null;
        var pcs = PlaylistController.Instances;
        var plControllerInScene = pcs.Count > 0;

        if (MasterAudioInspectorResources.logoTexture != null) {
            DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.logoTexture);
        }

        if (Application.isPlaying) {
            DTGUIHelper.ShowLargeBarAlert("This screen cannot be used during play.");
            GUI.EndScrollView();
            return;
        }

        Texture settings = MasterAudioInspectorResources.gearTexture;
		
        MasterAudio.Instance = null;
        var ma = MasterAudio.Instance;
        var maInScene = ma != null;
		
		var organizer = GameObject.FindObjectOfType(typeof(SoundGroupOrganizer));
		var hasOrganizer = organizer != null;
		
        DTGUIHelper.ShowColorWarning("The Master Audio prefab holds sound FX group and mixer controls. Add this first (only one per scene).");
        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);

        EditorGUILayout.LabelField("Master Audio prefab", GUILayout.Width(300));
        if (!maInScene) {
            GUI.contentColor = Color.green;
            if (GUILayout.Button(new GUIContent("Create", "Create Master Audio prefab"), EditorStyles.toolbarButton, GUILayout.Width(100))) {
                GameObject go = MasterAudio.CreateMasterAudio();
				UndoHelper.CreateObjectForUndo(go, "Create Master Audio prefab");		
            }
            GUI.contentColor = Color.white;
        } else {
            if (settings != null) {
                if (GUILayout.Button(new GUIContent(settings, "Master Audio Settings"), EditorStyles.toolbarButton)) {
                    Selection.activeObject = ma.transform;
                }
            }
            EditorGUILayout.LabelField("Exists in scene", EditorStyles.boldLabel);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        // Playlist Controller
        DTGUIHelper.ShowColorWarning("The Playlist Controller prefab controls sets of songs (or other audio) and ducking. No limit per scene.");
        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
        EditorGUILayout.LabelField("Playlist Controller prefab", GUILayout.Width(300));

        GUI.contentColor = Color.green;
        if (GUILayout.Button(new GUIContent("Create", "Place a Playlist Controller prefab in the current scene."), EditorStyles.toolbarButton, GUILayout.Width(100))) {
            var go = MasterAudio.CreatePlaylistController();
			UndoHelper.CreateObjectForUndo(go, "Create Playlist Controller prefab");
        }
        GUI.contentColor = Color.white;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        if (!plControllerInScene) {
            DTGUIHelper.ShowRedError("*There is no Playlist Controller in the scene. Music will not play.");
        }

        EditorGUILayout.Separator();
        // Dynamic Sound Group Creators
        DTGUIHelper.ShowColorWarning("The Dynamic Sound Group Creator prefab can per-Scene Sound Groups and other audio. No limit per scene.");
        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
        EditorGUILayout.LabelField("Dynamic Sound Group Creator prefab", GUILayout.Width(300));

        GUI.contentColor = Color.green;
        if (GUILayout.Button(new GUIContent("Create", "Place a Dynamic Sound Group prefab in the current scene."), EditorStyles.toolbarButton, GUILayout.Width(100))) {
            var go = MasterAudio.CreateDynamicSoundGroupCreator();
			UndoHelper.CreateObjectForUndo(go, "Create Dynamic Sound Group Creator prefab");
		}

        GUI.contentColor = Color.white;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
		
		
        EditorGUILayout.Separator();
        // Sound Group Organizer
        DTGUIHelper.ShowColorWarning("The Sound Group Organizer prefab can import/export Groups to/from MA and Dynamic SGC prefabs.");
        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
        EditorGUILayout.LabelField("Sound Group Organizer prefab", GUILayout.Width(300));

        if (hasOrganizer) {
            if (settings != null) {
                if (GUILayout.Button(new GUIContent(settings, "Sound Group Organizer Settings"), EditorStyles.toolbarButton)) {
                    Selection.activeObject = organizer;
                }
            }
            EditorGUILayout.LabelField("Exists in scene", EditorStyles.boldLabel);
		} else {
	        GUI.contentColor = Color.green;
			if (GUILayout.Button(new GUIContent("Create", "Place a Sound Group Organizer prefab in the current scene."), EditorStyles.toolbarButton, GUILayout.Width(100))) {
	            var go = MasterAudio.CreateSoundGroupOrganizer();
				UndoHelper.CreateObjectForUndo(go, "Create Dynamic Sound Group Creator prefab");
			}
		}

        GUI.contentColor = Color.white;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

		
        EditorGUILayout.Separator();

        if (!Application.isPlaying) {
			EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
			GUILayout.Label("Global Settings");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();

			var newVol = EditorGUILayout.Toggle("Display dB For Volumes", MasterAudio.UseDbScaleForVolume);
			if (newVol != MasterAudio.UseDbScaleForVolume) {
				MasterAudio.UseDbScaleForVolume = newVol;
			}

			//GUILayout.Toggle(true, "Use Db for Volumes");
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
            GUILayout.Label("Utility Functions");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUI.contentColor = Color.green;
            if (GUILayout.Button(new GUIContent("Delete all unused Filter FX", "This will delete all unused Unity Audio Filter FX components in the entire MasterAudio prefab and all Sound Groups within."), EditorStyles.toolbarButton, GUILayout.Width(160))) {
                DeleteAllUnusedFilterFX();
            }

            if (maInScene) {
                GUILayout.Space(10);

                if (GUILayout.Button(new GUIContent("Upgrade MA Prefab to V3.5.5", "This will upgrade all Sound Groups in the entire MasterAudio prefab and all Sound Groups within to the latest changes, including a new script in V3.5.5."), EditorStyles.toolbarButton, GUILayout.Width(160))) {
                    UpgradeMasterAudioPrefab();
                }
            }

            GUI.contentColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        GUI.EndScrollView();
    }

    private void UpgradeMasterAudioPrefab() {
        var ma = MasterAudio.Instance;

        var added = 0;

        for (var i = 0; i < ma.transform.childCount; i++) {
            var grp = ma.transform.GetChild(i);
            for (var v = 0; v < grp.transform.childCount; v++) {
                var variation = grp.transform.GetChild(v);
                var updater = variation.GetComponent<SoundGroupVariationUpdater>();
                if (updater != null) {
                    continue;
                }

                variation.gameObject.AddComponent<SoundGroupVariationUpdater>();
                added++;
            }
        }

        DTGUIHelper.ShowAlert(string.Format("{0} Variations fixed.", added));
    }

    private void DeleteAllUnusedFilterFX() {
        var ma = MasterAudio.Instance;

        if (ma == null) {
            DTGUIHelper.ShowAlert("There is no MasterAudio prefab in this scene. Try pressing this button on a different Scene.");
            return;
        }

        var affectedVariations = new List<SoundGroupVariation>();
        var filtersToDelete = new List<Object>();

        for (var g = 0; g < ma.transform.childCount; g++) {
            var sGroup = ma.transform.GetChild(g);
            for (var v = 0; v < sGroup.childCount; v++) {
                var variation = sGroup.GetChild(v);
                var grpVar = variation.GetComponent<SoundGroupVariation>();
                if (grpVar == null) {
                    continue;
                }

                if (grpVar.LowPassFilter != null && !grpVar.LowPassFilter.enabled) {
                    if (!filtersToDelete.Contains(grpVar.LowPassFilter)) {
                        filtersToDelete.Add(grpVar.LowPassFilter);
                    }

                    if (!affectedVariations.Contains(grpVar)) {
                        affectedVariations.Add(grpVar);
                    }
                }

                if (grpVar.HighPassFilter != null && !grpVar.HighPassFilter.enabled) {
                    if (!filtersToDelete.Contains(grpVar.HighPassFilter)) {
                        filtersToDelete.Add(grpVar.HighPassFilter);
                    }

                    if (!affectedVariations.Contains(grpVar)) {
                        affectedVariations.Add(grpVar);
                    }
                }

                if (grpVar.ChorusFilter != null && !grpVar.ChorusFilter.enabled) {
                    if (!filtersToDelete.Contains(grpVar.ChorusFilter)) {
                        filtersToDelete.Add(grpVar.ChorusFilter);
                    }

                    if (!affectedVariations.Contains(grpVar)) {
                        affectedVariations.Add(grpVar);
                    }
                }

                if (grpVar.DistortionFilter != null && !grpVar.DistortionFilter.enabled) {
                    if (!filtersToDelete.Contains(grpVar.DistortionFilter)) {
                        filtersToDelete.Add(grpVar.DistortionFilter);
                    }

                    if (!affectedVariations.Contains(grpVar)) {
                        affectedVariations.Add(grpVar);
                    }
                }

                if (grpVar.EchoFilter != null && !grpVar.EchoFilter.enabled) {
                    if (!filtersToDelete.Contains(grpVar.EchoFilter)) {
                        filtersToDelete.Add(grpVar.EchoFilter);
                    }

                    if (!affectedVariations.Contains(grpVar)) {
                        affectedVariations.Add(grpVar);
                    }
                }

                if (grpVar.ReverbFilter != null && !grpVar.ReverbFilter.enabled) {
                    if (!filtersToDelete.Contains(grpVar.ReverbFilter)) {
                        filtersToDelete.Add(grpVar.ReverbFilter);
                    }

                    if (!affectedVariations.Contains(grpVar)) {
                        affectedVariations.Add(grpVar);
                    }
                }
            }
        }

        UndoHelper.RecordObjectsForUndo(affectedVariations.ToArray(), "delete all unused Filter FX Components");

        for (var i = 0; i < filtersToDelete.Count; i++) {
            DestroyImmediate(filtersToDelete[i]);
        }

        DTGUIHelper.ShowAlert(string.Format("{0} Filter FX Components deleted.", filtersToDelete.Count));
    }
}
