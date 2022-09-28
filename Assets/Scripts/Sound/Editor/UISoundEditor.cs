using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace GMReloaded
{
	[CustomEditor(typeof(UISound))]
	public class UISoundEditor : Editor 
	{
		private List<string> _choices = new List<string>();

		protected void OnEnable()
		{

		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			List<FieldInfo> _fields = GetConstants(typeof(Config.Sounds));
			_choices.Clear();

			_choices.Add("Choose ...");

			foreach(var _field in _fields)
			{
				_choices.Add(_field.GetRawConstantValue().ToString());
			}

			string[] _choicesArray = _choices.ToArray();

			UISound _uiSound = target as UISound;

			_uiSound.selectedSoundID = EditorGUILayout.Popup("Sound ID", _uiSound.selectedSoundID, _choicesArray);

			string _selectedSoundID = _choicesArray[_uiSound.selectedSoundID];

			if(GUI.changed || _selectedSoundID != _uiSound.soundID)
			{
				_uiSound.soundID = _selectedSoundID;

				EditorUtility.SetDirty(target);
			}
		}

		private List<FieldInfo> GetConstants(Type type)
		{
			FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
				BindingFlags.Static | BindingFlags.FlattenHierarchy);

			return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
		}

	}
}
