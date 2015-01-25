using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class AudioResourceOptimizer {
	private static Dictionary<string, List<AudioSource>> audioResourceTargetsByName = new Dictionary<string, List<AudioSource>>();
    private static Dictionary<string, AudioClip> audioClipsByName = new Dictionary<string, AudioClip>();
	private static Dictionary<string, List<AudioClip>> playlistClipsByPlaylistName = new Dictionary<string, List<AudioClip>>(5);
	
	private static string supportedLanguageFolder = string.Empty;

    public static void ClearAudioClips() {
        audioClipsByName.Clear();
        audioResourceTargetsByName.Clear();
    }

	public static string GetLocalizedDynamicSoundGroupFileName(SystemLanguage localLanguage, bool useLocalization, string resourceFileName) {
        if (!useLocalization) {
            return resourceFileName;
        }

        if (MasterAudio.Instance != null) {
            return GetLocalizedFileName(useLocalization, resourceFileName);
        }

		return localLanguage.ToString() + "/" + resourceFileName;
    } 

    public static string GetLocalizedFileName(bool useLocalization, string resourceFileName) {
        return useLocalization ? SupportedLanguageFolder() + "/" + resourceFileName : resourceFileName;
    }

    public static void AddTargetForClip(string clipName, AudioSource source) {
        if (!audioResourceTargetsByName.ContainsKey(clipName)) {
            audioResourceTargetsByName.Add(clipName, new List<AudioSource>() {
				source
			});
        } else {
            var sources = audioResourceTargetsByName[clipName];
            sources.Add(source);
        }
    }
	
    private static string SupportedLanguageFolder() {
        if (string.IsNullOrEmpty(supportedLanguageFolder)) {
            SystemLanguage curLanguage = Application.systemLanguage;

			if (MasterAudio.Instance != null) {
				switch (MasterAudio.Instance.langMode) {
					case MasterAudio.LanguageMode.SpecificLanguage:
						curLanguage = MasterAudio.Instance.testLanguage;
						break;
					case MasterAudio.LanguageMode.DynamicallySet:
						curLanguage = MasterAudio.DynamicLanguage;
						break;
				}
            } 

            if (MasterAudio.Instance.supportedLanguages.Contains(curLanguage)) {
                supportedLanguageFolder = curLanguage.ToString();
            } else {
                supportedLanguageFolder = MasterAudio.Instance.defaultLanguage.ToString();
            }
        }

        return supportedLanguageFolder;
    }

    public static void ClearSupportLanguageFolder() {
        supportedLanguageFolder = string.Empty;
    }

    public static AudioClip PopulateResourceSongToPlaylistController(string controllerName, string songResourceName, string playlistName) {
        var resAudioClip = Resources.Load(songResourceName) as AudioClip;

        if (resAudioClip == null) {
            MasterAudio.LogWarning("Resource file '" + songResourceName + "' could not be located from Playlist '" + playlistName + "'.");
            return null;
        }

        FinishRecordingPlaylistClip(controllerName, resAudioClip);
		
        return resAudioClip;
    }

    private static void FinishRecordingPlaylistClip(string controllerName, AudioClip resAudioClip) {
		List<AudioClip> clips = null;
		
		if (!playlistClipsByPlaylistName.ContainsKey(controllerName)) {
			clips = new List<AudioClip>(5);
			playlistClipsByPlaylistName.Add(controllerName, clips);
		} else {
			clips = playlistClipsByPlaylistName[controllerName];
		}

		clips.Add(resAudioClip); // even needs to add duplicates
    }

#if UNITY_4_5_3 || UNITY_4_5_4 || UNITY_4_5_5 || UNITY_4_6 
    public static IEnumerator PopulateResourceSongToPlaylistControllerAsync(string songResourceName, string playlistName, PlaylistController controller, PlaylistController.AudioPlayType playType) {
		var asyncRes = Resources.LoadAsync(songResourceName, typeof(AudioClip));

		while (!asyncRes.isDone) {
			yield return MasterAudio.endOfFrameDelay;
		}

        var resAudioClip = asyncRes.asset as AudioClip;

        if (resAudioClip == null) {
            MasterAudio.LogWarning("Resource file '" + songResourceName + "' could not be located from Playlist '" + playlistName + "'.");
            yield break;
        }

        FinishRecordingPlaylistClip(controller.ControllerName, resAudioClip);

        controller.FinishLoadingNewSong(resAudioClip, playType);
    }

    /// <summary>
    /// Populates the sources with resource clip, non-thread blocking.
    /// </summary>
    /// <param name="clipName">Clip name.</param>
    /// <param name="variation">Variation.</param>
    /// <param name="successAction">Method to execute if successful.</param>
    /// <param name="failureAction">Method to execute if not successful.</param>
    public static IEnumerator PopulateSourcesWithResourceClipAsync(string clipName, SoundGroupVariation variation, System.Action successAction, System.Action failureAction) {
        if (audioClipsByName.ContainsKey(clipName)) {
            if (successAction != null) {
                successAction();
            }

            yield break;
        }

        var asyncRes = Resources.LoadAsync(clipName, typeof(AudioClip));

        while (!asyncRes.isDone) {
            yield return MasterAudio.endOfFrameDelay;
        }

        var resAudioClip = asyncRes.asset as AudioClip;

        if (resAudioClip == null) {
            MasterAudio.LogError("Resource file '" + clipName + "' could not be located.");

            if (failureAction != null) {
                failureAction();
            }
            yield break;
        }

        if (!audioResourceTargetsByName.ContainsKey(clipName)) {
            Debug.LogError("No Audio Sources found to add Resource file '" + clipName + "'.");

            if (failureAction != null) {
                failureAction();
            }
            yield break;
        } else {
            var sources = audioResourceTargetsByName[clipName];

            for (var i = 0; i < sources.Count; i++) {
                sources[i].clip = resAudioClip;
            }
        }

		if (!audioClipsByName.ContainsKey(clipName)) {
			audioClipsByName.Add(clipName, resAudioClip);
		}

        if (successAction != null) {
            successAction();
        }
    }
#else
	public static IEnumerator PopulateResourceSongToPlaylistControllerAsync(string songResourceName, string playlistName, PlaylistController controller, PlaylistController.AudioPlayType playType) {
		Debug.LogError("If this method got called, please report it to Dark Tonic immediately. It should not happen.");
		yield break;
	}

	public static IEnumerator PopulateSourcesWithResourceClipAsync(string clipName, SoundGroupVariation variation, System.Action successAction, System.Action failureAction) {
		Debug.LogError("If this method got called, please report it to Dark Tonic immediately. It should not happen.");
		yield break;
	}
#endif

    public static void UnloadPlaylistSongIfUnused(string controllerName, AudioClip clipToRemove) {
        if (clipToRemove == null) {
            return; // no need
        }
		
		if (!playlistClipsByPlaylistName.ContainsKey(controllerName)) {
			return; // no resource clips have been played yet.
		}
		
		var clips = playlistClipsByPlaylistName[controllerName];
		if (!clips.Contains(clipToRemove)) {
			return; // this resource clip hasn't been played yet.
		}
		
		clips.Remove(clipToRemove);
		
		var hasDuplicateClip  = clips.Contains(clipToRemove);
		
		if (!hasDuplicateClip) {
			Resources.UnloadAsset(clipToRemove);
		}
    }

    /// <summary>
    /// Populates the sources with resource clip.
    /// </summary>
    /// <returns><c>true</c>, if sources with resource clip was populated, <c>false</c> otherwise.</returns>
    /// <param name="clipName">Clip name.</param>
    /// <param name="variation">Variation.</param>
    public static bool PopulateSourcesWithResourceClip(string clipName, SoundGroupVariation variation) {
        if (audioClipsByName.ContainsKey(clipName)) {
            //Debug.Log("clip already exists: " + clipName);
            return true; // work is done already!
        }

        var resAudioClip = Resources.Load(clipName) as AudioClip;

        if (resAudioClip == null) {
            MasterAudio.LogError("Resource file '" + clipName + "' could not be located.");
            return false;
        }

        if (!audioResourceTargetsByName.ContainsKey(clipName)) {
            Debug.LogError("No Audio Sources found to add Resource file '" + clipName + "'.");
            return false;
        } else {
            var sources = audioResourceTargetsByName[clipName];

            for (var i = 0; i < sources.Count; i++) {
                sources[i].clip = resAudioClip;
            }
        }

        audioClipsByName.Add(clipName, resAudioClip);
        return true;
    }

    public static void DeleteAudioSourceFromList(string clipName, AudioSource source) {
        if (!audioResourceTargetsByName.ContainsKey(clipName)) {
            Debug.Log("No Audio Sources found for Resource file '" + clipName + "'.");
            return;
        }

        var sources = audioResourceTargetsByName[clipName];
        sources.Remove(source);

        if (sources.Count == 0) {
            audioResourceTargetsByName.Remove(clipName);
        }
    }

    public static void UnloadClipIfUnused(string clipName) {
        if (!audioClipsByName.ContainsKey(clipName)) {
            // already removed.
            return;
        }

        var sources = new List<AudioSource>();

        if (audioResourceTargetsByName.ContainsKey(clipName)) {
            sources = audioResourceTargetsByName[clipName];

            AudioSource aSource = null;

            for (var i = 0; i < sources.Count; i++) {
                aSource = sources[i];
                var aVar = aSource.GetComponent<SoundGroupVariation>();

                if (aVar.IsPlaying) {
                    return; // still something playing
                }
            }
        }

        var clipToRemove = audioClipsByName[clipName];

        for (var i = 0; i < sources.Count; i++) {
            sources[i].clip = null;
        }

        audioClipsByName.Remove(clipName);
        Resources.UnloadAsset(clipToRemove);
    }
}
