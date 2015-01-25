using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// This class contains the actual Audio Source, Unity Filter FX components and other convenience methods having to do with playing sound effects.
/// </summary>
[RequireComponent(typeof(SoundGroupVariationUpdater))]
public class SoundGroupVariation : MonoBehaviour {
    public int weight = 1;

    public bool useLocalization = false;

	public bool useRandomPitch = false;
    public RandomPitchMode randomPitchMode = RandomPitchMode.AddToClipPitch;
    public float randomPitchMin = 0f;
    public float randomPitchMax = 0f;

    public bool useRandomVolume = false;
    public RandomVolumeMode randomVolumeMode = RandomVolumeMode.AddToClipVolume;
    public float randomVolumeMin = 0f;
    public float randomVolumeMax = 0f;

    public MasterAudio.AudioLocation audLocation = MasterAudio.AudioLocation.Clip;
	public string resourceFileName;
    public float fxTailTime = 0f;
    public float original_pitch = 0f;
	public bool isExpanded = true;
	public bool isChecked = true;

    public bool useFades = false;
    public float fadeInTime = 0f;
    public float fadeOutTime = 0f;
	
	public bool useRandomStartTime = false;
	public float randomStartMinPercent = 0f;
	public float randomStartMaxPercent = 0f;
	
    public bool useIntroSilence = false;
    public float introSilenceMin = 0f;
    public float introSilenceMax = 0f;

    private AudioSource _audioSource = null;
    public float fadeMaxVolume;
    public FadeMode curFadeMode = FadeMode.None;
    public DetectEndMode curDetectEndMode = DetectEndMode.None;
	private PlaySoundParams playSndParam = new PlaySoundParams(string.Empty, 1f, 1f, 1f, null, false, 0f, false, false);

    private AudioDistortionFilter distFilter;
    private AudioEchoFilter echoFilter;
    private AudioHighPassFilter hpFilter;
    private AudioLowPassFilter lpFilter;
    private AudioReverbFilter reverbFilter;
    private AudioChorusFilter chorusFilter;
    private bool isWaitingForDelay = false;
    private float _maxVol = 1f;
    private int _instanceId = -1;
    private bool? audioLoops = null;
    private SoundGroupVariationUpdater varUpdater = null;
	private int previousSoundFinishedFrame = -1;

    public delegate void SoundFinishedEventHandler();

    /// <summary>
    /// Subscribe to this event to be notified when the sound stops playing.
    /// </summary>
    public event SoundFinishedEventHandler SoundFinished;

    private Transform _trans;
    private GameObject _go;
	private AudioSource _aud;
	private Transform objectToFollow = null;
    private Transform objectToTriggerFrom = null;
    private MasterAudioGroup parentGroupScript;
    private bool attachToSource = false;
    private float lastTimePlayed = 0f;
    private string resFileName = string.Empty;

    public class PlaySoundParams {
        public string soundType;
        public float volumePercentage;
        public float? pitch;
        public Transform sourceTrans;
        public bool attachToSource;
        public float delaySoundTime;
        public bool isChainLoop;
        public bool isSingleSubscribedPlay;
        public float groupCalcVolume;
		public bool isPlaying;

        public PlaySoundParams(string _soundType, float _volPercent, float _groupCalcVolume, float? _pitch, Transform _sourceTrans, bool _attach, float _delaySoundTime, bool _isChainLoop, bool _isSingleSubscribedPlay) {
            soundType = _soundType;
            volumePercentage = _volPercent;
            groupCalcVolume = _groupCalcVolume;
            pitch = _pitch;
            sourceTrans = _sourceTrans;
            attachToSource = _attach;
            delaySoundTime = _delaySoundTime;
            isChainLoop = _isChainLoop;
            isSingleSubscribedPlay = _isSingleSubscribedPlay;
			isPlaying = false;
        }
    }

    public enum FadeMode {
        None,
        FadeInOut,
        FadeOutEarly,
        GradualFade
    }

    public enum RandomPitchMode {
        AddToClipPitch,
        IgnoreClipPitch
    }

    public enum RandomVolumeMode {
        AddToClipVolume,
        IgnoreClipVolume
    }

    public enum DetectEndMode {
        None,
        DetectEnd
    }

    void Awake() {
        this.original_pitch = VarAudio.pitch;
        this.audioLoops = VarAudio.loop;
		var c = VarAudio.clip; // pre-warm the clip access
        var g = GameObj; // pre-warm the game object clip access

		if (c != null || g != null) { } // to disable the warning for not using it.
    }

    void Start() {
        // this code needs to wait for cloning (for weight).
        var theParent = ParentGroup;
        if (theParent == null) {
            Debug.LogError("Sound Variation '" + this.name + "' has no parent!");
            return;
        }

		#if UNITY_5_0
			var aBus = ParentGroup.BusForGroup;
			if (aBus != null) {
				VarAudio.outputAudioMixerGroup = aBus.mixerChannel;
			}

			SetSpatialBlend();
		#endif


		SetPriority();
    }

	#if UNITY_5_0
		public void SetSpatialBlend() {
			VarAudio.spatialBlend = ParentGroup.SpatialBlendForGroup;
		}
	#endif


	private void SetPriority() {
		if (MasterAudio.Instance.prioritizeOnDistance) {
			if (ParentGroup.alwaysHighestPriority) {
				AudioPrioritizer.Set2dSoundPriority(VarAudio);
			} else {
				AudioPrioritizer.SetSoundGroupInitialPriority(VarAudio);
			}
		}
	}

    /// <summary>
    /// Do not call this! It's called by Master Audio after it is  done initializing.
    /// </summary>
    public void DisableUpdater() {
        if (VariationUpdater != null) {
            VariationUpdater.enabled = false;
        }
    }

    void OnDestroy() {
        StopSoundEarly();
    }

    void OnDisable() {
        StopSoundEarly();
    }

    private void StopSoundEarly() {
        if (MasterAudio.AppIsShuttingDown) {
            return;
        }

        Stop(); // maybe unload clip from Resources
    }

    void OnDrawGizmos() {
        if (MasterAudio.Instance.showGizmos) {
            Gizmos.DrawIcon(this.transform.position, MasterAudio.GIZMO_FILE_NAME, true);
        }
    }

    public void Play(float? pitch, float maxVolume, string gameObjectName, float volPercent, float targetVol, float? targetPitch, Transform sourceTrans, bool attach, float delayTime, bool isChaining, bool isSingleSubscribedPlay) {
		SoundFinished = null; // clear it out so subscribers don't have to clean up
        isWaitingForDelay = false;

		playSndParam.soundType = gameObjectName;
		playSndParam.volumePercentage = volPercent;
		playSndParam.groupCalcVolume = targetVol;
		playSndParam.pitch = targetPitch;
		playSndParam.sourceTrans = sourceTrans;
		playSndParam.attachToSource = attach;
		playSndParam.delaySoundTime = delayTime;
		playSndParam.isChainLoop = isChaining || ParentGroup.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain;
		playSndParam.isSingleSubscribedPlay = isSingleSubscribedPlay;
		playSndParam.isPlaying = true;

		SetPriority(); // reset it back to normal priority in case you're playing 2D this time.

        if (MasterAudio.HasAsyncResourceLoaderFeature() && ShouldLoadAsync) {
            StopAllCoroutines(); // The only Coroutine right now requires pro version and Unity 4.5.3
        }

        // compute pitch
        if (pitch.HasValue) {
            VarAudio.pitch = pitch.Value;
        } else if (useRandomPitch) {
            var randPitch = UnityEngine.Random.Range(randomPitchMin, randomPitchMax);

            switch (randomPitchMode) {
                case RandomPitchMode.AddToClipPitch:
                    randPitch += OriginalPitch;
                    break;
            }

            VarAudio.pitch = randPitch;
        } else { // non random pitch
            VarAudio.pitch = OriginalPitch;
        }

        // set fade mode
        this.curFadeMode = FadeMode.None;
        curDetectEndMode = DetectEndMode.DetectEnd;
        _maxVol = maxVolume;

        if (audLocation == MasterAudio.AudioLocation.Clip) {
            FinishSetupToPlay();
            return;
        }

        if (MasterAudio.HasAsyncResourceLoaderFeature() && ShouldLoadAsync) {
            StartCoroutine(AudioResourceOptimizer.PopulateSourcesWithResourceClipAsync(ResFileName, this, FinishSetupToPlay, ResourceFailedToLoad));
        } else {
            if (!AudioResourceOptimizer.PopulateSourcesWithResourceClip(ResFileName, this)) {
                return; // audio file not found!
            }

            FinishSetupToPlay();
        }
    }

    private void ResourceFailedToLoad() {
        Stop(); // to stop other behavior and disable the Updater script.
    }

    private void FinishSetupToPlay() {
		if (!VarAudio.isPlaying && VarAudio.time > 0f) {
            // paused. Do nothing except Play
        } else if (useFades && (fadeInTime > 0f || fadeOutTime > 0f)) {
            fadeMaxVolume = _maxVol;
            VarAudio.volume = 0f;
            if (VariationUpdater != null) {
                VariationUpdater.enabled = true;
                VariationUpdater.FadeInOut();
            }
        }
         
        VarAudio.loop = this.AudioLoops; // restore original loop setting in case it got lost by loop setting code below for a previous play.

		if (playSndParam.isPlaying && (playSndParam.isChainLoop || playSndParam.isSingleSubscribedPlay)) {
            VarAudio.loop = false;
        }

		if (!playSndParam.isPlaying) {
            return; // has already been "stop" 'd.
        }

        ParentGroup.AddActiveAudioSourceId(InstanceId);

		if (VariationUpdater != null) {
        	VariationUpdater.enabled = true;
			VariationUpdater.WaitForSoundFinish(playSndParam.delaySoundTime);
		}

        attachToSource = false;

        bool useClipAgePriority = MasterAudio.Instance.prioritizeOnDistance && (MasterAudio.Instance.useClipAgePriority || ParentGroup.useClipAgePriority);
		
		if (playSndParam.attachToSource || useClipAgePriority) {
			attachToSource = playSndParam.attachToSource;

			if (VariationUpdater != null) {
            	VariationUpdater.FollowObject(attachToSource, ObjectToFollow, useClipAgePriority);
			}
        }
    }

    /// <summary>
    /// This method allows you to jump to a specific time in an already playing or just triggered Audio Clip.
    /// </summary>
    /// <param name="timeToJumpTo">The time in seconds to jump to.</param>
    public void JumpToTime(float timeToJumpTo) {
		if (!VarAudio.isPlaying || !playSndParam.isPlaying) {
            return;
        }

		VarAudio.time = timeToJumpTo;
    }

    /// <summary>
    /// This method allows you to adjust the volume of an already playing clip, accounting for bus volume, mixer volume and group volume.
    /// </summary>
    /// <param name="volumePercentage"></param>
    public void AdjustVolume(float volumePercentage) {
		if (!VarAudio.isPlaying || !playSndParam.isPlaying) {
            return;
        }

		var newVol = playSndParam.groupCalcVolume * volumePercentage;
        VarAudio.volume = newVol;

		playSndParam.volumePercentage = volumePercentage;
    }

    /// <summary>
    /// This method allows you to pause the audio being played by this Variation. This is automatically called by MasterAudio.PauseSoundGroup and MasterAudio.PauseBus.
    /// </summary>
    public void Pause() {
        if (audLocation == MasterAudio.AudioLocation.ResourceFile && !MasterAudio.Instance.resourceClipsPauseDoNotUnload) {
            Stop();
            return;
        }

        VarAudio.Pause();
        this.curFadeMode = FadeMode.None;
        if (VariationUpdater != null) {
			VariationUpdater.StopWaitingForFinish(); // necessary so that the clip can be unpaused.
		}

        MaybeUnloadClip();
    }

    private void MaybeUnloadClip() {
        if (audLocation == MasterAudio.AudioLocation.ResourceFile) {
            AudioResourceOptimizer.UnloadClipIfUnused(resFileName);
        }
    }

    /// <summary>
    /// This method allows you to stop the audio being played by this Variation. 
    /// </summary>
    public void Stop(bool stopEndDetection = false) {
        var waitStopped = false;

        if (stopEndDetection || isWaitingForDelay) {
			if (VariationUpdater != null) {
				VariationUpdater.StopWaitingForFinish(); // turn off the chain loop endless repeat
            	waitStopped = true;
			}
        }

        objectToFollow = null;
		objectToTriggerFrom = null;
        ParentGroup.RemoveActiveAudioSourceId(InstanceId);

        VarAudio.Stop();
        VarAudio.time = 0f;
		if (VariationUpdater != null) {
			VariationUpdater.StopFollowing();
            VariationUpdater.StopFading();
		}
         
        if (!waitStopped) {
            if (VariationUpdater != null) {
				VariationUpdater.StopWaitingForFinish();
			}
        }

		playSndParam.isPlaying = false;

		if (SoundFinished != null) {
			var willAbort = false;
			if (previousSoundFinishedFrame == Time.frameCount) {
				willAbort = true; // to avoid stack overflow endless loop
			} 
			previousSoundFinishedFrame = Time.frameCount;
			
			if (!willAbort) {
				SoundFinished(); // parameters aren't used
			} 
			SoundFinished = null; // clear it out so subscribers don't have to clean up
		}

        MaybeUnloadClip();
    }

    /// <summary>
    /// This method allows you to fade the sound from this Variation to a specified volume over X seconds.
    /// </summary>
    /// <param name="newVolume">The target volume to fade to.</param>
    /// <param name="fadeTime">The time it will take to fully fade to the target volume.</param>
    public void FadeToVolume(float newVolume, float fadeTime) {
		if (newVolume < 0f || newVolume > 1f) {
			Debug.LogError("Illegal volume passed to FadeToVolume: '" + newVolume + "'. Legal volumes are between 0 and 1");
			return;
		}

		if (fadeTime <= MasterAudio.INNER_LOOP_CHECK_INTERVAL) {
            VarAudio.volume = newVolume; // time really short, just do it at once.
            if (VarAudio.volume <= 0f) {
                Stop();
            }
            return;
        }

        if (VariationUpdater != null) {
            VariationUpdater.FadeOverTimeToVolume(newVolume, fadeTime);
        }
    }

    /// <summary>
    /// This method will fully fade out the sound from this Variation to zero using its existing fadeOutTime.
    /// </summary>
    public void FadeOutNow() {
        if (MasterAudio.AppIsShuttingDown) {
            return;
        }

		if (VarAudio.isPlaying && VariationUpdater != null) {
			VariationUpdater.FadeOutEarly(fadeOutTime);
        }
    }

    /// <summary>
    /// This method will fully fade out the sound from this Variation to zero using over X seconds.
    /// </summary>
    /// <param name="fadeTime">The time it will take to fully fade to the target volume.</param>
    public void FadeOutNow(float fadeTime) {
        if (MasterAudio.AppIsShuttingDown) {
            return;
        }

		if (VarAudio.isPlaying && VariationUpdater != null) {
			VariationUpdater.FadeOutEarly(fadeTime);
        }
    }

    public bool WasTriggeredFromTransform(Transform trans) {
        if (ObjectToFollow == trans || ObjectToTriggerFrom == trans) {
            return true;
        }

        return false;
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity Distortion Filter FX component.
    /// </summary>
    public AudioDistortionFilter DistortionFilter {
        get {
            if (distFilter == null) {
                distFilter = this.GetComponent<AudioDistortionFilter>();
            }
  
            return distFilter;
        }
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity Reverb Filter FX component.
    /// </summary>
    public AudioReverbFilter ReverbFilter {
        get {
            if (reverbFilter == null) {
                reverbFilter = this.GetComponent<AudioReverbFilter>();
            }

            return reverbFilter;
        }
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity Chorus Filter FX component.
    /// </summary>
    public AudioChorusFilter ChorusFilter {
        get {
            if (chorusFilter == null) {
                chorusFilter = this.GetComponent<AudioChorusFilter>();
            }

            return chorusFilter;
        }
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity Echo Filter FX component.
    /// </summary>
    public AudioEchoFilter EchoFilter {
        get {
            if (echoFilter == null) {
                echoFilter = this.GetComponent<AudioEchoFilter>();
            }

            return echoFilter;
        }
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity Low Pass Filter FX component.
    /// </summary>
    public AudioLowPassFilter LowPassFilter {
        get {
            if (lpFilter == null) {
                lpFilter = this.GetComponent<AudioLowPassFilter>();
            }

            return lpFilter;
        }
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity High Pass Filter FX component.
    /// </summary>
    public AudioHighPassFilter HighPassFilter {
        get {
            if (hpFilter == null) {
                hpFilter = this.GetComponent<AudioHighPassFilter>();
            }

            return hpFilter;
        }
    }

    public Transform ObjectToFollow {
        get {
            return objectToFollow;
        }
        set {
            objectToFollow = value;
        }
    }

    public Transform ObjectToTriggerFrom {
        get {
            return objectToTriggerFrom;
        }
        set {
            objectToTriggerFrom = value;
        }
    }

    /// <summary>
    /// This property will return whether there are any Unity FX Filters enabled on this Variation.
    /// </summary>
    public bool HasActiveFXFilter {
        get {
            if (HighPassFilter != null && HighPassFilter.enabled) {
                return true;
            }
            if (LowPassFilter != null && LowPassFilter.enabled) {
                return true;
            }
            if (ReverbFilter != null && ReverbFilter.enabled) {
                return true;
            }
            if (DistortionFilter != null && DistortionFilter.enabled) {
                return true;
            }
            if (EchoFilter != null && EchoFilter.enabled) {
                return true;
            }
            if (ChorusFilter != null && ChorusFilter.enabled) {
                return true;
            }

            return false;
        }
    }

    public MasterAudioGroup ParentGroup {
        get {
			if (this.Trans.parent == null) {
				return null; // project view
			}
			
			if (this.parentGroupScript == null) {
				this.parentGroupScript = this.Trans.parent.GetComponent<MasterAudioGroup>();
            }

            if (this.parentGroupScript == null) {
                Debug.LogError("The Group that Sound Variation '" + this.name + "' is in does not have a MasterAudioGroup script in it!");
            }

            return this.parentGroupScript;
        }
    }

    /// <summary>
    /// This property will return the original pitch of the Variation.
    /// </summary>
    public float OriginalPitch {
        get {
            if (this.original_pitch == 0f) { // lazy lookup for race conditions.
                original_pitch = VarAudio.pitch;
            }

            return this.original_pitch;
        }
    }

    public bool IsAvailableToPlay {
        get {
            if (weight == 0) {
                return false;
            }

			if (!playSndParam.isPlaying && VarAudio.time == 0f) {
                return true; // paused aren't available
            }

            return AudioUtil.GetAudioPlayedPercentage(VarAudio) >= ParentGroup.retriggerPercentage;
        }
    }

    /// <summary>
    /// This property will return the time of the last play of this Variation.
    /// </summary>
    public float LastTimePlayed {
        get {
            return lastTimePlayed;
        }
        set {
            lastTimePlayed = value;
        }
    }

    private int InstanceId {
        get {
            if (this._instanceId < 0) {
                this._instanceId = this.GetInstanceID();
            }

            return this._instanceId;
        }
    }

    public Transform Trans {
        get {
            if (this._trans == null) {
                this._trans = this.transform;
            }

            return this._trans;
        }
    }

    public GameObject GameObj {
        get {
            if (this._go == null) {
                this._go = this.gameObject;
            }

            return this._go;
        }
    }

    public AudioSource VarAudio {
        get {
            if (_audioSource == null) {
                _audioSource = this.GetComponent<AudioSource>();
            }

            return this._audioSource;
        }
    }

    public bool AudioLoops {
        get {
            if (!audioLoops.HasValue) {
                audioLoops = VarAudio.loop;
            }

            return audioLoops.Value;
        }
    }

    public string ResFileName {
        get {
            if (string.IsNullOrEmpty(resFileName)) {
                resFileName = AudioResourceOptimizer.GetLocalizedFileName(useLocalization, resourceFileName);
            }

            return resFileName;
        }
    }

    public SoundGroupVariationUpdater VariationUpdater {
        get {
            if (varUpdater == null) {
                varUpdater = this.GetComponent<SoundGroupVariationUpdater>();
            }

            return varUpdater;
        }
    }

    public bool IsWaitingForDelay {
        get {
            return isWaitingForDelay;
        }
        set {
            isWaitingForDelay = value;
        }
    }

    public PlaySoundParams PlaySoundParm {
        get {
			return playSndParam;
        }
    }

    public bool IsPlaying {
        get {
            return playSndParam.isPlaying;
        }
    }

	public float SetGroupVolume {
		get {
			return playSndParam.groupCalcVolume;
		}
		set {
			playSndParam.groupCalcVolume = value;
		}
	}

    private bool ShouldLoadAsync {
        get {
            if (MasterAudio.Instance.resourceClipsAllLoadAsync) {
                return true;
            }

            return ParentGroup.resourceClipsAllLoadAsync;
        }
    }

	public void ClearSubscribers() {
		SoundFinished = null;
	}
}