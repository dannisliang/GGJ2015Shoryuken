using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DynamicGroupVariation : MonoBehaviour {
	public bool useLocalization = false;
	public bool useRandomPitch = false;
	public SoundGroupVariation.RandomPitchMode randomPitchMode = SoundGroupVariation.RandomPitchMode.AddToClipPitch;
	public float randomPitchMin = 0f;
	public float randomPitchMax = 0f;

	public bool useRandomVolume = false;
	public SoundGroupVariation.RandomVolumeMode randomVolumeMode = SoundGroupVariation.RandomVolumeMode.AddToClipVolume;
	public float randomVolumeMin = 0f;
    public float randomVolumeMax = 0f;

	public int weight = 1;
	public MasterAudio.AudioLocation audLocation = MasterAudio.AudioLocation.Clip;
    public string resourceFileName;
	public bool isExpanded = true;
	public bool isChecked = true;

	public float fxTailTime = 0f;
	public bool useFades = false;
    public float fadeInTime = 0f;
    public float fadeOutTime = 0f;
	
	public bool useIntroSilence;
	public float introSilenceMin;
	public float introSilenceMax;
	
	public bool useRandomStartTime = false;
	public float randomStartMinPercent = 0f;
	public float randomStartMaxPercent = 0f;
	
    private AudioDistortionFilter distFilter;
    private AudioEchoFilter echoFilter;
    private AudioHighPassFilter hpFilter;
    private AudioLowPassFilter lpFilter;
    private AudioReverbFilter reverbFilter;
    private AudioChorusFilter chorusFilter;
    private DynamicSoundGroup parentGroupScript;
	private Transform _trans;
	private AudioSource _aud;

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity Distortion Filter FX component.
    /// </summary>
    public AudioDistortionFilter DistortionFilter
    {
        get
        {
            if (distFilter == null)
            {
                distFilter = this.GetComponent<AudioDistortionFilter>();
            }

            return distFilter;
        }
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity Reverb Filter FX component.
    /// </summary>
    public AudioReverbFilter ReverbFilter
    {
        get
        {
            if (reverbFilter == null)
            {
                reverbFilter = this.GetComponent<AudioReverbFilter>();
            }

            return reverbFilter;
        }
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity Chorus Filter FX component.
    /// </summary>
    public AudioChorusFilter ChorusFilter
    {
        get
        {
            if (chorusFilter == null)
            {
                chorusFilter = this.GetComponent<AudioChorusFilter>();
            }

            return chorusFilter;
        }
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity Echo Filter FX component.
    /// </summary>
    public AudioEchoFilter EchoFilter
    {
        get
        {
            if (echoFilter == null)
            {
                echoFilter = this.GetComponent<AudioEchoFilter>();
            }

            return echoFilter;
        }
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity Low Pass Filter FX component.
    /// </summary>
    public AudioLowPassFilter LowPassFilter
    {
        get
        {
            if (lpFilter == null)
            {
                lpFilter = this.GetComponent<AudioLowPassFilter>();
            }

            return lpFilter;
        }
    }

    /// <summary>
    /// This property returns you a lazy-loaded reference to the Unity High Pass Filter FX component.
    /// </summary>
    public AudioHighPassFilter HighPassFilter
    {
        get
        {
            if (hpFilter == null)
            {
                hpFilter = this.GetComponent<AudioHighPassFilter>();
            }

            return hpFilter;
        }
    }

    public DynamicSoundGroup ParentGroup {
        get {
            if (this.parentGroupScript == null) {
                this.parentGroupScript = this.Trans.parent.GetComponent<DynamicSoundGroup>();
            }

            if (this.parentGroupScript == null) {
                Debug.LogError("The Group that Dynamic Sound Variation '" + this.name + "' is in does not have a DynamicSoundGroup script in it!");
            }

            return this.parentGroupScript;
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

	public AudioSource VarAudio {
		get {
			if (_aud == null) {
				_aud = this.GetComponent<AudioSource>();
			}
			
			return this._aud;
		}
	}
}
