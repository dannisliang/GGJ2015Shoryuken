using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5_0
	using UnityEngine.Audio;
#endif

    /// <summary>
    /// This class is used to host and play Playlists. Contains cross-fading, ducking and more!
    /// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlaylistController : MonoBehaviour {
    public bool startPlaylistOnAwake = true;
    public bool isShuffle = false;
    public bool isAutoAdvance = true;
    public bool loopPlaylist = true;
    public float _playlistVolume = 1f;
    public bool isMuted = false;
    public string startPlaylistName = string.Empty;
    public int syncGroupNum = -1;

	#if UNITY_5_0
		public AudioMixerGroup mixerChannel;
		public MasterAudio.ItemSpatialBlendType spatialBlendType = MasterAudio.ItemSpatialBlendType.ForceTo2D;
		public float spatialBlend = MasterAudio.SPATIAL_BLEND_2D_VALUE;
	#endif

	public bool songChangedEventExpanded = false;
	public string songChangedCustomEvent = string.Empty;
	public bool songEndedEventExpanded = false;
	public string songEndedCustomEvent = string.Empty;

    private AudioSource activeAudio;
    private AudioSource transitioningAudio;
    private float activeAudioEndVolume;
    private float transitioningAudioStartVolume;
    private bool isCrossFading = false;
    private float crossFadeStartTime;
    private List<int> clipsRemaining = new List<int>(10);
    private int currentSequentialClipIndex;
    private AudioDuckingMode duckingMode;
    private float timeToStartUnducking;
    private float timeToFinishUnducking;
    private float originalMusicVolume;
    private float initialDuckVolume;
    private float duckRange;
    private MusicSetting currentSong;
    private GameObject go;
	private string _name;
    private FadeMode curFadeMode = FadeMode.None;
    private float slowFadeTargetVolume;
    private float slowFadeVolStep;
    private MasterAudio.Playlist currentPlaylist = null;
    private float lastTimeMissingPlaylistLogged = -5f;
    private System.Action fadeCompleteCallback;
    private List<MusicSetting> queuedSongs = new List<MusicSetting>(5);
    private bool lostFocus = false;

    private AudioSource audioClip;
    private AudioSource transClip;
    private MusicSetting newSongSetting;
    private bool nextSongRequested = false;
	private bool nextSongScheduled = false;
	private int lastRandomClipIndex = -1;
	private Dictionary<AudioSource, double> scheduledSongsByAudioSource = new Dictionary<AudioSource, double>(2);

    public delegate void SongChangedEventHandler(string newSongName);
    public delegate void SongEndedEventHandler(string songName);

    /// <summary>
    /// This event will notify you when the Playlist song changes.
    /// </summary>
    public event SongChangedEventHandler SongChanged;

    /// <summary>
    /// This event will notify you when the Playlist song ends.
    /// </summary>
    public event SongEndedEventHandler SongEnded;

    private static List<PlaylistController> _instances = null;
    private static int songsPlayedFromPlaylist = 0;
	private AudioSource audio1;
	private AudioSource audio2;

	private Transform _trans;

	public enum AudioPlayType {
		PlayNow,
		Schedule,
		AlreadyScheduled
	}

    public enum PlaylistStates {
        NotInScene,
        Stopped,
        Playing,
        Paused,
        Crossfading
    }

    public enum FadeMode {
        None,
        GradualFade
    }

    public enum AudioDuckingMode {
        NotDucking,
        SetToDuck,
        Ducked
    }

    #region Monobehavior events
    void Awake() {
		// check for "extra" Playlist Controllers of the same name.
        var controllers = (PlaylistController[])GameObject.FindObjectsOfType(typeof(PlaylistController));
        var sameNameCount = 0;

        for (var i = 0; i < controllers.Length; i++) {
            if (controllers[i].ControllerName == ControllerName) {
                sameNameCount++;
            }
        }

        if (sameNameCount > 1) {
            Destroy(gameObject);
            Debug.Log("More than one Playlist Controller prefab exists in this Scene with the same name. Destroying the one called '" + ControllerName + "'. You may wish to set up a Bootstrapper Scene so this does not occur.");
            return;
        }
        // end check

        this.useGUILayout = false;
        duckingMode = AudioDuckingMode.NotDucking;
        currentSong = null;
        songsPlayedFromPlaylist = 0;

        var audios = this.GetComponents<AudioSource>();
        if (audios.Length < 2) {
            Debug.LogError("This prefab should have exactly two Audio Source components. Please revert it.");
            return;
        }

        audio1 = audios[0];
        audio2 = audios[1];

        audio1.clip = null;
        audio2.clip = null;

        activeAudio = audio1;
        transitioningAudio = audio2;
        
		#if UNITY_5_0
			audio1.outputAudioMixerGroup = this.mixerChannel;
			audio2.outputAudioMixerGroup = this.mixerChannel;

			SetSpatialBlend();
		#endif

        curFadeMode = FadeMode.None;
        fadeCompleteCallback = null;
        lostFocus = false;
    }

	#if UNITY_5_0
		public void SetSpatialBlend() {
			switch (MasterAudio.Instance.musicSpatialBlendType) {
				case MasterAudio.AllMusicSpatialBlendType.ForceAllTo2D:
					SetAudioSpatialBlend(MasterAudio.SPATIAL_BLEND_2D_VALUE);	
					break;		
				case MasterAudio.AllMusicSpatialBlendType.ForceAllTo3D:
					SetAudioSpatialBlend(MasterAudio.SPATIAL_BLEND_3D_VALUE);	
					break;		
				case MasterAudio.AllMusicSpatialBlendType.ForceAllToCustom:
					SetAudioSpatialBlend(MasterAudio.Instance.musicSpatialBlend);
					break;		
				case MasterAudio.AllMusicSpatialBlendType.AllowDifferentPerController:
					switch (spatialBlendType) {
						case MasterAudio.ItemSpatialBlendType.ForceTo2D:
							SetAudioSpatialBlend(MasterAudio.SPATIAL_BLEND_2D_VALUE);
							break;
						case MasterAudio.ItemSpatialBlendType.ForceTo3D:
							SetAudioSpatialBlend(MasterAudio.SPATIAL_BLEND_3D_VALUE);
							break;
						case MasterAudio.ItemSpatialBlendType.ForceToCustom:
							SetAudioSpatialBlend(spatialBlend);
							break;
						}
					
					break;
			}
		}

		private void SetAudioSpatialBlend(float blend) {
			audio1.spatialBlend = blend;	
			audio2.spatialBlend = blend;	
		}
	#endif

    // Use this for initialization 
    void Start() {
        if (!string.IsNullOrEmpty(startPlaylistName) && currentPlaylist == null) {
			// fill up randomizer
			InitializePlaylist();
		}

        if (currentPlaylist != null && startPlaylistOnAwake) {
            PlayNextOrRandom(AudioPlayType.PlayNow);
        }

        StartCoroutine(this.CoUpdate());
    }

    IEnumerator CoUpdate() {
        while (true) {
            if (MasterAudio.IgnoreTimeScale) {
                yield return StartCoroutine(CoroutineHelper.WaitForActualSeconds(MasterAudio.INNER_LOOP_CHECK_INTERVAL));
            } else {
                yield return MasterAudio.InnerLoopDelay;
            }

			// fix scheduled track play time if it has changed (it changes constantly).
			if (CanSchedule) {
				if (scheduledSongsByAudioSource.Count > 0 && scheduledSongsByAudioSource.ContainsKey(audioClip)) {
					var newStartTime = CalculateNextTrackStartTime();
					var existingStartTime = scheduledSongsByAudioSource[audioClip];

					if (newStartTime != existingStartTime) {
						audioClip.Stop(); // stop the previous scheduled play
						ScheduleClipPlay(newStartTime);
					}
				}
			}

            // gradual fade code
            if (curFadeMode != FadeMode.GradualFade) {
                continue;
            }

            if (activeAudio == null) {
                continue; // paused or error in setup
            }

            var newVolume = _playlistVolume + slowFadeVolStep;

            if (slowFadeVolStep > 0f) {
                newVolume = Math.Min(newVolume, slowFadeTargetVolume);
            } else {
                newVolume = Math.Max(newVolume, slowFadeTargetVolume);
            }

            _playlistVolume = newVolume;

            UpdateMasterVolume();

            if (newVolume == slowFadeTargetVolume) {
                if (MasterAudio.Instance.stopZeroVolumePlaylists && slowFadeTargetVolume == 0f) {
					StopPlaylist();
				}

				if (fadeCompleteCallback != null) {
                    fadeCompleteCallback();
                    fadeCompleteCallback = null;
                }
                curFadeMode = FadeMode.None;
            }
        }
    }

	void OnEnable() {
		_instances = null; // in case you have a new Controller in the next Scene, we need to uncache the list.
	}

	void OnDisable() {
		_instances = null; // in case you have a new Controller in the next Scene, we need to uncache the list.
	}

    void OnApplicationPause(bool pauseStatus) {
        lostFocus = pauseStatus;
    }

    void Update() {
        if (lostFocus) {
            return; // don't accidentally stop the song below if we just lost focus.
        }

        if (isCrossFading) {
            // cross-fade code
            if (activeAudio.volume >= activeAudioEndVolume) {
                CeaseAudioSource(transitioningAudio);
                isCrossFading = false;
				if (CanSchedule && !nextSongScheduled) { // this needs to run if using crossfading > 0 seconds, because it will not schedule during cross fading (it would kill the crossfade).
					PlayNextOrRandom(AudioPlayType.Schedule);
				}
                SetDuckProperties(); // they now should read from a new audio source
            }

            var workingCrossFade = Math.Max(CrossFadeTime, .001f);
            var ratioPassed = (Time.realtimeSinceStartup - crossFadeStartTime) / workingCrossFade;

            activeAudio.volume = ratioPassed * activeAudioEndVolume;
            transitioningAudio.volume = transitioningAudioStartVolume * (1 - ratioPassed);
            // end cross-fading code
        }

        if (!activeAudio.loop && activeAudio.clip != null) {
            if (!IsAutoAdvance && !activeAudio.isPlaying) {
                CeaseAudioSource(activeAudio); // this will release the resources if not auto-advance
                return;
            }

			if (AudioUtil.IsAudioPaused(activeAudio)) {
				// do not auto-advance if the audio is paused.
				goto AfterAutoAdvance; 
			}

			var shouldAdvance = false;

			if (!activeAudio.isPlaying) {
				shouldAdvance = true; // this will advance even if the code below didn't and the clip stopped due to excessive lag.
			} else {
				var currentClipTime = activeAudio.clip.length - activeAudio.time - (CrossFadeTime * activeAudio.pitch);
				var clipFadeStartTime = Time.deltaTime * EventCalcSounds.FRAMES_EARLY_TO_TRIGGER * activeAudio.pitch;
				shouldAdvance = currentClipTime <= clipFadeStartTime;
			}

			if (shouldAdvance) { // time to cross fade or fade out
                if (currentPlaylist.fadeOutLastSong) {
                    if (isShuffle) {
                        if (clipsRemaining.Count == 0 || !IsAutoAdvance) {
                            FadeOutPlaylist();
                            return;
                        }
                    } else {
                        if (currentSequentialClipIndex >= currentPlaylist.MusicSettings.Count || currentPlaylist.MusicSettings.Count == 1 || !IsAutoAdvance) {
                            FadeOutPlaylist();
                            return;
                        }
                    }
                }

                if (IsAutoAdvance && !nextSongRequested) {
                    if (CanSchedule) {
						FadeInScheduledSong();
					} else {
						PlayNextOrRandom(AudioPlayType.PlayNow);
					}
                }
            }
        }

		AfterAutoAdvance:

        if (isCrossFading) {
            return;
        }

        this.AudioDucking();
    }
    #endregion

    #region public methods

    /// <summary>
    /// This method returns a reference to the PlaylistController whose name you specify. This is necessary when you have more than one.
    /// </summary>
    /// <param name="playlistControllerName"></param>
    /// <returns></returns>
    public static PlaylistController InstanceByName(string playlistControllerName) {
        var match = Instances.Find(delegate(PlaylistController obj) {
            return obj != null && obj.ControllerName == playlistControllerName;
        });

        if (match != null) {
            return match;
        }

        Debug.LogError("Could not find Playlist Controller '" + playlistControllerName + "'.");
        return null;
    }

    /// <summary>
    /// Call this method to clear all songs out of the queued songs list.
    /// </summary>
    public void ClearQueue() {
        queuedSongs.Clear();
    }

    /// <summary>
    /// This method mutes the Playlist if it's not muted, and vice versa.
    /// </summary>
    public void ToggleMutePlaylist() {
        if (IsMuted) {
            UnmutePlaylist();
        } else {
            MutePlaylist();
        }
    }

    /// <summary>
    /// This method mutes the Playlist.
    /// </summary>
    public void MutePlaylist() {
        PlaylistIsMuted = true;
    }

    /// <summary>
    /// This method unmutes the Playlist.
    /// </summary>
    public void UnmutePlaylist() {
        PlaylistIsMuted = false;
    }

    /// <summary>
    /// This method is used by Master Audio to update conditions based on the Ducked Volume Multiplier changing.
    /// </summary>
    public void UpdateDuckedVolumeMultiplier() {
        if (Application.isPlaying) {
            SetDuckProperties();
        }
    }

    /// <summary>
    /// This method will pause the Playlist.
    /// </summary>
    public void PausePlaylist() {
        if (activeAudio == null || transitioningAudio == null) {
            return;
        }

        activeAudio.Pause();
        transitioningAudio.Pause();
    }

    /// <summary>
    /// This method will unpause the Playlist.
    /// </summary>
    public bool ResumePlaylist() {
        if (activeAudio == null || transitioningAudio == null) {
            return false;
        }

        if (activeAudio.clip == null) {
            return false;
        }

        activeAudio.Play();
        transitioningAudio.Play();
        return true;
    }

    /// <summary>
    /// This method will Stop the Playlist. 
    /// </summary>
    public void StopPlaylist(bool onlyFadingClip = false) {
        if (!Application.isPlaying) {
            return;
        }

        currentSong = null;
        if (!onlyFadingClip) {
            CeaseAudioSource(this.activeAudio);
        }

        CeaseAudioSource(this.transitioningAudio);
    }

    /// <summary>
    /// This method allows you to fade the Playlist to a specified volume over X seconds.
    /// </summary>
    /// <param name="targetVolume">The volume to fade to.</param>
    /// <param name="fadeTime">The amount of time to fully fade to the target volume.</param>
    public void FadeToVolume(float targetVolume, float fadeTime, System.Action callback = null) {
        if (fadeTime <= MasterAudio.INNER_LOOP_CHECK_INTERVAL) {
            _playlistVolume = targetVolume;
            UpdateMasterVolume();
            curFadeMode = FadeMode.None; // in case another fade is happening, stop it!
            return;
        }

        curFadeMode = FadeMode.GradualFade;
        slowFadeTargetVolume = targetVolume;
        slowFadeVolStep = (slowFadeTargetVolume - _playlistVolume) / (fadeTime / MasterAudio.INNER_LOOP_CHECK_INTERVAL);

        fadeCompleteCallback = callback;
    }

	/// <summary>
	/// This method will play a random song in the current Playlist.
	/// </summary>
	public void PlayRandomSong() {
		PlayARandomSong(AudioPlayType.PlayNow, false);		
	}

	public void PlayARandomSong(AudioPlayType playType, bool isMidsong) {
		if (clipsRemaining.Count == 0) {
			Debug.LogWarning("There are no clips left in this Playlist. Turn on Loop Playlist if you want to loop the entire song selection.");
            return;
        }

		if (IsCrossFading && playType == AudioPlayType.Schedule) {
			return; // this will kill the crossfade, so abort
		}

		if (isMidsong) {
			nextSongScheduled = false;
		}

		var randIndex = UnityEngine.Random.Range(0, clipsRemaining.Count);
		var clipIndex = clipsRemaining[randIndex];

		switch (playType) {
			case AudioPlayType.PlayNow:
				RemoveRandomClip(randIndex);
				break;
			case AudioPlayType.Schedule:
				lastRandomClipIndex = randIndex;
				break;
			case AudioPlayType.AlreadyScheduled:
				if (lastRandomClipIndex >= 0) {
					RemoveRandomClip(lastRandomClipIndex);
				}
				break;
		}

		PlaySong(currentPlaylist.MusicSettings[clipIndex], playType);
    }

	private void RemoveRandomClip(int randIndex) {
		clipsRemaining.RemoveAt(randIndex);
		if (loopPlaylist && clipsRemaining.Count == 0) {
			FillClips();
		}
	}

	private void PlayFirstQueuedSong(AudioPlayType playType) {
        if (queuedSongs.Count == 0) {
            Debug.LogWarning("There are zero queued songs in PlaylistController '" + this.ControllerName + "'. Cannot play first queued song.");
            return;
        }

        var oldestQueued = queuedSongs[0];
        queuedSongs.RemoveAt(0); // remove before playing so the queued song can loop.

        currentSequentialClipIndex = oldestQueued.songIndex; // keep track of which song we're playing so we don't loop playlist if it's not supposed to.
        PlaySong(oldestQueued, playType);
    }

	/// <summary>
	/// This method will play the next song in the current Playlist.
	/// </summary>
	public void PlayNextSong() {
		PlayTheNextSong(AudioPlayType.PlayNow, false);
	}

	public void PlayTheNextSong(AudioPlayType playType, bool isMidsong) {
		if (currentPlaylist == null) {
            return;
        }

		if (IsCrossFading && playType == AudioPlayType.Schedule) {
			return; // this will kill the crossfade, so abort
		}

		//Debug.Log(nextSongScheduled);
		if (playType != AudioPlayType.AlreadyScheduled && songsPlayedFromPlaylist > 0 && !nextSongScheduled) {
			//Debug.LogError("advance!");
			AdvanceSongCounter();
		}

        if (currentSequentialClipIndex >= currentPlaylist.MusicSettings.Count) {
            Debug.LogWarning("There are no clips left in this Playlist. Turn on Loop Playlist if you want to loop the entire song selection.");
            return;
        }

		if (isMidsong) {
			nextSongScheduled = false;
		}

		PlaySong(currentPlaylist.MusicSettings[currentSequentialClipIndex], playType);
    }

	private void AdvanceSongCounter() {
		currentSequentialClipIndex++;
		
		if (currentSequentialClipIndex >= currentPlaylist.MusicSettings.Count) {
			if (loopPlaylist) {
				currentSequentialClipIndex = 0;
			}
		}
	}

    /// <summary>
    /// This method will play the song in the current Playlist whose name you specify as soon as the currently playing song is done. The current song, if looping, will have loop turned off by this call. This requires auto-advance to work.
    /// </summary>
    /// <param name="clipName">The name of the song to play.</param>
    public void QueuePlaylistClip(string clipName) {
        if (currentPlaylist == null) {
            MasterAudio.LogNoPlaylist(this.ControllerName, "QueuePlaylistClip");
            return;
        }

        if (!this.activeAudio.isPlaying) {
            TriggerPlaylistClip(clipName);
            return;
        }

        MusicSetting setting = currentPlaylist.MusicSettings.Find(delegate(MusicSetting obj) {
            if (obj.audLocation == MasterAudio.AudioLocation.Clip) {
                return obj.clip != null && obj.clip.name == clipName;
            } else { // resource file!
                return obj.resourceFileName == clipName;
            }
        });

        if (setting == null) {
            Debug.LogWarning("Could not find clip '" + clipName + "' in current Playlist in '" + this.ControllerName + "'.");
            return;
        }

        // turn off loop if it's on.
        this.activeAudio.loop = false;
        // add to queue.
        queuedSongs.Add(setting);
    }

    /// <summary>
    /// This method will play the song in the current Playlist whose name you specify.
    /// </summary>
    /// <param name="clipName">The name of the song to play.</param>
    /// <returns>bool - whether the song was played or not</returns>
    public bool TriggerPlaylistClip(string clipName) {
        if (currentPlaylist == null) {
            MasterAudio.LogNoPlaylist(this.ControllerName, "TriggerPlaylistClip");
            return false;
        }

        MusicSetting setting = currentPlaylist.MusicSettings.Find(delegate(MusicSetting obj) {
            if (obj.audLocation == MasterAudio.AudioLocation.Clip) {
                return obj.clip != null && obj.clip.name == clipName;
            } else { // resource file!
                return obj.resourceFileName == clipName || obj.songName == clipName;
            }
        });

        if (setting == null) {
            Debug.LogWarning("Could not find clip '" + clipName + "' in current Playlist in '" + this.ControllerName + "'.");
            return false;
        }

        currentSequentialClipIndex = setting.songIndex; // keep track of which song we're playing so we don't loop playlist if it's not supposed to.
        
		AdvanceSongCounter();

		PlaySong(setting, AudioPlayType.PlayNow);

        return true;
    }

    public void DuckMusicForTime(float duckLength, float pitch, float duckedTimePercentage) {
        if (isCrossFading) {
            return; // no ducking during cross-fading, it screws up calculations.
        }

        var rangedDuck = duckLength / pitch;

        duckingMode = AudioDuckingMode.SetToDuck;
        timeToStartUnducking = Time.realtimeSinceStartup + (rangedDuck * duckedTimePercentage);
        timeToFinishUnducking = Math.Max(Time.realtimeSinceStartup + rangedDuck, timeToStartUnducking);
    }

    /// <summary>
    /// This method is used to update state based on the Playlist Master Volume.
    /// </summary>
    public void UpdateMasterVolume() {
        if (!Application.isPlaying) {
            return;
        }

        if (activeAudio != null && currentSong != null && !IsCrossFading) {
            activeAudio.volume = currentSong.volume * PlaylistVolume;
        }

        if (currentSong != null) {
            activeAudioEndVolume = currentSong.volume * PlaylistVolume;
        }

        SetDuckProperties();
    }

	/// <summary>
	/// This method is used to start a Playlist whether it's already loaded and playing or not.
	/// </summary>
	/// <param name="playlistName">The name of the Playlist to start</param>
	public void StartPlaylist(string playlistName) {
		if (currentPlaylist != null && currentPlaylist.playlistName == playlistName) {
			RestartPlaylist();
		} else {
			ChangePlaylist(playlistName, true);
		}
	}

    /// <summary>
    /// This method is used to change the current Playlist to a new one, and optionally start it playing.
    /// </summary>
	/// <param name="playlistName">The name of the Playlist to start</param>
	/// <param name="playFirstClip">Defaults to true. Whether to start the first song or not.</param>
	public void ChangePlaylist(string playlistName, bool playFirstClip = true) {
        if (currentPlaylist != null && currentPlaylist.playlistName == playlistName) {
			Debug.LogWarning("The Playlist '" + playlistName + "' is already loaded. Ignoring Change Playlist request.");
			return;
		}

		startPlaylistName = playlistName;

        FinishPlaylistInit(playFirstClip);
    }

    private void FinishPlaylistInit(bool playFirstClip = true) {
        if (IsCrossFading) {
            StopPlaylist(true);
        }

        InitializePlaylist();

        if (!Application.isPlaying) {
            return;
        }

        queuedSongs.Clear();

        if (playFirstClip) {
            PlayNextOrRandom(AudioPlayType.PlayNow);
        }
    }

    /// <summary>
    /// This method can be called to restart the current Playlist
    /// </summary>
    public void RestartPlaylist() {
        FinishPlaylistInit(true);
    }

    #endregion

    #region Helper methods
    private void FadeOutPlaylist() {
        if (curFadeMode == FadeMode.GradualFade) {
            return;
        }

        var volumeBeforeFade = _playlistVolume;

        FadeToVolume(0f, CrossFadeTime, delegate() {
            StopPlaylist();
            _playlistVolume = volumeBeforeFade;
        });
    }

    private void InitializePlaylist() {
        FillClips();
        songsPlayedFromPlaylist = 0;
        currentSequentialClipIndex = 0;
		nextSongScheduled = false;
		lastRandomClipIndex = -1;
    }

	private void PlayNextOrRandom(AudioPlayType playType) {
		nextSongRequested = true;

		if (queuedSongs.Count > 0) {
            PlayFirstQueuedSong(playType);
        } else if (!isShuffle) {
            PlayTheNextSong(playType, false);
        } else {
            PlayARandomSong(playType, false);
        }
    }

    private void FillClips() {
        clipsRemaining.Clear();

        // add clips from named playlist.
        if (startPlaylistName == MasterAudio.NO_PLAYLIST_NAME) {
            return;
        }

        this.currentPlaylist = MasterAudio.GrabPlaylist(startPlaylistName);

        if (this.currentPlaylist == null) {
            return;
        }

        MusicSetting aSong = null;

        for (var i = 0; i < currentPlaylist.MusicSettings.Count; i++) {
            aSong = currentPlaylist.MusicSettings[i];
            aSong.songIndex = i;

            if (aSong.audLocation != MasterAudio.AudioLocation.ResourceFile) {
                if (aSong.clip == null) {
                    continue;
                }
            } else { // resource file!
                if (string.IsNullOrEmpty(aSong.resourceFileName)) {
                    continue;
                }
            }

            clipsRemaining.Add(i);
        }
    }

	private void PlaySong(MusicSetting setting, AudioPlayType playType) {
		//Debug.Log("play: " + playType);

		newSongSetting = setting;

        if (activeAudio == null) {
            Debug.LogError("PlaylistController prefab is not in your scene. Cannot play a song.");
            return;
        }

		AudioClip clipToPlay = null;

		var clipWillBeAudibleNow = playType == AudioPlayType.PlayNow || playType == AudioPlayType.AlreadyScheduled;
		if (clipWillBeAudibleNow && currentSong != null && !CanSchedule) {
			if (currentSong.songChangedCustomEvent != string.Empty && currentSong.songChangedCustomEvent != MasterAudio.NO_GROUP_NAME) {
				MasterAudio.FireCustomEvent(currentSong.songChangedCustomEvent, Trans.position);
			}
		}

		if (playType != AudioPlayType.AlreadyScheduled) {
			if (activeAudio.clip != null) {
				var newSongName = string.Empty;
				switch (setting.audLocation) {
					case MasterAudio.AudioLocation.Clip:
						if (setting.clip != null) {
							newSongName = setting.clip.name;
						}
						break;
					case MasterAudio.AudioLocation.ResourceFile:
						newSongName = setting.resourceFileName;
						break;
				}
				
				if (string.IsNullOrEmpty(newSongName)) {
					Debug.LogWarning("The next song has no clip or Resource file assigned. Please fix this. Ignoring song change request.");
					return;
				}
			}

			if (activeAudio.clip == null) {
				audioClip = activeAudio;
				transClip = transitioningAudio;
			} else if (transitioningAudio.clip == null) {
				audioClip = transitioningAudio;
				transClip = activeAudio;
			} else {
				// both are busy!
				audioClip = transitioningAudio;
				transClip = activeAudio;
			}

			if (setting.clip != null) {
				audioClip.clip = setting.clip;
	            audioClip.pitch = setting.pitch;
	        }
	
			audioClip.loop = SongShouldLoop(setting);

	        switch (setting.audLocation) {
	            case MasterAudio.AudioLocation.Clip:
	                if (setting.clip == null) {
	                    MasterAudio.LogWarning("MasterAudio will not play empty Playlist clip for PlaylistController '" + this.ControllerName + "'.");
	                    return;
	                }

	                clipToPlay = setting.clip;
	                break;
	            case MasterAudio.AudioLocation.ResourceFile:
	                if (MasterAudio.HasAsyncResourceLoaderFeature() && ShouldLoadAsync) {
	                    StartCoroutine(AudioResourceOptimizer.PopulateResourceSongToPlaylistControllerAsync(setting.resourceFileName, this.CurrentPlaylist.playlistName, this, playType));
	                } else {
	                    clipToPlay = AudioResourceOptimizer.PopulateResourceSongToPlaylistController(ControllerName, setting.resourceFileName, this.CurrentPlaylist.playlistName);
						if (clipToPlay == null) {
							return;
						}
					}

	                break;
	        }
		} else {
			FinishLoadingNewSong(null, AudioPlayType.AlreadyScheduled);
		}

		if (clipToPlay != null) {
			FinishLoadingNewSong(clipToPlay, playType);
		}
	}
	
	public void FinishLoadingNewSong(AudioClip clipToPlay, AudioPlayType playType) {
        nextSongRequested = false;
        
		var shouldPopulateClip = playType == AudioPlayType.PlayNow || playType == AudioPlayType.Schedule;
		var clipWillBeAudibleNow = playType == AudioPlayType.PlayNow || playType == AudioPlayType.AlreadyScheduled;

		if (shouldPopulateClip) {
			audioClip.clip = clipToPlay;
        	audioClip.pitch = newSongSetting.pitch;
		}

        // set last known time for current song.
        if (currentSong != null) {
            currentSong.lastKnownTimePoint = activeAudio.timeSamples;
        }

		if (clipWillBeAudibleNow) {
	        if (CrossFadeTime == 0 || transClip.clip == null) {
	            CeaseAudioSource(transClip);
	            audioClip.volume = newSongSetting.volume * PlaylistVolume;

	            if (!ActiveAudioSource.isPlaying && currentPlaylist != null && currentPlaylist.fadeInFirstSong) {
	                CrossFadeNow(audioClip);
	            }
	        } else {
	            CrossFadeNow(audioClip);
	        }

			SetDuckProperties();
		}

		switch (playType) {
			case AudioPlayType.AlreadyScheduled:
				// start crossfading now	
				nextSongScheduled = false;
				RemoveScheduledClip();
				break;
			case AudioPlayType.PlayNow:
				audioClip.Play(); // need to play before setting time or it sometimes resets back to zero.
				songsPlayedFromPlaylist++;
				break;
			case AudioPlayType.Schedule:
				#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
					Debug.LogError("Master Audio cannot do gapless song transition on Unity 3. Please report this bug to Dark Tonic as it should not happen.");
					return;
				#else
					var scheduledPlayTime = CalculateNextTrackStartTime();
					
					ScheduleClipPlay(scheduledPlayTime);
			
					nextSongScheduled = true;
					songsPlayedFromPlaylist++;
					break;
				#endif
		}

        var songTimeChanged = false;

        if (syncGroupNum > 0 && currentPlaylist.songTransitionType == MasterAudio.SongFadeInPosition.SynchronizeClips) {
            var firstMatchingGroupController = PlaylistController.Instances.Find(delegate(PlaylistController obj) {
                return obj != this && obj.syncGroupNum == syncGroupNum && obj.ActiveAudioSource.isPlaying;
            });

            if (firstMatchingGroupController != null) {
                audioClip.timeSamples = firstMatchingGroupController.activeAudio.timeSamples;
                songTimeChanged = true;
            }
        }

        // this code will adjust the starting position of a song, but shouldn't do so when you first change Playlists.
        if (currentPlaylist != null) {
            if (songsPlayedFromPlaylist <= 1 && !songTimeChanged) {
                audioClip.timeSamples = 0; // reset pointer so a new Playlist always starts at the beginning, but don't do it for synchronized! We need that first song to use the sync group.
            } else {
                switch (currentPlaylist.songTransitionType) {
                    case MasterAudio.SongFadeInPosition.SynchronizeClips:
                        if (!songTimeChanged) { // otherwise the sync group code above will get defeated.
                            transitioningAudio.timeSamples = activeAudio.timeSamples;
                        }
                        break;
                    case MasterAudio.SongFadeInPosition.NewClipFromLastKnownPosition:
                        var thisSongInPlaylist = currentPlaylist.MusicSettings.Find(delegate(MusicSetting obj) {
                            return obj == newSongSetting;
                        });

                        if (thisSongInPlaylist != null) {
                            transitioningAudio.timeSamples = thisSongInPlaylist.lastKnownTimePoint;
                        }
                        break;
                    case MasterAudio.SongFadeInPosition.NewClipFromBeginning:
                        audioClip.timeSamples = 0; // new song will start at beginning
                        break;
                }
            }

            // account for custom start time.
            if (currentPlaylist.songTransitionType == MasterAudio.SongFadeInPosition.NewClipFromBeginning && newSongSetting.customStartTime > 0f) {
                audioClip.timeSamples = (int)(newSongSetting.customStartTime * audioClip.clip.frequency);
            }
        }

		if (clipWillBeAudibleNow) {
			activeAudio = audioClip;
			transitioningAudio = transClip;

			// song changed
			if (songChangedCustomEvent != string.Empty && songChangedCustomEvent != MasterAudio.NO_GROUP_NAME) {
				MasterAudio.FireCustomEvent(songChangedCustomEvent, Trans.position);
			}

	        if (SongChanged != null) {
	            var clipName = String.Empty;
	            if (audioClip != null) {
	                clipName = audioClip.clip.name;
	            }
	            SongChanged(clipName);
	        }
			// song changed end
		}

        activeAudioEndVolume = newSongSetting.volume * PlaylistVolume;
        var transStartVol = transitioningAudio.volume;
        if (currentSong != null) {
            transStartVol = currentSong.volume;
        }

        transitioningAudioStartVolume = transStartVol * PlaylistVolume;
        currentSong = newSongSetting;

		if (clipWillBeAudibleNow && currentSong.songStartedCustomEvent != string.Empty && currentSong.songStartedCustomEvent != MasterAudio.NO_GROUP_NAME) {
			MasterAudio.FireCustomEvent(currentSong.songStartedCustomEvent, Trans.position);
		}

		if (CanSchedule && playType != AudioPlayType.Schedule) {
			ScheduleNextSong();
		}
    }

	private void RemoveScheduledClip() {
		if (audioClip != null) {
			scheduledSongsByAudioSource.Remove(audioClip);
		}
	}

	#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		private void ScheduleClipPlay(double scheduledPlayTime) {
			// no support
		}
	
		private double CalculateNextTrackStartTime() {
			return 0;
		}	
	
		// can't schedule
		private void ScheduleNextSong() {
		}
		
		private void FadeInScheduledSong() {

		}
	#else
		private void ScheduleNextSong() {
			PlayNextOrRandom(AudioPlayType.Schedule);
		}
	
		private void FadeInScheduledSong() {
			PlayNextOrRandom(AudioPlayType.AlreadyScheduled);
		}
	
		private double CalculateNextTrackStartTime() {
			var timeRemainingOnMainClip = (activeAudio.clip.length - activeAudio.time) / activeAudio.pitch - CrossFadeTime;
			return AudioSettings.dspTime + timeRemainingOnMainClip;
		}

		private void ScheduleClipPlay(double scheduledPlayTime) {
			audioClip.PlayScheduled(scheduledPlayTime);			

			RemoveScheduledClip();
			
			scheduledSongsByAudioSource.Add(audioClip, scheduledPlayTime);
			//Debug.Log("sched: " + scheduledSongsByAudioSource.Count);
			//Debug.LogError(audioClip.clip.name + " : " + scheduledPlayTime);
		}
	#endif

    private void CrossFadeNow(AudioSource audioClip) {
        audioClip.volume = 0f;
        isCrossFading = true;
        duckingMode = AudioDuckingMode.NotDucking;
        crossFadeStartTime = Time.realtimeSinceStartup;
    }

    private void CeaseAudioSource(AudioSource source) {
        if (source == null) {
            return;
        }

		var songName = source.clip == null ? string.Empty : source.clip.name;
        source.Stop();
        AudioResourceOptimizer.UnloadPlaylistSongIfUnused(ControllerName, source.clip);
        source.clip = null;
		RemoveScheduledClip();

		// song ended start
		if (songEndedCustomEvent != string.Empty && songEndedCustomEvent != MasterAudio.NO_GROUP_NAME) {
			MasterAudio.FireCustomEvent(songEndedCustomEvent, Trans.position);
		}

        if (SongEnded != null && !string.IsNullOrEmpty(songName)) {
            SongEnded(songName);
        }
		// song ended end
	}

    private void SetDuckProperties() {
		originalMusicVolume = activeAudio == null ? 1 : activeAudio.volume;

		if (currentSong != null) {
            originalMusicVolume = currentSong.volume * PlaylistVolume;
        }

        initialDuckVolume = MasterAudio.Instance.DuckedVolumeMultiplier * originalMusicVolume;
        duckRange = originalMusicVolume - MasterAudio.Instance.DuckedVolumeMultiplier;

        duckingMode = AudioDuckingMode.NotDucking; // cancel any ducking
    }

    private void AudioDucking() {
		switch (duckingMode) {
            case AudioDuckingMode.NotDucking:
                break;
            case AudioDuckingMode.SetToDuck:
                activeAudio.volume = initialDuckVolume;
                duckingMode = AudioDuckingMode.Ducked;
                break;
            case AudioDuckingMode.Ducked:
                if (Time.realtimeSinceStartup >= timeToFinishUnducking) {
                    activeAudio.volume = originalMusicVolume;
                    duckingMode = AudioDuckingMode.NotDucking;
                } else if (Time.realtimeSinceStartup >= timeToStartUnducking) {
                    activeAudio.volume = initialDuckVolume + (Time.realtimeSinceStartup - timeToStartUnducking) / (timeToFinishUnducking - timeToStartUnducking) * duckRange;
                }
                break;
        }
    }

    private bool SongShouldLoop(MusicSetting setting) {
        if (queuedSongs.Count > 0) {
            return false;
        }

        if (CurrentPlaylist != null && CurrentPlaylist.songTransitionType == MasterAudio.SongFadeInPosition.SynchronizeClips) {
            return true;
        }

        return setting.isLoop;
    }

    #endregion

    #region Properties
    private bool ShouldLoadAsync {
        get {
            if (MasterAudio.Instance.resourceClipsAllLoadAsync) {
                return true;
            }

            return CurrentPlaylist.resourceClipsAllLoadAsync;
        }
    }

    /// <summary>
    /// This property returns the current state of the Playlist. Choices are: NotInScene, Stopped, Playing, Paused, Crossfading
    /// </summary>
    public PlaylistStates PlaylistState {
        get {
            if (this.activeAudio == null || this.transitioningAudio == null) {
                return PlaylistStates.NotInScene;
            }

            if (!ActiveAudioSource.isPlaying) {
                if (ActiveAudioSource.time != 0f) {
                    return PlaylistStates.Paused;
                } else {
                    return PlaylistStates.Stopped;
                }
            }

            if (isCrossFading) {
                return PlaylistStates.Crossfading;
            }

            return PlaylistStates.Playing;
        }
    }

    /// <summary>
    /// This property returns the active audio source for the PlaylistControllers in the Scene. During cross-fading, the one fading in is returned, not the one fading out.
    /// </summary>
    public AudioSource ActiveAudioSource {
        get {
            if (activeAudio.clip == null) {
                return transitioningAudio;
            } else {
                return activeAudio;
            }
        }
    }

    /// <summary>
    /// This property returns all the PlaylistControllers in the Scene.
    /// </summary>
    public static List<PlaylistController> Instances {
        get {
            if (_instances == null) {
                _instances = new List<PlaylistController>();

                var controllers = GameObject.FindObjectsOfType(typeof(PlaylistController));
                for (var i = 0; i < controllers.Length; i++) {
                    _instances.Add(controllers[i] as PlaylistController);
                }
            }

            return _instances;
        }
        set {
            // only for non-caching.
            _instances = value;
        }
    }

    /// <summary>
    /// This property returns the GameObject for the PlaylistController's GameObject.
    /// </summary>
    public GameObject PlaylistControllerGameObject {
        get {
            return go;
        }
    }

    /// <summary>
    ///  This property returns the current Audio Source for the current Playlist song that is playing.
    /// </summary>
    public AudioSource CurrentPlaylistSource {
        get {
            if (activeAudio == null) {
                return null;
            }

            return activeAudio;
        }
    }

    /// <summary>
    ///  This property returns the current Audio Clip for the current Playlist song that is playing.
    /// </summary>
    public AudioClip CurrentPlaylistClip {
        get {
            if (activeAudio == null) {
                return null;
            }

            return activeAudio.clip;
        }
    }

    /// <summary>
    /// This property returns the currently fading out Audio Clip for the Playlist (null if not during cross-fading).
    /// </summary>
    public AudioClip FadingPlaylistClip {
        get {
            if (!isCrossFading) {
                return null;
            }

            if (transitioningAudio == null) {
                return null;
            }

            return transitioningAudio.clip;
        }
    }

    /// <summary>
    /// This property returns the currently fading out Audio Source for the Playlist (null if not during cross-fading).
    /// </summary>
    public AudioSource FadingSource {
        get {
            if (!isCrossFading) {
                return null;
            }

            if (transitioningAudio == null) {
                return null;
            }

            return transitioningAudio;
        }
    }

    /// <summary>
    /// This property returns whether or not the Playlist is currently cross-fading.
    /// </summary>
    public bool IsCrossFading {
        get {
            return isCrossFading;
        }
    }

    /// <summary>
    /// This property returns whether or not the Playlist is currently cross-fading or doing another fade.
    /// </summary>
    public bool IsFading {
        get {
            return isCrossFading || curFadeMode != FadeMode.None;
        }
    }

    /// <summary>
    /// This property gets and sets the volume of the Playlist Controller with Master Playlist Volume taken into account.
    /// </summary>
    public float PlaylistVolume {
        get {
            return MasterAudio.PlaylistMasterVolume * _playlistVolume;
        }
        set {
            _playlistVolume = value;
			UpdateMasterVolume(); 
        }
    }

	#if UNITY_5_0
		public void RouteToMixerChannel(AudioMixerGroup group) {
			activeAudio.outputAudioMixerGroup = group;
			transitioningAudio.outputAudioMixerGroup = group;
		}
	#endif

    /// <summary>
    /// This property returns the current Playlist
    /// </summary>
    public MasterAudio.Playlist CurrentPlaylist {
        get {
            if (currentPlaylist == null && Time.realtimeSinceStartup - lastTimeMissingPlaylistLogged > 2f) {
                Debug.LogWarning("Current Playlist is NULL. Subsequent calls will fail.");
                lastTimeMissingPlaylistLogged = Time.realtimeSinceStartup;
            }
            return currentPlaylist;
        }
    }

    /// <summary>
    /// This property returns whether you have a Playlist assigned to this controller or not.
    /// </summary>
    public bool HasPlaylist {
        get {
            return currentPlaylist != null;
        }
    }

    /// <summary>
    /// This property returns the name of the current Playlist
    /// </summary>
    public string PlaylistName {
        get {
            if (CurrentPlaylist == null) {
                return string.Empty;
            }

            return CurrentPlaylist.playlistName;
        }
    }

    private bool IsMuted {
        get {
            return isMuted;
        }
    }

    /// <summary>
    /// This property returns whether the current Playlist is muted or not
    /// </summary>
    private bool PlaylistIsMuted {
        get {
            return isMuted;
        }
        set {
            isMuted = value;

            if (Application.isPlaying) {
                if (activeAudio != null) {
                    activeAudio.mute = value;
                }

                if (transitioningAudio != null) {
                    transitioningAudio.mute = value;
                }
            } else {
                var audios = this.GetComponents<AudioSource>();
                for (var i = 0; i < audios.Length; i++) {
                    audios[i].mute = value;
                }
            }
        }
    }

    private float CrossFadeTime {
        get {
            if (currentPlaylist != null) {
                return currentPlaylist.crossfadeMode == MasterAudio.Playlist.CrossfadeTimeMode.UseMasterSetting ? MasterAudio.Instance.MasterCrossFadeTime : currentPlaylist.crossFadeTime;
            }

            return MasterAudio.Instance.MasterCrossFadeTime;
        }
    }

    private bool IsAutoAdvance {
        get {
            if (currentPlaylist != null && currentPlaylist.songTransitionType == MasterAudio.SongFadeInPosition.SynchronizeClips) {
                return false;
            }

            return isAutoAdvance;
        }
    }
	
	public GameObject GameObj {
		get {
			if (this.go == null) {
				this.go = this.gameObject;
			}
			
			return this.go;
		}
	}
	
	public string ControllerName {
		get {
			if (_name == null) {
				_name = GameObj.name;
			}
			
			return _name;
		}
	}

	public bool CanSchedule {
		get {
			#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
				return false;
			#else
				return MasterAudio.Instance.useGaplessPlaylists && IsAutoAdvance;
			#endif
		}
	}

	private Transform Trans {
		get {
			if (this._trans == null) {
				this._trans = this.transform;
			}

			return this._trans;
		}
	}

    #endregion
}