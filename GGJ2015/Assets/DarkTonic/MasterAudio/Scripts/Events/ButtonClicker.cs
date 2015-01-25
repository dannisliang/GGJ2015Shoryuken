using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Dark Tonic/Master Audio/Button Clicker")]
public class ButtonClicker : MonoBehaviour {
	public const float SMALL_SIZE_MULTIPLIER = 0.9f;
	public const float LARGE_SIZE_MULTIPLIER = 1.1f;
	
	public bool resizeOnClick = true;
	public bool resizeClickAllSiblings = false;
	public bool resizeOnHover = false;
	public bool resizeHoverAllSiblings = false;
	public string mouseDownSound = string.Empty;
	public string mouseUpSound = string.Empty;
	public string mouseClickSound = string.Empty;
	public string mouseOverSound = string.Empty;
	public string mouseOutSound = string.Empty;
	
	private Vector3 originalSize;
	private Vector3 smallerSize;
	private Vector3 largerSize;
	private Transform trans;
	private Dictionary<Transform, Vector3> siblingClickScaleByTransform = new Dictionary<Transform, Vector3>();
	private Dictionary<Transform, Vector3> siblingHoverScaleByTransform = new Dictionary<Transform, Vector3>();
	
	// This script can be triggered from NGUI clickable elements only. 
	void Awake() {
		this.trans = this.transform;
		this.originalSize = trans.localScale;
		this.smallerSize = this.originalSize * SMALL_SIZE_MULTIPLIER;
		this.largerSize = this.originalSize * LARGE_SIZE_MULTIPLIER;
		
		var holder = this.trans.parent;
		
		if (resizeOnClick && resizeClickAllSiblings && holder != null) {
			for (var i = 0; i < holder.transform.childCount; i++) {
				var aChild = holder.transform.GetChild(i);
				siblingClickScaleByTransform.Add(aChild, aChild.localScale);
			}
		}
		
		if (resizeOnHover && resizeHoverAllSiblings && holder != null) {
			for (var i = 0; i < holder.transform.childCount; i++) {
				var aChild = holder.transform.GetChild(i);
				siblingHoverScaleByTransform.Add(aChild, aChild.localScale);
			}
		}
	}
	
	void OnPress(bool isDown) {
		if (isDown) {
			if (enabled) {
				MasterAudio.PlaySoundAndForget(mouseDownSound);
				
				if (resizeOnClick) {			
					trans.localScale = this.smallerSize;
					
					var scales = siblingClickScaleByTransform.GetEnumerator();
					
					while (scales.MoveNext()) {
						scales.Current.Key.localScale = scales.Current.Value * SMALL_SIZE_MULTIPLIER;
					}
				}
			}
		} else {
			if (enabled) {
                MasterAudio.PlaySoundAndForget(mouseUpSound);
			}
			// still want to restore size if disabled
			
			if (resizeOnClick) {			
				trans.localScale = this.originalSize;
				
				var scales = siblingClickScaleByTransform.GetEnumerator();
				
				while (scales.MoveNext()) {
					scales.Current.Key.localScale = scales.Current.Value;
				}
			}
		}
	}
	
	void OnClick() {
		if (enabled) {
            MasterAudio.PlaySoundAndForget(mouseClickSound);
		}
	}
	
	void OnHover(bool isOver) {
		if (isOver) {
			if (enabled) {
                MasterAudio.PlaySoundAndForget(mouseOverSound);
				
				if (resizeOnHover) {
					trans.localScale = this.largerSize;
					
					var scales = siblingHoverScaleByTransform.GetEnumerator();
					
					while (scales.MoveNext()) {
						scales.Current.Key.localScale = scales.Current.Value * LARGE_SIZE_MULTIPLIER;
					}
				}
			}
		} else {
			if (enabled) {
                MasterAudio.PlaySoundAndForget(mouseOutSound);
			}
			// still want to restore size if disabled
			
			if (resizeOnHover) {
				trans.localScale = this.originalSize;
				
				var scales = siblingHoverScaleByTransform.GetEnumerator();
				
				while (scales.MoveNext()) {
					scales.Current.Key.localScale = scales.Current.Value;
				}
			}
		}
	}
}
