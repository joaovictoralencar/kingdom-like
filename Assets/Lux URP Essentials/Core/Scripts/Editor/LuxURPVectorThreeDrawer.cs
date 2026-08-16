#if UNITY_EDITOR
//  Editor only. Guarded so the file also stays out of player builds if this
//  package ever ends up inside a user-provided .asmdef, where Unity's
//  "Editor folder" rule does not apply.
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace LuxURPEssentials
{
	public class LuxURPVectorThreeDrawer : MaterialPropertyDrawer {

		public override void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
			
		//	Needed since Unity 2019
			EditorGUIUtility.labelWidth = 0;

			Vector3 vec3value = prop.vectorValue;

			GUILayout.Space(-18);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.BeginVertical();
				EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(label);
					GUILayout.Space(-1);
					vec3value = EditorGUILayout.Vector3Field ("", vec3value);
				EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			// GUILayout.Space(2);
			if (EditorGUI.EndChangeCheck ()) {
				prop.vectorValue = vec3value;
			}
		}
	}
}
#endif // UNITY_EDITOR
