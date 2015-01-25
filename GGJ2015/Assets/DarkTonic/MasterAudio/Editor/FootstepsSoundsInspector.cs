using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(FootstepSounds))]
[CanEditMultipleObjects]
public class FootstepsSoundsInspector : Editor {
    private bool isDirty = false;
	private FootstepSounds sounds = null;
	private List<string> groupNames = null;

	public override void OnInspectorGUI() {
        EditorGUIUtility.LookLikeControls();

        MasterAudio.Instance = null;

        MasterAudio ma = MasterAudio.Instance;
        var maInScene = ma != null;
        
        if (maInScene) {
            DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.logoTexture);
			groupNames = ma.GroupNames;
		}

		isDirty = false;

        sounds = (FootstepSounds)target;

		var newEvent = (FootstepSounds.FootstepTriggerMode) EditorGUILayout.EnumPopup("Event Used", sounds.footstepEvent);
		if (newEvent != sounds.footstepEvent) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Event Used");
			sounds.footstepEvent = newEvent;
		}

		if (sounds.footstepEvent == FootstepSounds.FootstepTriggerMode.None) {
			DTGUIHelper.ShowRedError("No sound will be made when Event Used is set to None.");
			return;
		}

		EditorGUILayout.BeginHorizontal();
		GUI.contentColor = Color.green;
		GUILayout.Space(10);
		if (GUILayout.Button("Add Footstep Sound", EditorStyles.toolbarButton, GUILayout.Width(130))) {
			AddFootstepSound();
		}

		if (sounds.footstepGroups.Count > 0) {
			GUILayout.Space(10);
			if (GUILayout.Button(new GUIContent("Delete Footstep Sound", "Delete the bottom Footstep Sound"), EditorStyles.toolbarButton, GUILayout.Width(130))) {
				DeleteFootstepSound();
			}
			var buttonText = "Collapse All";
			bool allCollapsed = true;
			
			for (var j = 0; j < sounds.footstepGroups.Count; j++) {
				if (sounds.footstepGroups[j].isExpanded) {
					allCollapsed = false;
					break;
				}
			}
			
			if (allCollapsed) {
				buttonText = "Expand All";
			}
			
			GUILayout.Space(10);
			if (GUILayout.Button(new GUIContent(buttonText), EditorStyles.toolbarButton, GUILayout.Width(100))) {
				isDirty = true;
				ExpandCollapseAll(allCollapsed);
			}
		}

		GUI.contentColor = Color.white;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Separator();
		
		var newRetrigger = (EventSounds.RetriggerLimMode)EditorGUILayout.EnumPopup("Retrigger Limit Mode", sounds.retriggerLimitMode);
		if (newRetrigger != sounds.retriggerLimitMode) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Retrigger Limit Mode");
			sounds.retriggerLimitMode = newRetrigger;
		}
		
		EditorGUI.indentLevel = 1;
		switch (sounds.retriggerLimitMode) {
			case EventSounds.RetriggerLimMode.FrameBased:
				var newFrm = EditorGUILayout.IntSlider("Min Frames Between", sounds.limitPerXFrm, 0, 10000);
				if (newFrm != sounds.limitPerXFrm) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Min Frames Between");
					sounds.limitPerXFrm = newFrm;
				}
				break;
			case EventSounds.RetriggerLimMode.TimeBased:
				var newSec = EditorGUILayout.Slider("Min Seconds Between", sounds.limitPerXSec, 0f, 10000f);
				if (newSec != sounds.limitPerXSec) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Min Seconds Between");
					sounds.limitPerXSec = newSec;
				}
				break;
		}
		
		EditorGUI.indentLevel = 0;
		if (sounds.footstepGroups.Count == 0) {
			DTGUIHelper.ShowRedError("You have no Footstep Sounds configured.");
		} 
		for (var f = 0; f < sounds.footstepGroups.Count; f++) {
			var step = sounds.footstepGroups[f];

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			var isExpanded = DTGUIHelper.Foldout(step.isExpanded, "Footstep Sound #" + (f + 1));
			if (isExpanded != step.isExpanded) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "toggle expand Footstep Sound");
				step.isExpanded = isExpanded;
			}
			EditorGUILayout.EndHorizontal();

			if (step.isExpanded) {
				var newUseLayers = EditorGUILayout.BeginToggleGroup("Layer filters", step.useLayerFilter);
				if (newUseLayers != step.useLayerFilter) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "toggle Layer filters");
					step.useLayerFilter = newUseLayers;
				}

				if (step.useLayerFilter) {
					for (var i = 0; i < step.matchingLayers.Count; i++) {
						var newLayer = EditorGUILayout.LayerField("Layer Match " + (i + 1), step.matchingLayers[i]);
						if (newLayer != step.matchingLayers[i]) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Layer filter");
							step.matchingLayers[i] = newLayer;
						}
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(24);
					
					GUI.contentColor = Color.green;
					if (GUILayout.Button(new GUIContent("Add", "Click to add a layer match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "add Layer filter");
						step.matchingLayers.Add(0);
					}
					if (step.matchingLayers.Count > 1) {
						GUILayout.Space(10);
						if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last layer match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "remove Layer filter");
							step.matchingLayers.RemoveAt(step.matchingLayers.Count - 1);
						}
					}
					GUI.contentColor = Color.white;
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndToggleGroup();
				
				var newTagFilter = EditorGUILayout.BeginToggleGroup("Tag filter", step.useTagFilter);
				if (newTagFilter != step.useTagFilter) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "toggle Tag filter");
					step.useTagFilter = newTagFilter;
				}
				
				if (step.useTagFilter) {
					for (var i = 0; i < step.matchingTags.Count; i++) {
						var newTag = EditorGUILayout.TagField("Tag Match " + (i + 1), step.matchingTags[i]);
						if (newTag != step.matchingTags[i]) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Tag filter");
							step.matchingTags[i] = newTag;
						}
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(24);
					GUI.contentColor = Color.green;
					if (GUILayout.Button(new GUIContent("Add", "Click to add a tag match at the end"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "Add Tag filter");
						step.matchingTags.Add("Untagged");
					}
					if (step.matchingTags.Count > 1) {
						GUILayout.Space(10);
						if (GUILayout.Button(new GUIContent("Remove", "Click to remove the last tag match"), EditorStyles.toolbarButton, GUILayout.Width(60))) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "remove Tag filter");
							step.matchingTags.RemoveAt(step.matchingLayers.Count - 1);
						}
					}
					GUI.contentColor = Color.white;
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndToggleGroup();

				EditorGUI.indentLevel = 0;

				if (maInScene) {
					var existingIndex = groupNames.IndexOf(step.soundType);
					
					int? groupIndex = null;
					
					var noGroup = false;
					var noMatch = false;
					
					if (existingIndex >= 1) {
						groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, groupNames.ToArray());
						if (existingIndex == 1) {
							noGroup = true;
						} 
					} else if (existingIndex == -1 && step.soundType == MasterAudio.NO_GROUP_NAME) {
						groupIndex = EditorGUILayout.Popup("Sound Group", existingIndex, groupNames.ToArray());
					} else { // non-match
						noMatch = true;
						var newSound = EditorGUILayout.TextField("Sound Group", step.soundType);
						if (newSound != step.soundType) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Sound Group");
							step.soundType = newSound;
						}
						
						var newIndex = EditorGUILayout.Popup("All Sound Groups", -1, groupNames.ToArray());
						if (newIndex >= 0) {
							groupIndex = newIndex;
						}
					}
					
					if (noGroup) {
						DTGUIHelper.ShowRedError("No Sound Group specified. Footstep will not sound.");
					} else if (noMatch) {
						DTGUIHelper.ShowRedError("Sound Group found no match. Type in or choose one.");
					}
					
					if (groupIndex.HasValue) {
						if (existingIndex != groupIndex.Value) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Sound Group");
						}
						if (groupIndex.Value == -1) {
							step.soundType = MasterAudio.NO_GROUP_NAME;
						} else {
							step.soundType = groupNames[groupIndex.Value];
						}
					}
				} else {
					var newSType = EditorGUILayout.TextField("Sound Group", step.soundType);
					if (newSType != step.soundType) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Sound Group");
						step.soundType = newSType;
					}
				}

				var newVarType = (EventSounds.VariationType)EditorGUILayout.EnumPopup("Variation Mode", step.variationType);
				if (newVarType != step.variationType) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Variation Mode");
					step.variationType = newVarType;
				}
				
				if (step.variationType == EventSounds.VariationType.PlaySpecific) {
					var newVarName = EditorGUILayout.TextField("Variation Name", step.variationName);
					if (newVarName != step.variationName) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Variation Name");
						step.variationName = newVarName;
					}
					
					if (string.IsNullOrEmpty(step.variationName)) {
						DTGUIHelper.ShowRedError("Variation Name is empty. No sound will play.");
					}
				}

				var newVol = DTGUIHelper.DisplayVolumeField(step.volume, DTGUIHelper.VolumeFieldType.None, false, 0f, true);
				if (newVol != step.volume) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Volume");
					step.volume = newVol;
				}
				
				var newFixedPitch = EditorGUILayout.Toggle("Override pitch?", step.useFixedPitch);
				if (newFixedPitch != step.useFixedPitch) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "toggle Override pitch");
					step.useFixedPitch = newFixedPitch;
				}
				if (step.useFixedPitch) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Pitch");
					step.pitch = EditorGUILayout.Slider("Pitch", step.pitch, -3f, 3f);
				}
				
				var newDelay = EditorGUILayout.Slider("Delay Sound (sec)", step.delaySound, 0f, 10f);
				if (newDelay != step.delaySound) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "change Delay Sound");
					step.delaySound = newDelay;
				}
			}
		}
		
		if (GUI.changed || isDirty) {
			EditorUtility.SetDirty(target);
		}
		
		//DrawDefaultInspector();
	}
	
	private void AddFootstepSound() {
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "Add Footstep Sound");
		sounds.footstepGroups.Add(new FootstepGroup());
	}

	private void DeleteFootstepSound() {
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "Delete Footstep Sound");
		sounds.footstepGroups.RemoveAt(sounds.footstepGroups.Count - 1);
	}

	private void ExpandCollapseAll(bool expand) {
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, sounds, "toggle Expand / Collapse Footstep Groups");
		for (var i = 0; i < sounds.footstepGroups.Count; i++) {
			sounds.footstepGroups[i].isExpanded = expand;
		}
	}
}
