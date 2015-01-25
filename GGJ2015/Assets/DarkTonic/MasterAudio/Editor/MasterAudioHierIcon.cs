using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public class MasterAudioHierIcon : MonoBehaviour {
    static Texture2D MAicon;
	static Texture2D PCicon;

    static MasterAudioHierIcon() {
		MAicon = AssetDatabase.LoadAssetAtPath("Assets/Gizmos/MasterAudio Icon.png", typeof(Texture2D)) as Texture2D;
		PCicon = AssetDatabase.LoadAssetAtPath("Assets/Gizmos/PlaylistController Icon.png", typeof(Texture2D)) as Texture2D;

		if (MAicon == null) {
            return;
        } 

        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
        EditorApplication.RepaintHierarchyWindow();
    }

    static void HierarchyItemCB(int instanceID, Rect selectionRect) {
        GameObject masterAudioGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

		if (masterAudioGameObject == null) {
			return;
		}   

		if (MAicon != null && masterAudioGameObject.GetComponent<MasterAudio>() != null) {
            Rect r = new Rect(selectionRect);
            r.x = r.width - 5;

			GUI.Label(r, MAicon);
		} else if (PCicon != null && masterAudioGameObject.GetComponent<PlaylistController>() != null) {
			Rect r = new Rect(selectionRect);
			r.x = r.width - 5;
			
			GUI.Label(r, PCicon); 
		}
	}
}
