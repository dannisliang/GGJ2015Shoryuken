using UnityEngine;
using System.Collections;

/// <summary>
    /// This class contains frequently used methods for audio in general.
    /// </summary>
public static class AudioUtil
{
	public static float GetDbFromFloatVolume(float vol) {
		return Mathf.Log(vol) * 20;
	}

	public static float GetFloatVolumeFromDb(float db) {
		return Mathf.Exp(db / 20);
	}

	/// <summary>
    /// This method will tell you the percentage of the clip that is done Playing (0-100).
    /// </summary>
    /// <param name="source">The Audio Source to calculate for.</param>
    /// <returns>(0-100 float)</returns>
    public static float GetAudioPlayedPercentage(AudioSource source)
    {
        if (source.clip == null || source.time == 0f)
        {
            return 0f;
        }

        var playedPercentage = (source.time / source.clip.length) * 100;
        return playedPercentage;
    }

    /// <summary>
    /// This method returns whether an AudioSource is paused or not.
    /// </summary>
    /// <param name="source">The Audio Source in question.</param>
    /// <returns>True or false</returns>
    public static bool IsAudioPaused(AudioSource source)
    {
        return !source.isPlaying && GetAudioPlayedPercentage(source) > 0f;
    }

	public static bool IsClipReadyToPlay(AudioClip clip) {
		#if UNITY_5_0
			return clip.loadType != AudioClipLoadType.Streaming;
		#else
			return clip.isReadyToPlay;
		#endif
	}
}