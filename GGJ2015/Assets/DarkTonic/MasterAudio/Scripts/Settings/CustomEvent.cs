using System;
using System.Collections;

[Serializable]
public class CustomEvent{
	public string EventName;
	public string ProspectiveName;
	public bool eventExpanded = true;
	public MasterAudio.CustomEventReceiveMode eventReceiveMode = MasterAudio.CustomEventReceiveMode.Always;
	public float distanceThreshold = 1f;
    public int frameLastFired = -1;

	public CustomEvent(string eventName) {
		EventName = eventName;
		ProspectiveName = eventName;
	}
}
