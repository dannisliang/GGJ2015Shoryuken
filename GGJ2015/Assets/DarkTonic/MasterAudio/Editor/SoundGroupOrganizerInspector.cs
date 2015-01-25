using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(SoundGroupOrganizer))]
public class SoundGroupOrganizerInspector : Editor {
	private SoundGroupOrganizer organizer = null;
	private List<DynamicSoundGroup> _groups;
	private bool isDirty  = false;
    private GameObject _previewer;

	public override void OnInspectorGUI() {
		isDirty = false;

		if (MasterAudioInspectorResources.logoTexture != null) {
			DTGUIHelper.ShowHeaderTexture(MasterAudioInspectorResources.logoTexture);
		}

		organizer = (SoundGroupOrganizer)target;
		
		if (Application.isPlaying) {
			DTGUIHelper.ShowRedError("Sound Group Inspector cannot be used at runtime. Press stop to use it.");
			return;
		}
		
		_groups = ScanForGroups();
		
		var isInProjectView = DTGUIHelper.IsPrefabInProjectView(organizer);

        _previewer = organizer.gameObject;

		if (MasterAudio.Instance == null) {
			var newLang = (SystemLanguage) EditorGUILayout.EnumPopup(new GUIContent("Preview Language", "This setting is only used (and visible) to choose the previewing language when there's no Master Audio prefab in the Scene (language settings are grabbed from there normally). This should only happen when you're using a Master Audio prefab from a previous Scene in persistent mode."), organizer.previewLanguage);
			if (newLang != organizer.previewLanguage) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Preview Language");
				organizer.previewLanguage = newLang;
			}
		}
		
		var ma = MasterAudio.Instance;
		
		var sources = new List<GameObject>();
		if (ma != null) {
			sources.Add (ma.gameObject);
		}
		
		var dgscs = GameObject.FindObjectsOfType(typeof(DynamicSoundGroupCreator));
		for (var i = 0; i < dgscs.Length; i++) {
			var dsgc = (DynamicSoundGroupCreator) dgscs[i];
			sources.Add(dsgc.gameObject);
		}

		var sourceNames = new List<string>();
		for (var i = 0; i < sources.Count; i++) {
			sourceNames.Add(sources[i].name);
		}
		
		var scannedDest = false;
		
		var newType = (SoundGroupOrganizer.MAItemType) EditorGUILayout.EnumPopup("Item Type", organizer.itemType);
		if (newType != organizer.itemType) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Item Type");
			organizer.itemType = newType;
		}			
		
		var newMode = (SoundGroupOrganizer.TransferMode) EditorGUILayout.EnumPopup("Transfer Mode", organizer.transMode);
		if (newMode != organizer.transMode) {
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Transfer Mode");
			organizer.transMode = newMode;
			
			RescanDestinationGroups();
			scannedDest = true;
		}
		
		if (!scannedDest && organizer.selectedDestSoundGroups.Count == 0) {
			RescanDestinationGroups();
			scannedDest = true;
		}
		
		var shouldRescanGroups = false; 
		var hasRescannedGroups = false;
		var shouldRescanEvents = false;
		var hasRescannedEvents = false;

		if (organizer.itemType == SoundGroupOrganizer.MAItemType.SoundGroups) {
			switch(organizer.transMode) {
				case SoundGroupOrganizer.TransferMode.Import:
					if (sources.Count == 0) {
						DTGUIHelper.ShowRedError("You have no Master Audio or Dynamic Sound Group Creator prefabs in this Scene. Can't import.");
					} else if (isInProjectView) {
						DTGUIHelper.ShowRedError("You are in Project View and can't import. Create this prefab with Master Audio Manager.");
					} else {
						var srcIndex = sources.IndexOf(organizer.sourceObject);	
						if (srcIndex < 0) {
							srcIndex = 0;
						}
					
						var newIndex = EditorGUILayout.Popup("Source Object", srcIndex, sourceNames.ToArray());
						if (newIndex != srcIndex) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Source Object");
						}
						var newSource = sources[newIndex];
						if (!hasRescannedGroups && newSource != organizer.sourceObject || organizer.selectedSourceSoundGroups.Count == 0) {
							if (RescanSourceGroups()) {						
								hasRescannedGroups = true;
							}
						} 
						organizer.sourceObject = newSource;
					
						if (!hasRescannedGroups && organizer.selectedSourceSoundGroups.Count != organizer.sourceObject.transform.childCount) {
							if (RescanSourceGroups()) {
								hasRescannedGroups = true;
							}
						}

						if (organizer.sourceObject != null) {
							if (organizer.selectedSourceSoundGroups.Count > 0) {
								DTGUIHelper.ShowLargeBarAlert("Check Groups to Import below and click 'Import'");
							} else {
								DTGUIHelper.ShowRedError("Source Object has no Groups to import.");
							}
							
							EditorGUI.indentLevel = 1;
							
							for (var i = 0; i < organizer.selectedSourceSoundGroups.Count; i++) {
								var aGroup = organizer.selectedSourceSoundGroups[i];
								if (!hasRescannedGroups && aGroup._go == null) {
									shouldRescanGroups = true;
									continue;
								} 
							
								var newSel = EditorGUILayout.Toggle(aGroup._go.name, aGroup._isSelected);
								if (newSel != aGroup._isSelected) {
									UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "toggle Sound Group selection");
									aGroup._isSelected = newSel;
								}
							}
						}
						
						if (!hasRescannedGroups && shouldRescanGroups) {				
							if (RescanSourceGroups()) {
								hasRescannedGroups = true;
							}
						}
					
						if (organizer.selectedSourceSoundGroups.Count > 0) {
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(10);
							GUI.contentColor = Color.green;
							if (GUILayout.Button(new GUIContent("Import", "Import Selected Groups"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								ImportSelectedGroups();
							}
	
							GUI.contentColor = Color.yellow;
							GUILayout.Space(10);
							if (GUILayout.Button(new GUIContent("Check All", "Check all Groups above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								CheckUncheckAllSourceGroups(true);
							}
							GUILayout.Space(10);
							if (GUILayout.Button(new GUIContent("Uncheck All", "Uncheck all Groups above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								CheckUncheckAllSourceGroups(false);
							}
							EditorGUILayout.EndHorizontal();
						}
					}
					break;
				case SoundGroupOrganizer.TransferMode.Export:
					if (_groups.Count == 0) {
						DTGUIHelper.ShowRedError("You have no Groups to export. Import or create some first.");
					} else if (sources.Count == 0) {
						DTGUIHelper.ShowRedError("You have no Master Audio or Dynamic Sound Group Creator prefabs in this Scene to export to.");
					} else {
						var destIndex = sources.IndexOf(organizer.destObject);	
						if (destIndex < 0) {
							destIndex = 0;
						}
					
						var newIndex = EditorGUILayout.Popup("Destination Object", destIndex, sourceNames.ToArray());
						if (newIndex != destIndex) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Destination Object");
						}
						var newDest = sources[newIndex];
					
						organizer.destObject = newDest;
						DTGUIHelper.ShowLargeBarAlert("Check Groups to export (same as Group Control below) and click 'Export'");
					
						if (organizer.destObject != null) {
							EditorGUI.indentLevel = 1;
							
							for (var i = 0; i < organizer.selectedDestSoundGroups.Count; i++) {
								var aGroup = organizer.selectedDestSoundGroups[i];
								if (!hasRescannedGroups && aGroup._go == null) {
									shouldRescanGroups = true;
									continue;
								} 
							
								var newSel = EditorGUILayout.Toggle(aGroup._go.name, aGroup._isSelected);
								if (newSel != aGroup._isSelected) {
									UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "toggle Sound Group selection");
									aGroup._isSelected = newSel;
								}
							}
						}
					
						if (!hasRescannedGroups && shouldRescanGroups) {
							RescanDestinationGroups();
							hasRescannedGroups = true;
						}
					
						if (organizer.selectedDestSoundGroups.Count > 0) {
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(10);
							GUI.contentColor = Color.green;
							if (GUILayout.Button(new GUIContent("Export", "Export Selected Groups"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								ExportSelectedGroups();
							}
	
							GUI.contentColor = Color.yellow;
							GUILayout.Space(10);
							if (GUILayout.Button(new GUIContent("Check All", "Check all Groups above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								CheckUncheckAllDestGroups(true);
							}
							GUILayout.Space(10);
							if (GUILayout.Button(new GUIContent("Uncheck All", "Uncheck all Groups above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								CheckUncheckAllDestGroups(false);
							}
							EditorGUILayout.EndHorizontal();
						}
					}
	
					break;
			}
		} else {
			// custom events
			switch(organizer.transMode) {
				case SoundGroupOrganizer.TransferMode.Import:
					if (sources.Count == 0) {
						DTGUIHelper.ShowRedError("You have no Master Audio or Dynamic Sound Group Creator prefabs in this Scene. Can't import.");
					} else if (isInProjectView) {
						DTGUIHelper.ShowRedError("You are in Project View and can't import. Create this prefab with Master Audio Manager.");
					} else {
					
						var srcMa = organizer.sourceObject.GetComponent<MasterAudio>();
						var srcDgsc = organizer.sourceObject.GetComponent<DynamicSoundGroupCreator>();
						
						var isSourceMA = srcMa != null;
						var isSourceDGSC = srcDgsc != null;
						
						List<CustomEvent> sourceEvents = null;
						
						if (isSourceMA) {
							sourceEvents = srcMa.customEvents;
						} else if (isSourceDGSC) {
							sourceEvents = srcDgsc.customEventsToCreate;
						}
					
						var srcIndex = sources.IndexOf(organizer.sourceObject);	
						if (srcIndex < 0) {
							srcIndex = 0;
						}
					
						var newIndex = EditorGUILayout.Popup("Source Object", srcIndex, sourceNames.ToArray());
						if (newIndex != srcIndex) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Source Object");
						}
						var newSource = sources[newIndex];
						if (!hasRescannedEvents && newSource != organizer.sourceObject || organizer.selectedSourceCustomEvents.Count == 0) {
							if (RescanSourceEvents(sourceEvents)) {						
								hasRescannedEvents = true;
							}
						} 
						organizer.sourceObject = newSource;
					
						if (!hasRescannedEvents && organizer.selectedSourceCustomEvents.Count != sourceEvents.Count) {
							if (RescanSourceEvents(sourceEvents)) {
								hasRescannedEvents = true;
							}
						}
						
						if (organizer.sourceObject != null) {
							if (organizer.selectedSourceCustomEvents.Count > 0) {	
								DTGUIHelper.ShowLargeBarAlert("Check Custom Events to Import below and click 'Import'");
							} else {
								DTGUIHelper.ShowRedError("Source Object has no Custom Events to import.");
							}
						
							EditorGUI.indentLevel = 1;
							
							for (var i = 0; i < organizer.selectedSourceCustomEvents.Count; i++) {
								var aEvent = organizer.selectedSourceCustomEvents[i];
								if (!hasRescannedEvents && aEvent._event == null) {
									shouldRescanEvents = true;
									continue;
								} 
							
								var newSel = EditorGUILayout.Toggle(aEvent._event.EventName, aEvent._isSelected);
								if (newSel != aEvent._isSelected) {
									UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "toggle Custom Event selection");
									aEvent._isSelected = newSel;
								}
							}
						}
						
						if (!hasRescannedEvents && shouldRescanEvents) {
							RescanDestinationEvents(); 
							hasRescannedEvents = true;
						}

						if (organizer.selectedSourceCustomEvents.Count > 0) {
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(10);
							GUI.contentColor = Color.green;
							if (GUILayout.Button(new GUIContent("Import", "Import Selected Events"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								ImportSelectedEvents();
								RescanDestinationEvents();
							}
	
							GUI.contentColor = Color.yellow;
							GUILayout.Space(10);
							if (GUILayout.Button(new GUIContent("Check All", "Check all Events above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								CheckUncheckAllSourceEvents(true);
							}
							GUILayout.Space(10);
							if (GUILayout.Button(new GUIContent("Uncheck All", "Uncheck all Events above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								CheckUncheckAllSourceEvents(false);
							}
							EditorGUILayout.EndHorizontal();
						}
					}
					break;
				case SoundGroupOrganizer.TransferMode.Export:
					if (organizer.customEvents.Count == 0) {
						DTGUIHelper.ShowRedError("You have no Custom Events to export. Import or create some first.");
					} else if (sources.Count == 0) {
						DTGUIHelper.ShowRedError("You have no Master Audio or Dynamic Sound Group Creator prefabs in this Scene to export to.");
					} else {
						var destIndex = sources.IndexOf(organizer.destObject);	
						if (destIndex < 0) {
							destIndex = 0;
						}

						var newIndex = EditorGUILayout.Popup("Destination Object", destIndex, sourceNames.ToArray());
						if (newIndex != destIndex) {
							UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Destination Object");
						}
						var newDest = sources[newIndex];
					
						organizer.destObject = newDest;

						if (organizer.destObject != null) {
							if (organizer.selectedDestCustomEvents.Count == 0) {
								DTGUIHelper.ShowRedError("You have no Custom Events to export");			
							} else {
								DTGUIHelper.ShowLargeBarAlert("Check Custom Events to export (same as Custom Events below) and click 'Export'");
							}

							EditorGUI.indentLevel = 1;

							if (organizer.selectedDestCustomEvents.Count != organizer.customEvents.Count) {
								shouldRescanEvents = true;
							}
							if (!hasRescannedEvents && shouldRescanEvents) {				
								RescanDestinationEvents();
								hasRescannedEvents = true;
							}

							for (var i = 0; i < organizer.selectedDestCustomEvents.Count; i++) {
								var aEvent = organizer.selectedDestCustomEvents[i];

								var newSel = EditorGUILayout.Toggle(aEvent._event.EventName, aEvent._isSelected);
								if (newSel != aEvent._isSelected) {
									UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "toggle Custom Event selection");
									aEvent._isSelected = newSel;
								}
							}
						}

						if (organizer.selectedDestCustomEvents.Count > 0) {
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(10);
							GUI.contentColor = Color.green;
							if (GUILayout.Button(new GUIContent("Export", "Export Selected Custom Events"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								ExportSelectedEvents();
							}
	 
							GUI.contentColor = Color.yellow;
							GUILayout.Space(10);
							if (GUILayout.Button(new GUIContent("Check All", "Check all Custom Events above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								CheckUncheckAllDestEvents(true);
							}
							GUILayout.Space(10);
							if (GUILayout.Button(new GUIContent("Uncheck All", "Uncheck all Custom Events above"), EditorStyles.toolbarButton, GUILayout.Width(80))) {
								CheckUncheckAllDestEvents(false);
							}
							EditorGUILayout.EndHorizontal();
						}
					}  
	
					break;
			}			
		}
		
		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		
		GUI.contentColor = Color.white;
		var sliderIndicatorChars = 6;
		var sliderWidth = 40;
		
		if (MasterAudio.UseDbScaleForVolume) {
			sliderIndicatorChars = 9;
			sliderWidth = 56;
		}
		
		EditorGUI.indentLevel = 0;
        if (organizer.itemType == SoundGroupOrganizer.MAItemType.SoundGroups) {
			EditorGUILayout.LabelField("Group Control", EditorStyles.miniBoldLabel);
			var newDragMode = (MasterAudio.DragGroupMode)EditorGUILayout.EnumPopup("Bulk Creation Mode", organizer.curDragGroupMode);
			if (newDragMode != organizer.curDragGroupMode) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Bulk Creation Mode");
				organizer.curDragGroupMode = newDragMode;
			}
			
			var bulkMode = (MasterAudio.AudioLocation)EditorGUILayout.EnumPopup("Variation Create Mode", organizer.bulkVariationMode);
			if (bulkMode != organizer.bulkVariationMode) {
				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Variation Mode");
				organizer.bulkVariationMode = bulkMode;
			}
	
			if (_groups.Count > 0) {
				var newUseTextGroupFilter = EditorGUILayout.Toggle("Use Text Group Filter", organizer.useTextGroupFilter);
				if (newUseTextGroupFilter != organizer.useTextGroupFilter) {
					UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "toggle Use Text Group Filter");
					organizer.useTextGroupFilter = newUseTextGroupFilter;
				}
				
				if (organizer.useTextGroupFilter) {
					EditorGUI.indentLevel = 1;
					
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(10);
					GUILayout.Label("Text Group Filter", GUILayout.Width(140));
					var newTextFilter = GUILayout.TextField(organizer.textGroupFilter, GUILayout.Width(180));
					if (newTextFilter != organizer.textGroupFilter) {
						UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Text Group Filter");
						organizer.textGroupFilter = newTextFilter;
					}
					GUILayout.Space(10);
					GUI.contentColor = Color.green;
					if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(70))) {
						organizer.textGroupFilter = string.Empty;
					}
					GUI.contentColor = Color.white;
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.Separator();
				}
			}
	
			EditorGUI.indentLevel = 0;
	
			// create groups start
			EditorGUILayout.BeginVertical();
			var aEvent = Event.current;
			
			var groupAdded = false;
			
			if (isInProjectView) {
				DTGUIHelper.ShowLargeBarAlert("*You are in Project View and cannot create or delete Groups.");
				DTGUIHelper.ShowRedError("*Create this prefab With Master Audio Manager. Do not drag into Scene!");
			} else {
				//DTGUIHelper.ShowRedError("Make sure this prefab is not in a gameplay Scene. Use a special Sandbox Scene.");
				GUI.color = Color.yellow;
				
				var dragAreaGroup = GUILayoutUtility.GetRect(0f, 35f, GUILayout.ExpandWidth(true));
				GUI.Box(dragAreaGroup, "Drag Audio clips here to create groups!");
	
				switch (aEvent.type) {
					case EventType.DragUpdated:
					case EventType.DragPerform:
						if (!dragAreaGroup.Contains(aEvent.mousePosition)) {
							break;
						}
					
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					
						if (aEvent.type == EventType.DragPerform) {
							DragAndDrop.AcceptDrag();
							
							Transform groupInfo = null;
							
							var clips = new List<AudioClip>();
						
							foreach (var dragged in DragAndDrop.objectReferences) {
								var aClip = dragged as AudioClip;
								if (aClip == null) {
									continue;
								}
								
								clips.Add(aClip);
							}
						
							clips.Sort(delegate(AudioClip x, AudioClip y) {
								return x.name.CompareTo(y.name);
							});
						
							for (var i = 0; i < clips.Count; i++) {
								var aClip = clips[i];
								if (organizer.curDragGroupMode == MasterAudio.DragGroupMode.OneGroupPerClip) {
									CreateGroup(aClip);
								} else {
									if (groupInfo == null) { // one group with variations
										groupInfo = CreateGroup(aClip);
									} else {
										CreateVariation(groupInfo, aClip);
									}
								}
								groupAdded = true;
							
								isDirty = true;
							}
						}
						Event.current.Use();
						break;
				}
			}
	
			EditorGUILayout.EndVertical();
			// create groups end
			 
			if (groupAdded) {
				RescanDestinationGroups();
			}
			
			var filteredGroups = new List<DynamicSoundGroup>();
			filteredGroups.AddRange(_groups);
	
			if (organizer.useTextGroupFilter) {
				if (!string.IsNullOrEmpty(organizer.textGroupFilter)) {
					filteredGroups.RemoveAll(delegate(DynamicSoundGroup obj) {
						return !obj.transform.name.ToLower().Contains(organizer.textGroupFilter.ToLower());
					});
				}
			}
	
			if (_groups.Count == 0) {
				DTGUIHelper.ShowLargeBarAlert("You currently have no Sound Groups created.");
			} else {
				var groupsFiltered = _groups.Count - filteredGroups.Count;
				if (groupsFiltered > 0) {
					DTGUIHelper.ShowLargeBarAlert(string.Format("{0} Group(s) filtered out.", groupsFiltered));
				}
			}
			
			int? indexToDelete = null;
			
			GUI.color = Color.white;
	
			filteredGroups.Sort(delegate(DynamicSoundGroup x, DynamicSoundGroup y) {
				return x.name.CompareTo(y.name);
			});
	
			for (var i = 0; i < filteredGroups.Count; i++) {
				var aGroup = filteredGroups[i];
	
				var groupDirty = false;
	
				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				GUILayout.Label(aGroup.name, GUILayout.Width(150));
	
				GUILayout.FlexibleSpace();
				
				GUI.contentColor = Color.white;
				GUI.color = Color.cyan;
				
				GUI.color = Color.white;
				
				GUI.contentColor = Color.green;
				GUILayout.TextField(DTGUIHelper.DisplayVolumeNumber(aGroup.groupMasterVolume, sliderIndicatorChars), sliderIndicatorChars, EditorStyles.miniLabel, GUILayout.Width(sliderWidth));
				
				var newVol = DTGUIHelper.DisplayVolumeField(aGroup.groupMasterVolume, DTGUIHelper.VolumeFieldType.DynamicMixerGroup, false);
				if (newVol != aGroup.groupMasterVolume) {
					UndoHelper.RecordObjectPropertyForUndo(ref groupDirty, aGroup, "change Group Volume");
					aGroup.groupMasterVolume = newVol;
				}
				
				GUI.contentColor = Color.white;
				
				var buttonPressed = DTGUIHelper.AddDynamicGroupButtons(organizer); 
				EditorGUILayout.EndHorizontal();
				
				switch (buttonPressed) {
				case DTGUIHelper.DTFunctionButtons.Go:
					Selection.activeGameObject = aGroup.gameObject;
					break;
				case DTGUIHelper.DTFunctionButtons.Remove:
					indexToDelete = i;
					break;
				case DTGUIHelper.DTFunctionButtons.Play:
					PreviewGroup(aGroup);
					break;
				case DTGUIHelper.DTFunctionButtons.Stop:
	                StopPreviewer();
					break;
				}
				
				if (groupDirty) {
					EditorUtility.SetDirty(aGroup);
				}
			}
	
			if (indexToDelete.HasValue) {
				UndoHelper.DestroyForUndo(filteredGroups[indexToDelete.Value].gameObject);
			}
	
			if (filteredGroups.Count > 0) {
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(6);
				
				GUI.contentColor = Color.green;
				if (GUILayout.Button(new GUIContent("Max Group Volumes", "Reset all group volumes to full"), EditorStyles.toolbarButton, GUILayout.Width(120))) {
					UndoHelper.RecordObjectsForUndo(filteredGroups.ToArray(), "Max Group Volumes");
					
					for (var l = 0; l < filteredGroups.Count; l++) {
						var aGroup = filteredGroups[l];
						aGroup.groupMasterVolume = 1f;
					}
				}
				GUI.contentColor = Color.white;
				EditorGUILayout.EndHorizontal();
			}
		} else {
			// custom events
	        EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField("Custom Event Control", EditorStyles.miniBoldLabel);
	
            var newEvent = EditorGUILayout.TextField("New Event Name", organizer.newEventName);
            if (newEvent != organizer.newEventName) {
                UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change New Event Name");
                organizer.newEventName = newEvent;
            }
	
	            EditorGUILayout.BeginHorizontal();
	            GUILayout.Space(10);
	            GUI.contentColor = Color.green;
	            if (GUILayout.Button("Create New Event", EditorStyles.toolbarButton, GUILayout.Width(100))) {
	                CreateCustomEvent(organizer.newEventName);
	            }
	            GUILayout.Space(10);
	            GUI.contentColor = Color.yellow;
	
	            var hasExpanded = false;
	            for (var i = 0; i < organizer.customEvents.Count; i++) {
	                if (organizer.customEvents[i].eventExpanded) {
	                    hasExpanded = true;
	                    break;
	                }
	            }
	
	            var buttonText = hasExpanded ? "Collapse All" : "Expand All";
	
	            if (GUILayout.Button(buttonText, EditorStyles.toolbarButton, GUILayout.Width(100))) {
	                ExpandCollapseCustomEvents(!hasExpanded);
	            }
	            GUILayout.Space(10);
	            if (GUILayout.Button("Sort Alpha", EditorStyles.toolbarButton, GUILayout.Width(100))) {
	                SortCustomEvents();
	            }
	
	            GUI.contentColor = Color.white;
	            EditorGUILayout.EndHorizontal();
	
	            if (organizer.customEvents.Count == 0) {
	                DTGUIHelper.ShowLargeBarAlert("You currently have no Custom Events.");
	            }
	
	            EditorGUILayout.Separator();
	
	            int? customEventToDelete = null;
	            int? eventToRename = null;
	
	            for (var i = 0; i < organizer.customEvents.Count; i++) {
	                EditorGUI.indentLevel = 0;
	                var anEvent = organizer.customEvents[i];
	
	                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
	                var exp = DTGUIHelper.Foldout(anEvent.eventExpanded, anEvent.EventName);
	                if (exp != anEvent.eventExpanded) {
	                    UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "toggle expand Custom Event");
	                    anEvent.eventExpanded = exp;
	                }
	
	                GUILayout.FlexibleSpace();
                    var newName = GUILayout.TextField(anEvent.ProspectiveName, GUILayout.Width(170));
                    if (newName != anEvent.ProspectiveName) {
                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Proposed Event Name");
                        anEvent.ProspectiveName = newName;
                    }

                    var buttonPressed = DTGUIHelper.AddCustomEventDeleteIcon(true);

                    switch (buttonPressed) {
                        case DTGUIHelper.DTFunctionButtons.Remove:
                            customEventToDelete = i;
                            break;
                        case DTGUIHelper.DTFunctionButtons.Rename:
                            eventToRename = i;
                            break;
                    }
	
	                EditorGUILayout.EndHorizontal();
	
	                if (anEvent.eventExpanded) {
	                    EditorGUI.indentLevel = 1;
	                    var rcvMode = (MasterAudio.CustomEventReceiveMode)EditorGUILayout.EnumPopup("Send To Receivers", anEvent.eventReceiveMode);
	                    if (rcvMode != anEvent.eventReceiveMode) {
	                        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Send To Receivers");
	                        anEvent.eventReceiveMode = rcvMode;
	                    }
	
	                    if (rcvMode == MasterAudio.CustomEventReceiveMode.WhenDistanceLessThan || rcvMode == MasterAudio.CustomEventReceiveMode.WhenDistanceMoreThan) {
	                        var newDist = EditorGUILayout.Slider("Distance Threshold", anEvent.distanceThreshold, 0f, float.MaxValue);
	                        if (newDist != anEvent.distanceThreshold) {
	                            UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "change Distance Threshold");
	                            anEvent.distanceThreshold = newDist;
	                        }
	                    }
	
	                    EditorGUILayout.Separator();
	                }
	            }
	
	            if (customEventToDelete.HasValue) {
	                organizer.customEvents.RemoveAt(customEventToDelete.Value);
	            }
	            if (eventToRename.HasValue) {
	                RenameEvent(organizer.customEvents[eventToRename.Value]);
	            }
		}

        if (GUI.changed || isDirty) {
            EditorUtility.SetDirty(target);
        }

        //DrawDefaultInspector();
    }
	
	private void RescanDestinationGroups() {
		organizer.selectedDestSoundGroups.Clear();
		
		for (var i = 0; i < organizer.transform.childCount; i++) {
			var aGroup = organizer.transform.GetChild(i);
			organizer.selectedDestSoundGroups.Add(
				new SoundGroupOrganizer.SoundGroupSelection(aGroup.gameObject, false));
		}
	}

	private void RescanDestinationEvents() {
		organizer.selectedDestCustomEvents.Clear();
		
		for (var i = 0; i < organizer.customEvents.Count; i++) {
			var aEvent = organizer.customEvents[i];
			organizer.selectedDestCustomEvents.Add(
				new SoundGroupOrganizer.CustomEventSelection(aEvent, false));
		}
	}

	private bool RescanSourceGroups() {
		if (organizer.sourceObject == null) {
			return false;
		}
		
		organizer.selectedSourceSoundGroups.Clear();
		for (var i = 0; i < organizer.sourceObject.transform.childCount; i++) {
			var aGroup = organizer.sourceObject.transform.GetChild(i);
			organizer.selectedSourceSoundGroups.Add(
				new SoundGroupOrganizer.SoundGroupSelection(aGroup.gameObject, false));
		}
		
		isDirty = true;
		return true;
	}

	private bool RescanSourceEvents(List<CustomEvent> sourceEvents) {
		if (organizer.sourceObject == null) {
			return false;
		}
		
		organizer.selectedSourceCustomEvents.Clear();
		
		for (var i = 0; i < sourceEvents.Count; i++) {
			var anEvent = sourceEvents[i];
			organizer.selectedSourceCustomEvents.Add(
				new SoundGroupOrganizer.CustomEventSelection(anEvent, false));
		}
		
		isDirty = true;
		return true;
	}
	
	private void CheckUncheckAllDestGroups(bool shouldCheck) {
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "check/uncheck All destination Groups");
		
		for (var i = 0; i < organizer.selectedDestSoundGroups.Count; i++) {
			organizer.selectedDestSoundGroups[i]._isSelected = shouldCheck;
		}			
	}

	private void CheckUncheckAllDestEvents(bool shouldCheck) {
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "check/uncheck All destination Custom Events");
		
		for (var i = 0; i < organizer.selectedDestCustomEvents.Count; i++) {
			organizer.selectedDestCustomEvents[i]._isSelected = shouldCheck;
		}			
	}

	private void CheckUncheckAllSourceGroups(bool shouldCheck) {
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "check/uncheck All source Groups");
		
		for (var i = 0; i < organizer.selectedSourceSoundGroups.Count; i++) {
			organizer.selectedSourceSoundGroups[i]._isSelected = shouldCheck;
		}			
	}
	
	private void CheckUncheckAllSourceEvents(bool shouldCheck) {
		UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "check/uncheck All source Custom Events");
		
		for (var i = 0; i < organizer.selectedSourceCustomEvents.Count; i++) {
			organizer.selectedSourceCustomEvents[i]._isSelected = shouldCheck;
		}			
	}
	
	private Transform CreateGroup(AudioClip aClip) {
		if (organizer.dynGroupTemplate == null) {
			DTGUIHelper.ShowAlert("Your 'Group Template' field is empty, please assign it in debug mode. Drag the 'DynamicSoundGroup' prefab from MasterAudio/Sources/Prefabs into that field, then switch back to normal mode.");
			return null;
		}
		
		var groupName = UtilStrings.TrimSpace(aClip.name);
		
		var matchingGroup = _groups.Find(delegate(DynamicSoundGroup obj) {
			return obj.transform.name == groupName;
		});
		
		if (matchingGroup != null) {
			DTGUIHelper.ShowAlert("You already have a Group named '" + groupName + "'. \n\nPlease rename this Group when finished to be unique.");
		}
		
		var spawnedGroup = (GameObject)GameObject.Instantiate(organizer.dynGroupTemplate, organizer.transform.position, Quaternion.identity);
		spawnedGroup.name = groupName;
		
		UndoHelper.CreateObjectForUndo(spawnedGroup, "create Dynamic Group");
		spawnedGroup.transform.parent = organizer.transform;
		
		CreateVariation(spawnedGroup.transform, aClip);
		
		return spawnedGroup.transform;
	}

	private void CreateVariation(Transform aGroup, AudioClip aClip) {
		if (organizer.dynVariationTemplate == null) {
			DTGUIHelper.ShowAlert("Your 'Variation Template' field is empty, please assign it in debug mode. Drag the 'DynamicGroupVariation' prefab from MasterAudio/Sources/Prefabs into that field, then switch back to normal mode.");
			return;
		}
		
		var resourceFileName = string.Empty;
		var useLocalization = false;
		if (organizer.bulkVariationMode == MasterAudio.AudioLocation.ResourceFile) {
			resourceFileName = DTGUIHelper.GetResourcePath(aClip, ref useLocalization);
			if (string.IsNullOrEmpty(resourceFileName)) {
				resourceFileName = aClip.name;
			}
		}
		
		var clipName = UtilStrings.TrimSpace(aClip.name);
		
		var myGroup = aGroup.GetComponent<DynamicSoundGroup>();
		
		var matches = myGroup.groupVariations.FindAll(delegate(DynamicGroupVariation obj) {
			return obj.name == clipName;
		});
		
		if (matches.Count > 0) {
			DTGUIHelper.ShowAlert("You already have a variation for this Group named '" + clipName + "'. \n\nPlease rename these variations when finished to be unique, or you may not be able to play them by name if you have a need to.");
		}
		
		var spawnedVar = (GameObject)GameObject.Instantiate(organizer.dynVariationTemplate, organizer.transform.position, Quaternion.identity);
		spawnedVar.name = clipName;
		
		spawnedVar.transform.parent = aGroup;
		
		var dynamicVar = spawnedVar.GetComponent<DynamicGroupVariation>();
		
		if (organizer.bulkVariationMode == MasterAudio.AudioLocation.ResourceFile) {
			dynamicVar.audLocation = MasterAudio.AudioLocation.ResourceFile;
			dynamicVar.resourceFileName = resourceFileName;
			dynamicVar.useLocalization = useLocalization;
		} else {
			dynamicVar.VarAudio.clip = aClip;
		}
	}

	private List<DynamicSoundGroup> ScanForGroups() {
		var groups = new List<DynamicSoundGroup>();
		
		for (var i = 0; i < organizer.transform.childCount; i++) {
			var aChild = organizer.transform.GetChild(i);
			
			var grp = aChild.GetComponent<DynamicSoundGroup>();
			if (grp == null) {
				continue;
			}
			
			grp.groupVariations = VariationsForGroup(aChild.transform);
			
			groups.Add(grp);
		}
		
		return groups;
	}

	private List<DynamicGroupVariation> VariationsForGroup(Transform groupTrans) {
		var variations = new List<DynamicGroupVariation>();
		
		for (var i = 0; i < groupTrans.childCount; i++) {
			var aVar = groupTrans.GetChild(i);
			
			var variation = aVar.GetComponent<DynamicGroupVariation>();
			variations.Add(variation);
		}
		
		return variations;
	}

	private void PreviewGroup(DynamicSoundGroup aGroup) {
		var rndIndex = UnityEngine.Random.Range(0, aGroup.groupVariations.Count);
		var rndVar = aGroup.groupVariations[rndIndex];
		
		if (rndVar.audLocation == MasterAudio.AudioLocation.ResourceFile) {
            StopPreviewer();
			var fileName = AudioResourceOptimizer.GetLocalizedDynamicSoundGroupFileName(organizer.previewLanguage, rndVar.useLocalization, rndVar.resourceFileName);
			
			var clip = Resources.Load(fileName) as AudioClip;
			if (clip != null) {
                GetPreviewer().PlayOneShot(clip, rndVar.VarAudio.volume);
			} else {
				DTGUIHelper.ShowAlert("Could not find Resource file: " + fileName);
			}
		} else {
			GetPreviewer().PlayOneShot(rndVar.VarAudio.clip, rndVar.VarAudio.volume);
		}
	}

	private void ImportSelectedGroups() {
		if (organizer.sourceObject == null) {
			return;
		}
		
		var imported = 0;
		var skipped = 0;
		
		for (var i = 0; i < organizer.selectedSourceSoundGroups.Count; i++) {
			var item = organizer.selectedSourceSoundGroups[i];
			if (!item._isSelected) {
				continue;
			}
			
			var grp = item._go;
			var dynGrp = grp.GetComponent<DynamicSoundGroup>();
			var maGrp = grp.GetComponent<MasterAudioGroup>();
			
			var wasSkipped = false;
			
			for (var g = 0; g < _groups.Count; g++) {
				if (_groups[g].name == grp.name) {
					Debug.LogError("Group '" + grp.name + "' skipped because there's already a Group with that name in your Organizer. If you wish to import the Group, please delete the one in the Organizer first.");
					skipped++;
					wasSkipped = true;
					break;
				}
			}
			
			if (wasSkipped) {
				continue;
			}
			
			if (dynGrp != null) {
				ImportDynamicGroup(dynGrp);
				imported++;
			} else if (maGrp != null) {
				ImportMAGroup(maGrp);
				imported++;
			} else {
				Debug.LogError("Invalid Group '" + grp.name + "'. It's set up wrong. Contact DarkTonic for assistance.");
			}
		}
		
		var summaryText = imported + " Group(s) imported.";
		if (skipped == 0) {
			Debug.Log(summaryText);
		}
	}

	private void ImportSelectedEvents() {
		if (organizer.sourceObject == null) {
			return;
		}
		
		var imported = 0;
		var skipped = 0;
		
		for (var i = 0; i < organizer.selectedSourceCustomEvents.Count; i++) {
			var item = organizer.selectedSourceCustomEvents[i];
			if (!item._isSelected) {
				continue;
			}
			
			var evt = item._event;
			
			var wasSkipped = false;
			
			for (var g = 0; g < organizer.customEvents.Count; g++) {
				if (organizer.customEvents[g].EventName == evt.EventName) {
					Debug.LogError("Custom Event '" + evt.EventName + "' skipped because there's already a Custom Event with that name in your Organizer. If you wish to import the Custom Event, please delete the one in the Organizer first.");
					skipped++;
					wasSkipped = true;
					break;
				}
			}
			
			if (wasSkipped) {
				continue;
			}
			
			UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "import Organizer Custom Event(s)");
			
			organizer.customEvents.Add(new CustomEvent(item._event.EventName) {
				distanceThreshold = item._event.distanceThreshold,
				eventExpanded = item._event.eventExpanded,
				eventReceiveMode = item._event.eventReceiveMode,
				ProspectiveName = item._event.ProspectiveName
			});
			imported++;
		}
		
		var summaryText = imported + " Custom Event(s) imported.";
		if (skipped == 0) {
			Debug.Log(summaryText);
		}
	}
	
	private GameObject CreateBlankGroup(string grpName) {
		var spawnedGroup = (GameObject)GameObject.Instantiate(organizer.dynGroupTemplate, organizer.transform.position, Quaternion.identity);
		spawnedGroup.name = grpName;
		
		UndoHelper.CreateObjectForUndo(spawnedGroup, "import Organizer Group(s)");
		spawnedGroup.transform.parent = organizer.transform;
		return spawnedGroup;
	}
	
	private void ImportDynamicGroup(DynamicSoundGroup aGroup) {
		var newGroup = CreateBlankGroup(aGroup.name);
		
		var groupTrans = newGroup.transform;
		DynamicGroupVariation aVariation = null;
		DynamicGroupVariation variation = null;
		AudioClip clip = null;
		
        for (var i = 0; i < aGroup.groupVariations.Count; i++) {
            aVariation = aGroup.groupVariations[i];

            GameObject newVariation = (GameObject)GameObject.Instantiate(organizer.dynVariationTemplate.gameObject, groupTrans.position, Quaternion.identity);
            newVariation.transform.parent = groupTrans;

			variation = newVariation.GetComponent<DynamicGroupVariation>();

            var clipName = aVariation.name;

            var aVarAudio = aVariation.GetComponent<AudioSource>();

            #if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
                // copy fields one by one like below.
            #else
                UnityEditorInternal.ComponentUtility.CopyComponent(aVarAudio);
                GameObject.DestroyImmediate(variation.VarAudio);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(variation.gameObject);
                UnityEditorInternal.ComponentUtility.MoveComponentUp(variation.VarAudio);
            #endif

            switch (aVariation.audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    clip = aVarAudio.clip;
                    if (clip == null) {
                        continue;
                    }
                    variation.VarAudio.clip = clip;
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    variation.audLocation = MasterAudio.AudioLocation.ResourceFile;
                    variation.resourceFileName = aVariation.resourceFileName;
                    variation.useLocalization = aVariation.useLocalization;
                    break;
            }
			
			variation.VarAudio.dopplerLevel = aVarAudio.dopplerLevel;
			variation.VarAudio.maxDistance = aVarAudio.maxDistance;
			variation.VarAudio.minDistance = aVarAudio.minDistance;
			variation.VarAudio.bypassEffects = aVarAudio.bypassEffects;
			variation.VarAudio.ignoreListenerVolume = aVarAudio.ignoreListenerVolume;
			variation.VarAudio.mute = aVarAudio.mute;

			#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_3_5
				variation.VarAudio.pan = aVarAudio.pan;
			#else
				variation.VarAudio.panStereo = aVarAudio.panStereo;
			#endif

			variation.VarAudio.rolloffMode = aVarAudio.rolloffMode;
			variation.VarAudio.spread = aVarAudio.spread;
			
			variation.VarAudio.loop = aVarAudio.loop;
			variation.VarAudio.pitch = aVarAudio.pitch;
            variation.transform.name = clipName;
            variation.isExpanded = aVariation.isExpanded;

            variation.useRandomPitch = aVariation.useRandomPitch;
            variation.randomPitchMode = aVariation.randomPitchMode;
            variation.randomPitchMin = aVariation.randomPitchMin;
            variation.randomPitchMax = aVariation.randomPitchMax;

            variation.useRandomVolume = aVariation.useRandomVolume;
            variation.randomVolumeMode = aVariation.randomVolumeMode;
            variation.randomVolumeMin = aVariation.randomVolumeMin;
            variation.randomVolumeMax = aVariation.randomVolumeMax;

            variation.useFades = aVariation.useFades;
            variation.fadeInTime = aVariation.fadeInTime;
            variation.fadeOutTime = aVariation.fadeOutTime;

            variation.useIntroSilence = aVariation.useIntroSilence;
            variation.introSilenceMin = aVariation.introSilenceMin;
            variation.introSilenceMax = aVariation.introSilenceMax;
            variation.fxTailTime = aVariation.fxTailTime;

            variation.useRandomStartTime = aVariation.useRandomStartTime;
            variation.randomStartMinPercent = aVariation.randomStartMinPercent;
            variation.randomStartMaxPercent = aVariation.randomStartMaxPercent;

            // remove unused filter FX
            if (variation.LowPassFilter != null && !variation.LowPassFilter.enabled) {
                GameObject.Destroy(variation.LowPassFilter);
            }
            if (variation.HighPassFilter != null && !variation.HighPassFilter.enabled) {
                GameObject.Destroy(variation.HighPassFilter);
            }
            if (variation.DistortionFilter != null && !variation.DistortionFilter.enabled) {
                GameObject.Destroy(variation.DistortionFilter);
            }
            if (variation.ChorusFilter != null && !variation.ChorusFilter.enabled) {
                GameObject.Destroy(variation.ChorusFilter);
            }
            if (variation.EchoFilter != null && !variation.EchoFilter.enabled) {
                GameObject.Destroy(variation.EchoFilter);
            }
            if (variation.ReverbFilter != null && !variation.ReverbFilter.enabled) {
                GameObject.Destroy(variation.ReverbFilter);
            }
        }
        // added to Hierarchy!

        // populate sounds for playing!
        var groupScript = newGroup.GetComponent<DynamicSoundGroup>();
        // populate other properties.
        groupScript.retriggerPercentage = aGroup.retriggerPercentage;
        groupScript.groupMasterVolume = aGroup.groupMasterVolume;
        groupScript.limitMode = aGroup.limitMode;
        groupScript.limitPerXFrames = aGroup.limitPerXFrames;
        groupScript.minimumTimeBetween = aGroup.minimumTimeBetween;
        groupScript.limitPolyphony = aGroup.limitPolyphony;
        groupScript.voiceLimitCount = aGroup.voiceLimitCount;
        groupScript.curVariationSequence = aGroup.curVariationSequence;
        groupScript.useInactivePeriodPoolRefill = aGroup.useInactivePeriodPoolRefill;
        groupScript.inactivePeriodSeconds = aGroup.inactivePeriodSeconds;
        groupScript.curVariationMode = aGroup.curVariationMode;
        groupScript.useDialogFadeOut = aGroup.useDialogFadeOut;
        groupScript.dialogFadeOutTime = aGroup.dialogFadeOutTime;

        groupScript.chainLoopDelayMin = aGroup.chainLoopDelayMin;
        groupScript.chainLoopDelayMax = aGroup.chainLoopDelayMax;
        groupScript.chainLoopMode = aGroup.chainLoopMode;
        groupScript.chainLoopNumLoops = aGroup.chainLoopNumLoops;

        groupScript.childGroupMode = aGroup.childGroupMode;
        groupScript.childSoundGroups = aGroup.childSoundGroups;

		#if UNITY_5_0
			groupScript.spatialBlendType = aGroup.spatialBlendType;
			groupScript.spatialBlend = aGroup.spatialBlend;
		#endif

        groupScript.targetDespawnedBehavior = aGroup.targetDespawnedBehavior;
        groupScript.despawnFadeTime = aGroup.despawnFadeTime;

        groupScript.resourceClipsAllLoadAsync = aGroup.resourceClipsAllLoadAsync;
        groupScript.logSound = aGroup.logSound;
        groupScript.alwaysHighestPriority = aGroup.alwaysHighestPriority;		
	}
	
	private void ImportMAGroup(MasterAudioGroup aGroup) {
		var newGroup = CreateBlankGroup(aGroup.name);

		var groupTrans = newGroup.transform;
		SoundGroupVariation aVariation = null;
		DynamicGroupVariation variation = null;
		AudioClip clip = null;
		
        for (var i = 0; i < aGroup.groupVariations.Count; i++) {
            aVariation = aGroup.groupVariations[i];

            GameObject newVariation = (GameObject)GameObject.Instantiate(organizer.dynVariationTemplate.gameObject, groupTrans.position, Quaternion.identity);
            newVariation.transform.parent = groupTrans;

			variation = newVariation.GetComponent<DynamicGroupVariation>();

            var clipName = aVariation.name;

            var aVarAudio = aVariation.GetComponent<AudioSource>();

            #if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
                // copy fields one by one like below.
            #else
                UnityEditorInternal.ComponentUtility.CopyComponent(aVarAudio);
                GameObject.DestroyImmediate(variation.VarAudio);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(variation.gameObject);
                UnityEditorInternal.ComponentUtility.MoveComponentUp(variation.VarAudio);
            #endif

            switch (aVariation.audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    clip = aVarAudio.clip;
                    if (clip == null) {
                        continue;
                    }
                    variation.VarAudio.clip = clip;
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    variation.audLocation = MasterAudio.AudioLocation.ResourceFile;
                    variation.resourceFileName = aVariation.resourceFileName;
                    variation.useLocalization = aVariation.useLocalization;
                    break;
            }
			
			variation.VarAudio.dopplerLevel = aVarAudio.dopplerLevel;
			variation.VarAudio.maxDistance = aVarAudio.maxDistance;
			variation.VarAudio.minDistance = aVarAudio.minDistance;
			variation.VarAudio.bypassEffects = aVarAudio.bypassEffects;
			variation.VarAudio.ignoreListenerVolume = aVarAudio.ignoreListenerVolume;
			variation.VarAudio.mute = aVarAudio.mute;

			#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_3_5
				variation.VarAudio.pan = aVarAudio.pan;
			#else
				variation.VarAudio.panStereo = aVarAudio.panStereo;
			#endif

			variation.VarAudio.rolloffMode = aVarAudio.rolloffMode;
			variation.VarAudio.spread = aVarAudio.spread;
			
			variation.VarAudio.loop = aVarAudio.loop;
			variation.VarAudio.pitch = aVarAudio.pitch;
            variation.transform.name = clipName;
            variation.isExpanded = aVariation.isExpanded;

            variation.useRandomPitch = aVariation.useRandomPitch;
            variation.randomPitchMode = aVariation.randomPitchMode;
            variation.randomPitchMin = aVariation.randomPitchMin;
            variation.randomPitchMax = aVariation.randomPitchMax;

            variation.useRandomVolume = aVariation.useRandomVolume;
            variation.randomVolumeMode = aVariation.randomVolumeMode;
            variation.randomVolumeMin = aVariation.randomVolumeMin;
            variation.randomVolumeMax = aVariation.randomVolumeMax;

            variation.useFades = aVariation.useFades;
            variation.fadeInTime = aVariation.fadeInTime;
            variation.fadeOutTime = aVariation.fadeOutTime;

            variation.useIntroSilence = aVariation.useIntroSilence;
            variation.introSilenceMin = aVariation.introSilenceMin;
            variation.introSilenceMax = aVariation.introSilenceMax;
            variation.fxTailTime = aVariation.fxTailTime;

            variation.useRandomStartTime = aVariation.useRandomStartTime;
            variation.randomStartMinPercent = aVariation.randomStartMinPercent;
            variation.randomStartMaxPercent = aVariation.randomStartMaxPercent;

            // remove unused filter FX
            if (variation.LowPassFilter != null && !variation.LowPassFilter.enabled) {
                GameObject.Destroy(variation.LowPassFilter);
            }
            if (variation.HighPassFilter != null && !variation.HighPassFilter.enabled) {
                GameObject.Destroy(variation.HighPassFilter);
            }
            if (variation.DistortionFilter != null && !variation.DistortionFilter.enabled) {
                GameObject.Destroy(variation.DistortionFilter);
            }
            if (variation.ChorusFilter != null && !variation.ChorusFilter.enabled) {
                GameObject.Destroy(variation.ChorusFilter);
            }
            if (variation.EchoFilter != null && !variation.EchoFilter.enabled) {
                GameObject.Destroy(variation.EchoFilter);
            }
            if (variation.ReverbFilter != null && !variation.ReverbFilter.enabled) {
                GameObject.Destroy(variation.ReverbFilter);
            }
        }
        // added to Hierarchy!

        // populate sounds for playing!
        var groupScript = newGroup.GetComponent<DynamicSoundGroup>();
        // populate other properties.
        groupScript.retriggerPercentage = aGroup.retriggerPercentage;
        groupScript.groupMasterVolume = aGroup.groupMasterVolume;
        groupScript.limitMode = aGroup.limitMode;
        groupScript.limitPerXFrames = aGroup.limitPerXFrames;
        groupScript.minimumTimeBetween = aGroup.minimumTimeBetween;
        groupScript.limitPolyphony = aGroup.limitPolyphony;
        groupScript.voiceLimitCount = aGroup.voiceLimitCount;
        groupScript.curVariationSequence = aGroup.curVariationSequence;
        groupScript.useInactivePeriodPoolRefill = aGroup.useInactivePeriodPoolRefill;
        groupScript.inactivePeriodSeconds = aGroup.inactivePeriodSeconds;
        groupScript.curVariationMode = aGroup.curVariationMode;
        groupScript.useDialogFadeOut = aGroup.useDialogFadeOut;
        groupScript.dialogFadeOutTime = aGroup.dialogFadeOutTime;

        groupScript.chainLoopDelayMin = aGroup.chainLoopDelayMin;
        groupScript.chainLoopDelayMax = aGroup.chainLoopDelayMax;
        groupScript.chainLoopMode = aGroup.chainLoopMode;
        groupScript.chainLoopNumLoops = aGroup.chainLoopNumLoops;

        groupScript.childGroupMode = aGroup.childGroupMode;
        groupScript.childSoundGroups = aGroup.childSoundGroups;

		#if UNITY_5_0
			groupScript.spatialBlendType = aGroup.spatialBlendType;
			groupScript.spatialBlend = aGroup.spatialBlend;
		#endif

        groupScript.targetDespawnedBehavior = aGroup.targetDespawnedBehavior;
        groupScript.despawnFadeTime = aGroup.despawnFadeTime;

        groupScript.resourceClipsAllLoadAsync = aGroup.resourceClipsAllLoadAsync;
        groupScript.logSound = aGroup.logSound;
        groupScript.alwaysHighestPriority = aGroup.alwaysHighestPriority;
	}
	
	private void ExportGroupToDGSC(DynamicSoundGroup aGroup) {
		var newGroup = (GameObject)GameObject.Instantiate(organizer.dynGroupTemplate, organizer.transform.position, Quaternion.identity);
		newGroup.name = aGroup.name;
		newGroup.transform.position = organizer.destObject.transform.position;
		
		UndoHelper.CreateObjectForUndo(newGroup, "export Group(s)");
		newGroup.transform.parent = organizer.destObject.transform;
		
		var groupTrans = newGroup.transform;
		DynamicGroupVariation aVariation = null;
		DynamicGroupVariation variation = null;
		AudioClip clip = null;
		
        for (var i = 0; i < aGroup.groupVariations.Count; i++) {
            aVariation = aGroup.groupVariations[i];

            GameObject newVariation = (GameObject)GameObject.Instantiate(organizer.dynVariationTemplate.gameObject, groupTrans.position, Quaternion.identity);
            newVariation.transform.parent = groupTrans;
			newVariation.transform.position = groupTrans.position;
			
		    variation = newVariation.GetComponent<DynamicGroupVariation>();

            var clipName = aVariation.name;

            var aVarAudio = aVariation.GetComponent<AudioSource>();

            #if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
                // copy fields one by one like below.
            #else
                UnityEditorInternal.ComponentUtility.CopyComponent(aVarAudio);
                GameObject.DestroyImmediate(variation.VarAudio);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(variation.gameObject);
                UnityEditorInternal.ComponentUtility.MoveComponentUp(variation.VarAudio);
            #endif

            switch (aVariation.audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    clip = aVarAudio.clip;
                    if (clip == null) {
                        continue;
                    }
                    variation.VarAudio.clip = clip;
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    variation.audLocation = MasterAudio.AudioLocation.ResourceFile;
                    variation.resourceFileName = aVariation.resourceFileName;
                    variation.useLocalization = aVariation.useLocalization;
                    break;
            }
			
			variation.VarAudio.dopplerLevel = aVarAudio.dopplerLevel;
			variation.VarAudio.maxDistance = aVarAudio.maxDistance;
			variation.VarAudio.minDistance = aVarAudio.minDistance;
			variation.VarAudio.bypassEffects = aVarAudio.bypassEffects;
			variation.VarAudio.ignoreListenerVolume = aVarAudio.ignoreListenerVolume;
			variation.VarAudio.mute = aVarAudio.mute;

			#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_3_5
				variation.VarAudio.pan = aVarAudio.pan;
			#else
				variation.VarAudio.panStereo = aVarAudio.panStereo;
			#endif

			variation.VarAudio.rolloffMode = aVarAudio.rolloffMode;
			variation.VarAudio.spread = aVarAudio.spread;
			
			variation.VarAudio.loop = aVarAudio.loop;
			variation.VarAudio.pitch = aVarAudio.pitch;
            variation.transform.name = clipName;
            variation.isExpanded = aVariation.isExpanded;

            variation.useRandomPitch = aVariation.useRandomPitch;
            variation.randomPitchMode = aVariation.randomPitchMode;
            variation.randomPitchMin = aVariation.randomPitchMin;
            variation.randomPitchMax = aVariation.randomPitchMax;

            variation.useRandomVolume = aVariation.useRandomVolume;
            variation.randomVolumeMode = aVariation.randomVolumeMode;
            variation.randomVolumeMin = aVariation.randomVolumeMin;
            variation.randomVolumeMax = aVariation.randomVolumeMax;

            variation.useFades = aVariation.useFades;
            variation.fadeInTime = aVariation.fadeInTime;
            variation.fadeOutTime = aVariation.fadeOutTime;

            variation.useIntroSilence = aVariation.useIntroSilence;
            variation.introSilenceMin = aVariation.introSilenceMin;
            variation.introSilenceMax = aVariation.introSilenceMax;
            variation.fxTailTime = aVariation.fxTailTime;

            variation.useRandomStartTime = aVariation.useRandomStartTime;
            variation.randomStartMinPercent = aVariation.randomStartMinPercent;
            variation.randomStartMaxPercent = aVariation.randomStartMaxPercent;

            // remove unused filter FX
            if (variation.LowPassFilter != null && !variation.LowPassFilter.enabled) {
                GameObject.Destroy(variation.LowPassFilter);
            }
            if (variation.HighPassFilter != null && !variation.HighPassFilter.enabled) {
                GameObject.Destroy(variation.HighPassFilter);
            }
            if (variation.DistortionFilter != null && !variation.DistortionFilter.enabled) {
                GameObject.Destroy(variation.DistortionFilter);
            }
            if (variation.ChorusFilter != null && !variation.ChorusFilter.enabled) {
                GameObject.Destroy(variation.ChorusFilter);
            }
            if (variation.EchoFilter != null && !variation.EchoFilter.enabled) {
                GameObject.Destroy(variation.EchoFilter);
            }
            if (variation.ReverbFilter != null && !variation.ReverbFilter.enabled) {
                GameObject.Destroy(variation.ReverbFilter);
            }
        }
        // added to Hierarchy!

        // populate sounds for playing!
        var groupScript = newGroup.GetComponent<DynamicSoundGroup>();
        // populate other properties.
        groupScript.retriggerPercentage = aGroup.retriggerPercentage;
        groupScript.groupMasterVolume = aGroup.groupMasterVolume;
        groupScript.limitMode = aGroup.limitMode;
        groupScript.limitPerXFrames = aGroup.limitPerXFrames;
        groupScript.minimumTimeBetween = aGroup.minimumTimeBetween;
        groupScript.limitPolyphony = aGroup.limitPolyphony;
        groupScript.voiceLimitCount = aGroup.voiceLimitCount;
        groupScript.curVariationSequence = aGroup.curVariationSequence;
        groupScript.useInactivePeriodPoolRefill = aGroup.useInactivePeriodPoolRefill;
        groupScript.inactivePeriodSeconds = aGroup.inactivePeriodSeconds;
        groupScript.curVariationMode = aGroup.curVariationMode;
        groupScript.useDialogFadeOut = aGroup.useDialogFadeOut;
        groupScript.dialogFadeOutTime = aGroup.dialogFadeOutTime;

        groupScript.chainLoopDelayMin = aGroup.chainLoopDelayMin;
        groupScript.chainLoopDelayMax = aGroup.chainLoopDelayMax;
        groupScript.chainLoopMode = aGroup.chainLoopMode;
        groupScript.chainLoopNumLoops = aGroup.chainLoopNumLoops;

        groupScript.childGroupMode = aGroup.childGroupMode;
        groupScript.childSoundGroups = aGroup.childSoundGroups;

		#if UNITY_5_0
			groupScript.spatialBlendType = aGroup.spatialBlendType;
			groupScript.spatialBlend = aGroup.spatialBlend;
		#endif

        groupScript.targetDespawnedBehavior = aGroup.targetDespawnedBehavior;
        groupScript.despawnFadeTime = aGroup.despawnFadeTime;

        groupScript.resourceClipsAllLoadAsync = aGroup.resourceClipsAllLoadAsync;
        groupScript.logSound = aGroup.logSound;
        groupScript.alwaysHighestPriority = aGroup.alwaysHighestPriority;
	}
	
	private void ExportGroupToMA(DynamicSoundGroup aGroup) {
		var newGroup = (GameObject)GameObject.Instantiate(organizer.maGroupTemplate, organizer.transform.position, Quaternion.identity);
		newGroup.name = aGroup.name;
		newGroup.transform.position = organizer.destObject.transform.position;
		
		UndoHelper.CreateObjectForUndo(newGroup, "export Group(s)");
		newGroup.transform.parent = organizer.destObject.transform;
		
		var groupTrans = newGroup.transform;
		DynamicGroupVariation aVariation = null;
		SoundGroupVariation variation = null;
		AudioClip clip = null;
		
        for (var i = 0; i < aGroup.groupVariations.Count; i++) {
            aVariation = aGroup.groupVariations[i];

            GameObject newVariation = (GameObject)GameObject.Instantiate(organizer.maVariationTemplate.gameObject, groupTrans.position, Quaternion.identity);
            newVariation.transform.parent = groupTrans;
			newVariation.transform.position = groupTrans.position;
			
		    variation = newVariation.GetComponent<SoundGroupVariation>();

            var clipName = aVariation.name;

            var aVarAudio = aVariation.GetComponent<AudioSource>();

            #if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
                // copy fields one by one like below.
            #else
                UnityEditorInternal.ComponentUtility.CopyComponent(aVarAudio);
                GameObject.DestroyImmediate(variation.VarAudio);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(variation.gameObject);
                UnityEditorInternal.ComponentUtility.MoveComponentUp(variation.VarAudio);
            #endif

            switch (aVariation.audLocation) {
                case MasterAudio.AudioLocation.Clip:
                    clip = aVarAudio.clip;
                    if (clip == null) {
                        continue;
                    }
                    variation.VarAudio.clip = clip;
                    break;
                case MasterAudio.AudioLocation.ResourceFile:
                    variation.audLocation = MasterAudio.AudioLocation.ResourceFile;
                    variation.resourceFileName = aVariation.resourceFileName;
                    variation.useLocalization = aVariation.useLocalization;
                    break;
            }
			
			variation.VarAudio.dopplerLevel = aVarAudio.dopplerLevel;
			variation.VarAudio.maxDistance = aVarAudio.maxDistance;
			variation.VarAudio.minDistance = aVarAudio.minDistance;
			variation.VarAudio.bypassEffects = aVarAudio.bypassEffects;
			variation.VarAudio.ignoreListenerVolume = aVarAudio.ignoreListenerVolume;
			variation.VarAudio.mute = aVarAudio.mute;

			#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_3_5
				variation.VarAudio.pan = aVarAudio.pan;
			#else
				variation.VarAudio.panStereo = aVarAudio.panStereo;
			#endif

			variation.VarAudio.rolloffMode = aVarAudio.rolloffMode;
			variation.VarAudio.spread = aVarAudio.spread;
			
			variation.VarAudio.loop = aVarAudio.loop;
			variation.VarAudio.pitch = aVarAudio.pitch;
            variation.transform.name = clipName;
            variation.isExpanded = aVariation.isExpanded;

            variation.useRandomPitch = aVariation.useRandomPitch;
            variation.randomPitchMode = aVariation.randomPitchMode;
            variation.randomPitchMin = aVariation.randomPitchMin;
            variation.randomPitchMax = aVariation.randomPitchMax;

            variation.useRandomVolume = aVariation.useRandomVolume;
            variation.randomVolumeMode = aVariation.randomVolumeMode;
            variation.randomVolumeMin = aVariation.randomVolumeMin;
            variation.randomVolumeMax = aVariation.randomVolumeMax;

            variation.useFades = aVariation.useFades;
            variation.fadeInTime = aVariation.fadeInTime;
            variation.fadeOutTime = aVariation.fadeOutTime;

            variation.useIntroSilence = aVariation.useIntroSilence;
            variation.introSilenceMin = aVariation.introSilenceMin;
            variation.introSilenceMax = aVariation.introSilenceMax;
            variation.fxTailTime = aVariation.fxTailTime;

            variation.useRandomStartTime = aVariation.useRandomStartTime;
            variation.randomStartMinPercent = aVariation.randomStartMinPercent;
            variation.randomStartMaxPercent = aVariation.randomStartMaxPercent;

            // remove unused filter FX
            if (variation.LowPassFilter != null && !variation.LowPassFilter.enabled) {
                GameObject.Destroy(variation.LowPassFilter);
            }
            if (variation.HighPassFilter != null && !variation.HighPassFilter.enabled) {
                GameObject.Destroy(variation.HighPassFilter);
            }
            if (variation.DistortionFilter != null && !variation.DistortionFilter.enabled) {
                GameObject.Destroy(variation.DistortionFilter);
            }
            if (variation.ChorusFilter != null && !variation.ChorusFilter.enabled) {
                GameObject.Destroy(variation.ChorusFilter);
            }
            if (variation.EchoFilter != null && !variation.EchoFilter.enabled) {
                GameObject.Destroy(variation.EchoFilter);
            }
            if (variation.ReverbFilter != null && !variation.ReverbFilter.enabled) {
                GameObject.Destroy(variation.ReverbFilter);
            }
        }
        // added to Hierarchy!

        // populate sounds for playing!
        var groupScript = newGroup.GetComponent<MasterAudioGroup>();
        // populate other properties.
        groupScript.retriggerPercentage = aGroup.retriggerPercentage;
        groupScript.groupMasterVolume = aGroup.groupMasterVolume;
        groupScript.limitMode = aGroup.limitMode;
        groupScript.limitPerXFrames = aGroup.limitPerXFrames;
        groupScript.minimumTimeBetween = aGroup.minimumTimeBetween;
        groupScript.limitPolyphony = aGroup.limitPolyphony;
        groupScript.voiceLimitCount = aGroup.voiceLimitCount;
        groupScript.curVariationSequence = aGroup.curVariationSequence;
        groupScript.useInactivePeriodPoolRefill = aGroup.useInactivePeriodPoolRefill;
        groupScript.inactivePeriodSeconds = aGroup.inactivePeriodSeconds;
        groupScript.curVariationMode = aGroup.curVariationMode;
        groupScript.useDialogFadeOut = aGroup.useDialogFadeOut;
        groupScript.dialogFadeOutTime = aGroup.dialogFadeOutTime;

        groupScript.chainLoopDelayMin = aGroup.chainLoopDelayMin;
        groupScript.chainLoopDelayMax = aGroup.chainLoopDelayMax;
        groupScript.chainLoopMode = aGroup.chainLoopMode;
        groupScript.chainLoopNumLoops = aGroup.chainLoopNumLoops;

        groupScript.childGroupMode = aGroup.childGroupMode;
        groupScript.childSoundGroups = aGroup.childSoundGroups;

		#if UNITY_5_0
			groupScript.spatialBlendType = aGroup.spatialBlendType;
			groupScript.spatialBlend = aGroup.spatialBlend;
		#endif

        groupScript.targetDespawnedBehavior = aGroup.targetDespawnedBehavior;
        groupScript.despawnFadeTime = aGroup.despawnFadeTime;

        groupScript.resourceClipsAllLoadAsync = aGroup.resourceClipsAllLoadAsync;
        groupScript.logSound = aGroup.logSound;
        groupScript.alwaysHighestPriority = aGroup.alwaysHighestPriority;
	}

	
	private void ExportSelectedGroups() {
		if (organizer.destObject == null) {
			return;
		}
		
		var exported = 0;
		var skipped = 0;
		
		var isDestMA = organizer.destObject.GetComponent<MasterAudio>() != null;
		var isDestDGSC = organizer.destObject.GetComponent<DynamicSoundGroupCreator>() != null;
		
		if (!isDestMA && !isDestDGSC) {
			Debug.LogError("Invalid Destination Object '" + organizer.destObject.name + "'. It's set up wrong. Aborting Export. Contact DarkTonic for assistance.");
			return;
		}
		
		for (var i = 0; i < organizer.selectedDestSoundGroups.Count; i++) {
			var item = organizer.selectedDestSoundGroups[i];
			if (!item._isSelected) {
				continue;
			}
			
			var wasSkipped = false;
			var grp = item._go.GetComponent<DynamicSoundGroup>();
			
			if (isDestDGSC) {
				for (var g = 0; g < organizer.destObject.transform.childCount; g++) {
					var aGroup = organizer.destObject.transform.GetChild(g);
					if (aGroup.name == grp.name) {
						Debug.LogError("Group '" + grp.name + "' skipped because there's already a Group with that name in the destination Dynamic Sound Group Creator object. If you wish to export the Group, please delete the one in the DSGC object first.");
						skipped++;
						wasSkipped = true;
					}
				}
				
				if (wasSkipped) {
					continue;
				}

				ExportGroupToDGSC(grp);
				exported++;
			} else if (isDestMA) {
				for (var g = 0; g < organizer.destObject.transform.childCount; g++) {
					var aGroup = organizer.destObject.transform.GetChild(g);
					if (aGroup.name == grp.name) {
						Debug.LogError("Group '" + grp.name + "' skipped because there's already a Group with that name in the destination Master Audio object. If you wish to export the Group, please delete the one in the MA object first.");
						skipped++;
						wasSkipped = true;
					}
				}
				
				if (wasSkipped) {
					continue;
				}
				
				ExportGroupToMA(grp);
				exported++;
			} 
		}
		
		var summaryText = exported + " Group(s) exported.";
		if (skipped == 0) {
			Debug.Log(summaryText);
		}
	}

	private void ExportSelectedEvents() {
		if (organizer.destObject == null) {
			return;
		}
		
		var exported = 0;
		var skipped = 0;

		var ma = organizer.destObject.GetComponent<MasterAudio>();
		var dgsc = organizer.destObject.GetComponent<DynamicSoundGroupCreator>();

		var isDestMA = ma != null;
		var isDestDGSC = dgsc != null;

		if (!isDestMA && !isDestDGSC) {
			Debug.LogError("Invalid Destination Object '" + organizer.destObject.name + "'. It's set up wrong. Aborting Export. Contact DarkTonic for assistance.");
			return;
		}

		for (var i = 0; i < organizer.selectedDestCustomEvents.Count; i++) {
			var item = organizer.selectedDestCustomEvents[i];
			if (!item._isSelected) {
				continue;
			}

			var wasSkipped = false;
			var evt = item._event;

			if (isDestDGSC) {
				for (var g = 0; g < dgsc.customEventsToCreate.Count; g++) {
					var aEvt = dgsc.customEventsToCreate[g];
					if (aEvt.EventName == evt.EventName) {
						Debug.LogError("Group '" + evt.EventName + "' skipped because there's already a Custom Event with that name in the destination Dynamic Sound Group Creator object. If you wish to export the Custom Event, please delete the one in the DSGC object first.");
						skipped++;
						wasSkipped = true;
					}
				}

				if (wasSkipped) {
					continue;
				}

				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, dgsc, "export Custom Event(s)");

				dgsc.customEventsToCreate.Add(new CustomEvent(evt.EventName) { 
					distanceThreshold = evt.distanceThreshold,
					eventExpanded = evt.eventExpanded,
					eventReceiveMode = evt.eventReceiveMode,
					ProspectiveName = evt.EventName
				});

				exported++;
			} else if (isDestMA) {
				for (var g = 0; g < ma.customEvents.Count; g++) {
					var aEvt = ma.customEvents[g];
					if (aEvt.EventName == evt.EventName) {
						Debug.LogError("Custom Event '" + evt.EventName + "' skipped because there's already a Custom Event with that name in the destination Master Audio object. If you wish to export the Custom Event, please delete the one in the MA object first.");
						skipped++;
						wasSkipped = true;
					}
				}
				
				if (wasSkipped) {
					continue;
				}

				UndoHelper.RecordObjectPropertyForUndo(ref isDirty, ma, "export Custom Event(s)");

				ma.customEvents.Add(new CustomEvent(evt.EventName) { 
					distanceThreshold = evt.distanceThreshold,
					eventExpanded = evt.eventExpanded,
					eventReceiveMode = evt.eventReceiveMode,
					ProspectiveName = evt.EventName
				});

				exported++;
			} 
		}
		
		var summaryText = exported + " Custom Event(s) exported.";
		if (skipped == 0) {
			Debug.Log(summaryText);
		}
	}

    private void StopPreviewer() {
        GetPreviewer().Stop();
    }

    private AudioSource GetPreviewer() {
        var aud = _previewer.GetComponent<AudioSource>();
        if (aud == null) {

        #if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
            _previewer.AddComponent<AudioSource>();
        #else
            UnityEditorInternal.ComponentUtility.CopyComponent(organizer.maVariationTemplate.GetComponent<AudioSource>());
            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(_previewer);
        #endif

            aud = _previewer.GetComponent<AudioSource>();
        }

        return aud;
    }
	
    private void ExpandCollapseCustomEvents(bool shouldExpand) {
        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "Expand / Collapse All Custom Events");

        for (var i = 0; i < organizer.customEvents.Count; i++) {
            organizer.customEvents[i].eventExpanded = shouldExpand;
        }
    }
	
    private void SortCustomEvents() {
        UndoHelper.RecordObjectPropertyForUndo(ref isDirty, organizer, "Sort Custom Events Alpha");

        organizer.customEvents.Sort(delegate(CustomEvent x, CustomEvent y) {
            return x.EventName.CompareTo(y.EventName);
        });
    }
	
    private void CreateCustomEvent(string newEventName) {
        if (organizer.customEvents.FindAll(delegate(CustomEvent obj) {
            return obj.EventName == newEventName;
        }).Count > 0) {
            DTGUIHelper.ShowAlert("You already have a custom event named '" + newEventName + "'. Please choose a different name.");
            return;
        }

        organizer.customEvents.Add(new CustomEvent(newEventName));
    }
	
    private void RenameEvent(CustomEvent cEvent) {
        var match = organizer.customEvents.FindAll(delegate(CustomEvent obj) {
            return obj.EventName == cEvent.ProspectiveName;
        });

        if (match.Count > 0) {
            DTGUIHelper.ShowAlert("You already have a Custom Event named '" + cEvent.ProspectiveName + "'. Please choose a different name.");
            return;
        }

        cEvent.EventName = cEvent.ProspectiveName;
    }
}