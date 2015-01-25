using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5_0
	using UnityEngine.Audio;
#endif

public class MasterAudioGroup : MonoBehaviour {
	public const string NO_BUS = "[NO BUS]";

	public int busIndex = -1;

	#if UNITY_5_0
		public MasterAudio.ItemSpatialBlendType spatialBlendType = MasterAudio.ItemSpatialBlendType.ForceTo3D;
		public float spatialBlend = 1f;
	#endif

	public bool isSelected = false;
	public bool isExpanded = true;
	public float groupMasterVolume = 1f;
	public int retriggerPercentage = 50;
	public VariationMode curVariationMode = VariationMode.Normal;
	public bool alwaysHighestPriority = false;

	public float chainLoopDelayMin;
	public float chainLoopDelayMax;
	public ChainedLoopLoopMode chainLoopMode = ChainedLoopLoopMode.Endless;
	public int chainLoopNumLoops = 0;
	public bool useDialogFadeOut = false;
	public float dialogFadeOutTime = .5f;
	
	public VariationSequence curVariationSequence = VariationSequence.Randomized;
    public bool useInactivePeriodPoolRefill = false;
	public float inactivePeriodSeconds = 5f;
	public List<SoundGroupVariation> groupVariations = new List<SoundGroupVariation>();
	public MasterAudio.AudioLocation bulkVariationMode = MasterAudio.AudioLocation.Clip;
    public bool resourceClipsAllLoadAsync = true;
    public bool logSound = false;

    public bool copySettingsExpanded = false;
    public int selectedVariationIndex = 0;

    public ChildGroupMode childGroupMode = ChildGroupMode.None;
	public List<string> childSoundGroups = new List<string>();

	public LimitMode limitMode = LimitMode.None;
	public int limitPerXFrames = 1;
	public float minimumTimeBetween = 0.1f;
	public bool useClipAgePriority = false;
	
	public bool limitPolyphony = false;
	public int voiceLimitCount = 1;

	public TargetDespawnedBehavior targetDespawnedBehavior = TargetDespawnedBehavior.None;
	public float despawnFadeTime = 1f;

	public bool isSoloed = false;
	public bool isMuted = false;
	
	private List<int> activeAudioSourcesIds = null;
	private int chainLoopCount = 0;
	private string objectName = string.Empty;
	private Transform trans;
	private int childCount;

    public enum ChildGroupMode {
        None,
        TriggerLinkedGroupsWhenRequested,
        TriggerLinkedGroupsWhenPlayed
    }

	public enum TargetDespawnedBehavior {
		None,
		Stop,
		FadeOut
	}

	public enum VariationSequence {
		Randomized,
		TopToBottom
	}

	public enum VariationMode {
		Normal,
		LoopedChain,
		Dialog
	}
	
	public enum ChainedLoopLoopMode {
		Endless,
		NumberOfLoops
	}
	
	public enum LimitMode {
		None,
		FrameBased,
		TimeBased
	}
	
	public int ActiveVoices {
		get {
			return ActiveAudioSourceIds.Count;
		}
	}
	
	public int TotalVoices {
		get {
			return this.transform.childCount;
		}
	}

	void Start() { // time to rename!
		this.objectName = this.name; 
		var childCount = ActiveAudioSourceIds.Count; // time to create clones
		if (childCount > 0)  { } // to get rid of warning

        var needsUpgrade = false;

        for (var i = 0; i < Trans.childCount; i++) {
            var variation = Trans.GetChild(i).GetComponent<SoundGroupVariation>();
            if (variation == null) {
                continue;
            }

            var updater = variation.GetComponent<SoundGroupVariationUpdater>();
            if (updater == null) {
                needsUpgrade = true;
                break;
            }
        }

        if (!needsUpgrade) {
            return;
        }

        Debug.LogError("One or more Variations of Sound Group '" + this.GameObjectName + "' do not have the SoundGroupVariationUpdater component and will not function properly. Please stop and fix this by opening the Master Audio Manager window and clicking the Upgrade MA Prefab button before continuing.");
	}

    public void AddActiveAudioSourceId(int varInstanceId) {
		if (ActiveAudioSourceIds.Contains(varInstanceId))
        {
			return;
		}

		ActiveAudioSourceIds.Add(varInstanceId);
		
		var bus = BusForGroup;
		if (bus != null) {
            bus.AddActiveAudioSourceId(varInstanceId);	
		}
	}
	
	public void RemoveActiveAudioSourceId(int _varInstanceId) {
		ActiveAudioSourceIds.Remove(_varInstanceId);
		
		var bus = BusForGroup;
		if (bus != null) {
			bus.RemoveActiveAudioSourceId(_varInstanceId);	
		}
	}

	#if UNITY_5_0
		public float SpatialBlendForGroup {
			get {
				switch (MasterAudio.Instance.mixerSpatialBlendType) {
					case MasterAudio.AllMixerSpatialBlendType.ForceAllTo2D:
						return MasterAudio.SPATIAL_BLEND_2D_VALUE;	
					case MasterAudio.AllMixerSpatialBlendType.ForceAllTo3D:
						return MasterAudio.SPATIAL_BLEND_3D_VALUE;	
					case MasterAudio.AllMixerSpatialBlendType.ForceAllToCustom:
						return MasterAudio.Instance.mixerSpatialBlend;
					case MasterAudio.AllMixerSpatialBlendType.AllowDifferentPerGroup:
					default:
						switch (spatialBlendType) {
							case MasterAudio.ItemSpatialBlendType.ForceTo2D:
								return MasterAudio.SPATIAL_BLEND_2D_VALUE;
							case MasterAudio.ItemSpatialBlendType.ForceTo3D:
								return MasterAudio.SPATIAL_BLEND_3D_VALUE;
							case MasterAudio.ItemSpatialBlendType.ForceToCustom:
							default:
								return spatialBlend;
						}
				}
			}
		}
	#endif

	public GroupBus BusForGroup {
		get {
			if (busIndex < MasterAudio.HARD_CODED_BUS_OPTIONS || !Application.isPlaying) {
				return null; // no bus, so no voice limit
			}
			
			var index = busIndex - MasterAudio.HARD_CODED_BUS_OPTIONS;

			if (index >= MasterAudio.GroupBuses.Count) { // this happens only with Dynamic SGC item removal
				return null;
			}
			
			return MasterAudio.GroupBuses[index];
		}
	}
	
	public int ChainLoopCount {
		get {
			return chainLoopCount;
		}
		set {
			chainLoopCount = value;
		}
	}

	public string GameObjectName {
		get {
			if (string.IsNullOrEmpty(objectName)) {
				objectName = this.name;
			}

			return objectName;
		}
	}

	private Transform Trans {
		get {
			if (trans == null) {
				this.trans = this.transform;
			}

			return this.trans;
		}
	}

	private List<int> ActiveAudioSourceIds {
		get {
			if (activeAudioSourcesIds == null) {
				activeAudioSourcesIds = new List<int>(Trans.childCount);
			}

			return activeAudioSourcesIds;
		}
	}
}
