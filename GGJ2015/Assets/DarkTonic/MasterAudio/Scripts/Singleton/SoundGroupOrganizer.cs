using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundGroupOrganizer : MonoBehaviour {
	public GameObject dynGroupTemplate;
	public GameObject dynVariationTemplate;
	public GameObject maGroupTemplate;
	public GameObject maVariationTemplate;
	
	public MasterAudio.DragGroupMode curDragGroupMode = MasterAudio.DragGroupMode.OneGroupPerClip;
	public MasterAudio.AudioLocation bulkVariationMode = MasterAudio.AudioLocation.Clip;
	public SystemLanguage previewLanguage = SystemLanguage.English;
	public bool useTextGroupFilter = false;
	public string textGroupFilter = string.Empty;
	public TransferMode transMode = TransferMode.None;
	public GameObject sourceObject = null;
	public List<SoundGroupSelection> selectedSourceSoundGroups = new List<SoundGroupSelection>();
	public GameObject destObject = null;
	public List<SoundGroupSelection> selectedDestSoundGroups = new List<SoundGroupSelection>();
	public MAItemType itemType = MAItemType.SoundGroups;
	public List<CustomEventSelection> selectedSourceCustomEvents = new List<CustomEventSelection>();
	public List<CustomEventSelection> selectedDestCustomEvents = new List<CustomEventSelection>();
	public List<CustomEvent> customEvents = new List<CustomEvent>();
    public string newEventName = "my event";
	
	public class CustomEventSelection {
		public CustomEvent _event;
		public bool _isSelected;
		
		public CustomEventSelection(CustomEvent cEvent, bool isSelected) {
			_event = cEvent;
			_isSelected = isSelected;
		}
	}
	
	public class SoundGroupSelection {
		public GameObject _go;
		public bool _isSelected;
		
		public SoundGroupSelection(GameObject go, bool isSelected) {
			_go = go;
			_isSelected = isSelected;
		}
	}

	public enum MAItemType {
		SoundGroups,
		CustomEvents
	}
	
	public enum TransferMode {
		None,
		Import,
		Export
	}
	
	void Awake() {
		Debug.LogError("You have a Sound Group Organizer prefab in this Scene. You should never play a Scene with that type of prefab as it could take up tremendous amounts of audio memory. Please use a Sandbox Scene for that, which is only used to make changes to that prefab and apply them. This Sandbox Scene should never be a Scene that is played in the game.");
	}
}
