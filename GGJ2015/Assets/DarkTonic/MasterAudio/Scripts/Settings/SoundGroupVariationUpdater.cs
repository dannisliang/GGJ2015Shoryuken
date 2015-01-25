using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// This class is only activated when you need code to execute in an Update method, such as "follow" code.
/// </summary>
public class SoundGroupVariationUpdater : MonoBehaviour {
    private Transform objectToFollow = null;
	private GameObject objectToFollowGo = null;
	private bool isFollowing = false;
    private SoundGroupVariation _variation;
	private float priorityLastUpdated = -5f;
    private bool useClipAgePriority = false;
    private WaitForSoundFinishMode waitMode = WaitForSoundFinishMode.None;
    private float soundPlayTime;

    // fade in out vars
    private float fadeOutStartTime = -5;
    private bool fadeInOutWillFadeOut = false;
    private bool hasFadeInOutSetMaxVolume = false;
    private float fadeInOutInFactor = 0f;
    private float fadeInOutOutFactor = 0f;

    // fade out early vars
	private int fadeOutEarlyTotalFrames = 0;
	private float fadeOutEarlyFrameVolChange = 0f;
	private int fadeOutEarlyFrameNumber = 0;
	private float fadeOutEarlyOrigVol = 0f;

    // gradual fade vars
    private float fadeToTargetFrameVolChange = 0f;
    private int fadeToTargetFrameNumber = 0;
    private float fadeToTargetOrigVol = 0f;
    private int fadeToTargetTotalFrames = 0;
    private float fadeToTargetVolume = 0f;
    private bool fadeOutStarted = false;
	private float lastFrameClipTime = -1f;
    private float fxTailEndTime = -1f;
	private bool isPlayingBackward = false;

    private bool hasStartedNextInChain = false;
	
    private enum WaitForSoundFinishMode {
        None,
        Delay,
        Play,
        WaitForEnd,
        StopOrRepeat,
		FxTailWait
    }

    #region Public methods
    public void FadeOverTimeToVolume(float targetVolume, float fadeTime) {
        GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.GradualFade;

        var volDiff = targetVolume - VarAudio.volume;

        if (!VarAudio.loop && VarAudio.clip != null && fadeTime + VarAudio.time > VarAudio.clip.length) { // if too long, fade out faster
            fadeTime = VarAudio.clip.length - VarAudio.time;
        }

        fadeToTargetTotalFrames = (int) (fadeTime / Time.deltaTime);
        fadeToTargetFrameVolChange = volDiff / fadeToTargetTotalFrames;
        fadeToTargetFrameNumber = 0;
        fadeToTargetOrigVol = VarAudio.volume;
        fadeToTargetVolume = targetVolume;
    }

    public void FadeOutEarly(float fadeTime) {
        GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.FadeOutEarly; // cancel the FadeInOut loop, if it's going.

		if (!VarAudio.loop && VarAudio.clip != null && VarAudio.time + fadeTime > VarAudio.clip.length) { // if too long, fade out faster
			fadeTime = VarAudio.clip.length - VarAudio.time;
		}

		fadeOutEarlyTotalFrames = (int) (fadeTime / Time.deltaTime);
		fadeOutEarlyFrameVolChange = -VarAudio.volume / fadeOutEarlyTotalFrames;
		fadeOutEarlyFrameNumber = 0;
		fadeOutEarlyOrigVol = VarAudio.volume;
    }

    public void FadeInOut() {
        GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.FadeInOut; // wait to set this so it stops the previous one if it's still going.
        fadeOutStartTime = VarAudio.clip.length - (GrpVariation.fadeOutTime * VarAudio.pitch);

        if (GrpVariation.fadeInTime > 0f) {
            VarAudio.volume = 0f; // start at zero volume
            fadeInOutInFactor = GrpVariation.fadeMaxVolume / GrpVariation.fadeInTime;
        } else {
            fadeInOutInFactor = 0f;
        }

        fadeInOutWillFadeOut = GrpVariation.fadeOutTime > 0f && !VarAudio.loop;

        if (fadeInOutWillFadeOut) {
            fadeInOutOutFactor = GrpVariation.fadeMaxVolume  / (VarAudio.clip.length - fadeOutStartTime);
        } else {
            fadeInOutOutFactor = 0f;
        }
    }

    public void FollowObject(bool follow, Transform objToFollow, bool clipAgePriority) {
		isFollowing = follow;
        
		if (objToFollow != null) {
			objectToFollow = objToFollow;
			objectToFollowGo = objToFollow.gameObject;
		}
        useClipAgePriority = clipAgePriority;

        UpdateAudioLocationAndPriority(false); // in case we're not following, it should get one update.
    }

    public void WaitForSoundFinish(float delaySound) {
        if (MasterAudio.IsWarming) {
            PlaySoundAndWait();
            return;
        }

        waitMode = WaitForSoundFinishMode.Delay;

        var waitTime = 0f;

        if (GrpVariation.useIntroSilence && GrpVariation.introSilenceMax > 0f) {
            var rndSilence = UnityEngine.Random.Range(GrpVariation.introSilenceMin, GrpVariation.introSilenceMax);
            waitTime += rndSilence;
        }

        if (delaySound > 0f) {
            waitTime += delaySound;
        }

        if (waitTime == 0f) {
            waitMode = WaitForSoundFinishMode.Play; // skip delay mode
        } else {
            soundPlayTime = Time.realtimeSinceStartup + waitTime;
            GrpVariation.IsWaitingForDelay = true;
        }
    }

    public void StopFading() {
        GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.None;

        DisableIfFinished();
    }

    public void StopWaitingForFinish() {
        waitMode = WaitForSoundFinishMode.None;
        GrpVariation.curDetectEndMode = SoundGroupVariation.DetectEndMode.None;

        DisableIfFinished();
    }

    public void StopFollowing() {
        isFollowing = false;
        useClipAgePriority = false;
        objectToFollow = null;
		objectToFollowGo = null;

        DisableIfFinished();
    }
    #endregion

    #region Helper methods
    private void DisableIfFinished() {
        if (isFollowing || GrpVariation.curDetectEndMode == SoundGroupVariation.DetectEndMode.DetectEnd || GrpVariation.curFadeMode != SoundGroupVariation.FadeMode.None) {
            return;
        }

        this.enabled = false;
    }

    private void UpdateAudioLocationAndPriority(bool rePrioritize) {
        // update location, only if following.
        if (isFollowing && objectToFollow != null) {
            this.Trans.position = objectToFollow.position;
        }

        // re-set priority, still used by non-following (audio clip age priority)
        if (!MasterAudio.Instance.prioritizeOnDistance || !rePrioritize || ParentGroup.alwaysHighestPriority) {
            return;
        }

        if (Time.realtimeSinceStartup - priorityLastUpdated > MasterAudio.ReprioritizeTime) {
            AudioPrioritizer.Set3dPriority(VarAudio, useClipAgePriority);
            priorityLastUpdated = Time.realtimeSinceStartup;
        }
    }

    private void PlaySoundAndWait() {
		GrpVariation.IsWaitingForDelay = false;
        VarAudio.Play();
		
		var offset = 0f;
		
		if (GrpVariation.useRandomStartTime) {
			offset = UnityEngine.Random.Range(GrpVariation.randomStartMinPercent, GrpVariation.randomStartMaxPercent) * 0.01f * VarAudio.clip.length; // 0.01 converts percent to decimal
			VarAudio.time = offset;
		}
		
        GrpVariation.LastTimePlayed = Time.time;

        // sound play worked! Duck music if a ducking sound.
        MasterAudio.DuckSoundGroup(ParentGroup.GameObjectName, VarAudio);
		
        isPlayingBackward = GrpVariation.OriginalPitch < 0;
        lastFrameClipTime = isPlayingBackward ? VarAudio.clip.length + 1 : -1f;

        waitMode = WaitForSoundFinishMode.WaitForEnd;
    }

    private void StopOrChain() {
		var playSnd = GrpVariation.PlaySoundParm;

        var wasPlaying = playSnd.isPlaying;
        var usingChainLoop = wasPlaying && playSnd.isChainLoop;

		if (!VarAudio.loop || usingChainLoop) {
            GrpVariation.Stop();
        }

		if (usingChainLoop) {
	        StopWaitingForFinish();

            MaybeChain();
        }
    }

    private void MaybeChain() {
        if (hasStartedNextInChain) {
            return;
        }

        hasStartedNextInChain = true;

        var playSnd = GrpVariation.PlaySoundParm;

        // check if loop count is over.
        if (ParentGroup.chainLoopMode == MasterAudioGroup.ChainedLoopLoopMode.NumberOfLoops && ParentGroup.ChainLoopCount >= ParentGroup.chainLoopNumLoops) {
            // done looping
            return;
        }

        var rndDelay = playSnd.delaySoundTime;
        if (ParentGroup.chainLoopDelayMin > 0f || ParentGroup.chainLoopDelayMax > 0f) {
            rndDelay = UnityEngine.Random.Range(ParentGroup.chainLoopDelayMin, ParentGroup.chainLoopDelayMax);
        }

        // cannot use "AndForget" methods! Chain loop needs to check the status.
        if (playSnd.attachToSource || playSnd.sourceTrans != null) {
            if (playSnd.attachToSource) {
                MasterAudio.PlaySound3DFollowTransform(playSnd.soundType, playSnd.sourceTrans, playSnd.volumePercentage, playSnd.pitch, rndDelay, null, true);
            } else {
                MasterAudio.PlaySound3DAtTransform(playSnd.soundType, playSnd.sourceTrans, playSnd.volumePercentage, playSnd.pitch, rndDelay, null, true);
            }
        } else {
            MasterAudio.PlaySound(playSnd.soundType, playSnd.volumePercentage, playSnd.pitch, rndDelay, null, true);
        }
    }

    private void PerformFading() {
        float clipTime;

        switch (GrpVariation.curFadeMode) {
            case SoundGroupVariation.FadeMode.None:
                break;
            case SoundGroupVariation.FadeMode.FadeInOut:
                if (!VarAudio.isPlaying) {
                    break;
                }

                clipTime = VarAudio.time;
                if (GrpVariation.fadeInTime > 0f && clipTime < GrpVariation.fadeInTime) { // fade in!
                    VarAudio.volume = clipTime * fadeInOutInFactor;
                } else if (clipTime >= GrpVariation.fadeInTime && !hasFadeInOutSetMaxVolume) {
                    VarAudio.volume = GrpVariation.fadeMaxVolume;
                    hasFadeInOutSetMaxVolume = true;
                    if (!fadeInOutWillFadeOut) {
                        StopFading();
                    }
                } else if (fadeInOutWillFadeOut && clipTime >= fadeOutStartTime) { // fade out!
                    if (!fadeOutStarted) {
                        MaybeChain();
                        fadeOutStarted = true;
                    }
                    VarAudio.volume = (VarAudio.clip.length - clipTime) * fadeInOutOutFactor;
                }
                break;
            case SoundGroupVariation.FadeMode.FadeOutEarly:
                if (!VarAudio.isPlaying) {
					break;
                }

				fadeOutEarlyFrameNumber++;	

				VarAudio.volume = (fadeOutEarlyFrameNumber * fadeOutEarlyFrameVolChange) + fadeOutEarlyOrigVol;

				if (fadeOutEarlyFrameNumber >= fadeOutEarlyTotalFrames) {
					GrpVariation.curFadeMode = SoundGroupVariation.FadeMode.None;
					if (MasterAudio.Instance.stopZeroVolumeVariations) {	
						GrpVariation.Stop();
					}
					break;
				}

                break;
            case SoundGroupVariation.FadeMode.GradualFade:
                if (!VarAudio.isPlaying) {
                    break;
                }

                fadeToTargetFrameNumber++;
                if (fadeToTargetFrameNumber >= fadeToTargetTotalFrames) {
                    VarAudio.volume = fadeToTargetVolume;
                    StopFading();
                } else {
                    VarAudio.volume = (fadeToTargetFrameNumber * fadeToTargetFrameVolChange) + fadeToTargetOrigVol;
                }
                break;
        }
    }
    #endregion

    #region MonoBehavior events
    void OnEnable() { // values to be reset every time a sound plays.
        fadeInOutWillFadeOut = false;
        hasFadeInOutSetMaxVolume = false;
        fadeOutStarted = false;
        hasStartedNextInChain = false;
    }

    void LateUpdate() {
        if (isFollowing) {
			if (ParentGroup.targetDespawnedBehavior != MasterAudioGroup.TargetDespawnedBehavior.None) {
				if (objectToFollowGo == null || !DTMonoHelper.IsActive(objectToFollowGo)) { 
					switch (ParentGroup.targetDespawnedBehavior) {
	                    case MasterAudioGroup.TargetDespawnedBehavior.Stop:
	                        GrpVariation.Stop();
	                        break;
	                    case MasterAudioGroup.TargetDespawnedBehavior.FadeOut:
	                        GrpVariation.FadeOutNow(ParentGroup.despawnFadeTime);
	                        break;
	                }

	                StopFollowing();
				}
            }
        }

        // fade in out / out early etc.
        PerformFading();

        // priority
        UpdateAudioLocationAndPriority(true);

        switch (waitMode) {
            case WaitForSoundFinishMode.None:
                break;
            case WaitForSoundFinishMode.Delay:
                if (Time.realtimeSinceStartup >= soundPlayTime) {
                    waitMode = WaitForSoundFinishMode.Play;
                }
                break;
            case WaitForSoundFinishMode.Play:
                PlaySoundAndWait();
                break;
            case WaitForSoundFinishMode.WaitForEnd:
            	var willChangeModes = false;    
			
				if (isPlayingBackward) {
                    if (VarAudio.time > lastFrameClipTime) {
                    	willChangeModes = true;    
                    }
                } else {
                    if (VarAudio.time < lastFrameClipTime) {
                    	willChangeModes = true;    
                    }   
                }
			
				lastFrameClipTime = VarAudio.time;
			
				if (willChangeModes) {
					if (GrpVariation.fxTailTime > 0f) {
						waitMode = WaitForSoundFinishMode.FxTailWait;
						fxTailEndTime = Time.realtimeSinceStartup + GrpVariation.fxTailTime;
					} else {
						waitMode = WaitForSoundFinishMode.StopOrRepeat;
					}
				}
                break;
			case WaitForSoundFinishMode.FxTailWait:
				if (Time.realtimeSinceStartup >= fxTailEndTime) {
					waitMode = WaitForSoundFinishMode.StopOrRepeat;
				}
				break;
            case WaitForSoundFinishMode.StopOrRepeat:
                StopOrChain();
                break;
        }
	}
    #endregion

    #region Properties
    private Transform Trans {
        get {
            return GrpVariation.Trans;
        }
    }

    private AudioSource VarAudio {
        get {
            return GrpVariation.VarAudio;
        }
    }

    private MasterAudioGroup ParentGroup {
        get {
            return GrpVariation.ParentGroup;
        }
    }

    private SoundGroupVariation GrpVariation {
        get {
            if (_variation == null) {
                _variation = this.GetComponent<SoundGroupVariation>();
            }

            return _variation;
        }
    }
    #endregion
}