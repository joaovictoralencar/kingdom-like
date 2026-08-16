#if UNITY_EDITOR
//  Editor only. Guarded so the file also stays out of player builds if this
//  package ever ends up inside a user-provided .asmdef, where Unity's
//  "Editor folder" rule does not apply.
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace LuxURPEssentials
{
	public class LuxURPVectorTwoDrawer : MaterialPropertyDrawer {

		public override void OnGUI (Rect position, MaterialProperty prop, string label, MaterialEditor editor) {
			
		//	Needed since Unity 2019
			EditorGUIUtility.labelWidth = 0;

			Vector4 vec4value = prop.vectorValue;

			GUILayout.Space(-18);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.BeginVertical();
				EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(label);
					GUILayout.Space(-1);
					vec4value = EditorGUILayout.Vector2Field ("", vec4value);
				EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
			// GUILayout.Space(2);
			if (EditorGUI.EndChangeCheck ()) {
				prop.vectorValue = vec4value;
			}
		}
	}
}
#endif // UNITY_EDITOR
