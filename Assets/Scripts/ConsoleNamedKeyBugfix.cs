using UnityEngine;
using System.Collections;

public class ConsoleNamedKeyBugfix : MonoBehaviour {

	#if UNITY_EDITOR
	// Fixes the annoying issue described here: http://forum.unity3d.com/threads/158676-!dest-m_MultiFrameGUIState-m_NamedKeyControlList/page2
	// seems to happen because ConsoleGUI creates and destroys a GUI textfield and the GUI doesn't know what to give focus to
	void OnGUI() {
		string controlName = gameObject.GetHashCode().ToString();
		GUI.SetNextControlName(controlName);
		Rect bounds = new Rect(0,0,0,0);
		GUI.TextField(bounds, "", 0);
	}
	#endif
}