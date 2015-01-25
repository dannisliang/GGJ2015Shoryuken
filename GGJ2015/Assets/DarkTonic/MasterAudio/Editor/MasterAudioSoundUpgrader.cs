using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class MasterAudioSoundUpgrader : EditorWindow {
	private Vector2 scrollPos = Vector2.zero;
	private int audioSources = -1;
	
    [MenuItem("Window/Master Audio Sound Upgrader")]
    static void Init() {
        EditorWindow.GetWindow(typeof(MasterAudioSoundUpgrader));
    }

    void OnGUI() {
        scrollPos = GUI.BeginScrollView(
                new Rect(0, 0, position.width, position.height),
                scrollPos, 
                new Rect(0, 0, 600, 210)
        );

		
        if (MasterAudioInspectorResources.logoTexture != null) {
            DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.logoTexture);
        }
		
		if (Application.isPlaying) {
	        DTGUIHelper.ShowLargeBarAlert("This window can only be used in edit mode.");
		} else {
	        DTGUIHelper.ShowColorWarning("This window will help you prepare a project that has existing audio for switching over to Master Audio.");
	        DTGUIHelper.ShowColorWarning("All Audio Source components should be created by Master Audio only. Let's remove all your old ones.");
	        DTGUIHelper.ShowLargeBarAlert("For each Scene, open the Scene, then go through the steps below to locate & delete items.");
			
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Step 1", EditorStyles.boldLabel);
			
	        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
	        GUI.contentColor = Color.green;
	        if (GUILayout.Button(new GUIContent("Find Audio Sources In Scene"), EditorStyles.toolbarButton, GUILayout.Width(200))) {
				var audSources = GetNonMAAudioSources();			
				audioSources = audSources.Count;
				
				if (audioSources > 0) {
					Selection.objects = audSources.ToArray();
				}
				 
				if (audioSources == 0) {
					DTGUIHelper.ShowAlert("You have zero AudioSources in your Scene. You are finished.");
				} else {
					DTGUIHelper.ShowAlert(audSources.Count + " AudioSource(s) found and selected in the Hierarchy. Please take note of what game objects these are, so you can add sound to them later with Master Audio.");
				}
			}
	        GUI.contentColor = Color.white;
			
			if (audioSources < 0) {
				GUI.contentColor = Color.cyan;
				GUILayout.Label("Click button to find Audio Sources.");
			} else if (audioSources == 0) {
				GUI.contentColor = Color.green;
				GUILayout.Label("No Audio Sources! You are finished.");
			} else {
				GUI.contentColor = Color.red;
				GUILayout.Label(audioSources.ToString() + " Audio Source(s) selected. Take note of them and go to step 2.");
			}
			GUI.contentColor = Color.white; 
			
			EditorGUILayout.EndHorizontal();
	
			
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Step 2", EditorStyles.boldLabel);
			
	        EditorGUILayout.BeginHorizontal(EditorStyles.objectFieldThumb);
	        GUI.contentColor = Color.green;
	        if (GUILayout.Button(new GUIContent("Delete Audio Sources In Scene"), EditorStyles.toolbarButton, GUILayout.Width(200))) {
				var audSources = GetNonMAAudioSources();			
				audioSources = audSources.Count;
				
				if (audioSources == 0) {
					DTGUIHelper.ShowAlert("You have zero AudioSources in your Scene. You are finished.");
					audioSources = 0;
				} else {
					DeleteAudioSources();
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		
        GUI.EndScrollView();
    }
	
	private List<GameObject> GetNonMAAudioSources() {
		var sources = GameObject.FindObjectsOfType(typeof(AudioSource));
		
		List<GameObject> audSources = new List<GameObject>();
		for (var i = 0; i < sources.Length; i++) {
			var src = (AudioSource) sources[i];
			
			var plController = src.GetComponent<PlaylistController>();
			if (plController != null) {
				continue;
			}
			
			var variation = src.GetComponent<SoundGroupVariation>();
			if (variation != null) {
				continue;
			}

			var dynVariation = src.GetComponent<DynamicGroupVariation>();
			if (dynVariation != null) {
				continue;
			}
			
			audSources.Add(src.gameObject);
		}
		
		return audSources;
	}
	
	private void DeleteAudioSources() {
		Selection.objects = new Object[] { };

		var sources = GetNonMAAudioSources();
		
		var destroyed = 0;
		for (var i = 0; i < sources.Count; i++) {
			var aud = sources[i];
			GameObject.DestroyImmediate(aud.GetComponent<AudioSource>());
			destroyed++;
		}
		
		DTGUIHelper.ShowAlert(destroyed + " Audio Source(s) destroyed.");
		audioSources = 0;
	}
}
