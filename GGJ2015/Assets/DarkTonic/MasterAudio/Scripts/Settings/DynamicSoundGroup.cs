using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DynamicSoundGroup : MonoBehaviour {
	public GameObject variationTemplate;
	
	public bool alwaysHighestPriority = false;
	public float groupMasterVolume = 1f;
	public int retriggerPercentage = 50;
	public MasterAudioGroup.VariationSequence curVariationSequence = MasterAudioGroup.VariationSequence.Randomized;
	public bool useInactivePeriodPoolRefill = false;
	public float inactivePeriodSeconds = 5f;
	public MasterAudioGroup.VariationMode curVariationMode = MasterAudioGroup.VariationMode.Normal;
	public MasterAudio.AudioLocation bulkVariationMode = MasterAudio.AudioLocation.Clip;
	
	public float chainLoopDelayMin;
	public float chainLoopDelayMax;
	public MasterAudioGroup.ChainedLoopLoopMode chainLoopMode = MasterAudioGroup.ChainedLoopLoopMode.Endless;
	public int chainLoopNumLoops = 0;
	public bool useDialogFadeOut = false;
	public float dialogFadeOutTime = .5f;

    public bool resourceClipsAllLoadAsync = true;
    public bool logSound = false;

    public int busIndex = -1;

	#if UNITY_5_0
		public MasterAudio.ItemSpatialBlendType spatialBlendType = MasterAudio.ItemSpatialBlendType.ForceTo3D;
		public float spatialBlend = 1f;
	#endif

	public string busName = string.Empty; // only used to remember the bus name during group creation.
	
	public MasterAudioGroup.LimitMode limitMode = MasterAudioGroup.LimitMode.None;
	public int limitPerXFrames = 1;
	public float minimumTimeBetween = 0.1f;
	public bool limitPolyphony = false;
	public int voiceLimitCount = 1;

	public MasterAudioGroup.TargetDespawnedBehavior targetDespawnedBehavior = MasterAudioGroup.TargetDespawnedBehavior.None;
	public float despawnFadeTime = 1f;

    public bool copySettingsExpanded = false;
    public int selectedVariationIndex = 0;

    public MasterAudioGroup.ChildGroupMode childGroupMode = MasterAudioGroup.ChildGroupMode.None;
    public List<string> childSoundGroups = new List<string>();

	public List<DynamicGroupVariation> groupVariations = new List<DynamicGroupVariation>(); // filled and used by Inspector only
}
