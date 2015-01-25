using UnityEngine;
using System.Collections;
using System.Collections.Generic;

    /// <summary>
    /// This class allows you to set defaults that each Master Audio prefab will use during its Start event when the Scene loads up, only if you set the values via code. Useful for setting global SFX and music levels, as well as other more granular settings.
    /// </summary>
public static class PersistentAudioSettings {
    private const string SFX_VOL_KEY = "MA_sfxVolume";
    private const string MUSIC_VOL_KEY = "MA_musicVolume";

    private static Dictionary<string, float> _groupVolumesByName = new Dictionary<string, float>();
    private static Dictionary<string, float> _busVolumesByName = new Dictionary<string, float>();

    /// <summary>
    /// Sets the bus's volume. During startup (Awake event), the Master Audio prefab will assign any buses that match to the levels you specify here. This will also set the Bus's Volume in the current Scene's Master Audio prefab, if a match exists.
    /// </summary>
    /// <param name="busName">Bus name</param>
    /// <param name="vol">Volume</param>
    public static void SetBusVolume(string busName, float vol) {
        if (_busVolumesByName.ContainsKey(busName)) {
            _busVolumesByName[busName] = vol;
        } else {
            _busVolumesByName.Add(busName, vol);
        }

        var ma = MasterAudio.SafeInstance;
        if (ma != null) {
            if (MasterAudio.GrabBusByName(busName) != null) {
                MasterAudio.SetBusVolumeByName(busName, vol);
            }
        }
    }

    /// <summary>
    /// Gets the bus volume (used by Master Audio prefab during Awake event to set persistent levels).
    /// </summary>
    /// <returns>The group volume.</returns>
    /// <param name="grpName">Group name.</param>
    public static float? GetBusVolume(string busName) {
        if (!_busVolumesByName.ContainsKey(busName)) {
            return null;
        }

        return _busVolumesByName[busName];
    }

    /// <summary>
    /// Sets the group's volume. During startup (Awake event), the Master Audio prefab will assign any Sound Groups that match to the levels you specify here. This will also set the Group's Volume in the current Scene's Master Audio prefab, if a match exists.
    /// </summary>
    /// <param name="grpName">Group name</param>
    /// <param name="vol">Volume</param>
    public static void SetGroupVolume(string grpName, float vol) {
        if (_groupVolumesByName.ContainsKey(grpName)) {
            _groupVolumesByName[grpName] = vol;
        } else {
            _groupVolumesByName.Add(grpName, vol);
        }

        var ma = MasterAudio.SafeInstance;
        if (ma != null) {
            if (MasterAudio.GrabGroup(grpName, false) != null) {
                MasterAudio.SetGroupVolume(grpName, vol);
            }
        }
    }

    /// <summary>
    /// Gets the group volume (used by Master Audio prefab during Awake event to set persistent levels).
    /// </summary>
    /// <returns>The group volume.</returns>
    /// <param name="grpName">Group name.</param>
    public static float? GetGroupVolume(string grpName) {
        if (!_groupVolumesByName.ContainsKey(grpName)) {
            return null;
        }

        return _groupVolumesByName[grpName];
    }

    /// <summary>
    /// Gets or sets the persistent Master Mixer Volume value. If this value is set (via code), each Master Audio prefab will read from it and set the Master Mixer Volume to this value, during the Scene's start event. This will also set the Master Mixer Volume in the current Scene's Master Audio prefab, if any.
    /// </summary>
    /// <value>The mixer volume.</value>
    public static float? MixerVolume {
        get {
            if (!PlayerPrefs.HasKey(SFX_VOL_KEY)) {
                return null;
            }

            return PlayerPrefs.GetFloat(SFX_VOL_KEY);
        }
        set {
            if (!value.HasValue) {
                PlayerPrefs.DeleteKey(SFX_VOL_KEY);
                return;
            }

            var newVal = value.Value;
            PlayerPrefs.SetFloat(SFX_VOL_KEY, newVal);
            var ma = MasterAudio.SafeInstance;
            if (ma != null) {
                MasterAudio.MasterVolumeLevel = newVal;
            }
        }
    }

    /// <summary>
    /// Gets or sets the Master Playlist Volume. If this value is set, each Master Audio prefab will read from it and set the Master Playlist Volume to this value, during the Scene's start event. This will also set the Master Playlist Volume in the current Scene's Master Audio prefab, if any.
    /// </summary>
    /// <value>The mixer volume.</value>
    public static float? MusicVolume {
        get {
            if (!PlayerPrefs.HasKey(MUSIC_VOL_KEY)) {
                return null;
            }

            return PlayerPrefs.GetFloat(MUSIC_VOL_KEY);
        }
        set {
            if (!value.HasValue) {
                PlayerPrefs.DeleteKey(MUSIC_VOL_KEY);
                return;
            }

            var newVal = value.Value;
            PlayerPrefs.SetFloat(MUSIC_VOL_KEY, newVal);
            var ma = MasterAudio.SafeInstance;
            if (ma != null) {
                MasterAudio.PlaylistMasterVolume = newVal;
            }
        }
    }

    public static void RestoreMasterSettings() {
        if (MixerVolume.HasValue) {
            MasterAudio.MasterVolumeLevel = MixerVolume.Value;
        }

        if (MusicVolume.HasValue) {
            MasterAudio.PlaylistMasterVolume = MusicVolume.Value;
        }
    }
}