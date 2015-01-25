using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Master Audio/Footstep Sounds")]
public class FootstepSounds : MonoBehaviour {
    public MasterAudio.SoundSpawnLocationMode soundSpawnMode = MasterAudio.SoundSpawnLocationMode.AttachToCaller;
	public FootstepTriggerMode footstepEvent = FootstepTriggerMode.None;

	public List<FootstepGroup> footstepGroups = new List<FootstepGroup>();
	
    // retrigger limit
    public EventSounds.RetriggerLimMode retriggerLimitMode = EventSounds.RetriggerLimMode.None;
    public int limitPerXFrm = 0;
    public float limitPerXSec = 0f;
    public int triggeredLastFrame = -100;
    public float triggeredLastTime = -100f;
	
	private Transform _trans;

	#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
		public enum FootstepTriggerMode {
			None,
			OnCollision, 
			OnTriggerEnter
		}
	#else
		public enum FootstepTriggerMode {
			None,
			OnCollision, 
			OnTriggerEnter,
			OnCollision2D,
			OnTriggerEnter2D
		}
	#endif

	void OnTriggerEnter(Collider other) {
		if (this.footstepEvent != FootstepTriggerMode.OnTriggerEnter) {
			return;
		}

		PlaySoundsIfMatch(other.gameObject);
	}

	void OnCollisionEnter(Collision collision) {
		if (this.footstepEvent != FootstepTriggerMode.OnCollision) {
			return;
		}

		PlaySoundsIfMatch(collision.gameObject);
	}

	#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
		// events don't exist
	#else
		void OnCollisionEnter2D(Collision2D collision) {
			if (this.footstepEvent != FootstepTriggerMode.OnCollision2D) {
				return;
			}
			
			PlaySoundsIfMatch(collision.gameObject);
		}
		
		void OnTriggerEnter2D(Collider2D other) {
			if (this.footstepEvent != FootstepTriggerMode.OnTriggerEnter2D) {
				return;
			}
			
			PlaySoundsIfMatch(other.gameObject);
		}
	#endif

    private bool CheckForRetriggerLimit() {
        // check for limiting restraints
        switch (retriggerLimitMode) {
            case EventSounds.RetriggerLimMode.FrameBased:
                if (triggeredLastFrame > 0 && Time.frameCount - triggeredLastFrame < limitPerXFrm) {
                    return false;
                }
                break;
            case EventSounds.RetriggerLimMode.TimeBased:
                if (triggeredLastTime > 0 && Time.time - triggeredLastTime < limitPerXSec) {
                    return false;
                }
                break;
        }

        return true;
    }

	private void PlaySoundsIfMatch(GameObject go) {
        if (!CheckForRetriggerLimit()) {
            return;
        }

        // set the last triggered time or frame
        switch (retriggerLimitMode) {
            case EventSounds.RetriggerLimMode.FrameBased:
                triggeredLastFrame = Time.frameCount;
                break;
            case EventSounds.RetriggerLimMode.TimeBased:
                triggeredLastTime = Time.time;
                break;
        }

        for (var i = 0; i < footstepGroups.Count; i++) {
			var aGroup = footstepGroups[i];

			// check filters for matches if turned on
			if (aGroup.useLayerFilter && !aGroup.matchingLayers.Contains(go.layer)) {
				return;
			}
			
			if (aGroup.useTagFilter && !aGroup.matchingTags.Contains(go.tag)) {
				return;
			}

			float volume = aGroup.volume;
			float? pitch = aGroup.pitch;
			if (!aGroup.useFixedPitch) {
				pitch = null;
			}

			string variationName = null;
			if (aGroup.variationType == EventSounds.VariationType.PlaySpecific) {
				variationName = aGroup.variationName;
			}

			switch (soundSpawnMode) {
				case MasterAudio.SoundSpawnLocationMode.CallerLocation:
					MasterAudio.PlaySound3DAtTransform(aGroup.soundType, Trans, volume, pitch, aGroup.delaySound, variationName);
					break;
				case MasterAudio.SoundSpawnLocationMode.AttachToCaller:
					MasterAudio.PlaySound3DFollowTransform(aGroup.soundType, Trans, volume, pitch, aGroup.delaySound, variationName);
					break;
				case MasterAudio.SoundSpawnLocationMode.MasterAudioLocation:
					MasterAudio.PlaySound(aGroup.soundType, volume, pitch, aGroup.delaySound, variationName);
					break;
			}
		}
	}

	private Transform Trans {
		get {
			if (_trans == null) {
				_trans = this.transform;
			}

			return _trans;
		}
	}
}
