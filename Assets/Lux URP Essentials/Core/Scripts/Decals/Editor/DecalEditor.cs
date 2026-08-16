#if UNITY_EDITOR
//  Editor only. Guarded so the file also stays out of player builds if this
//  package ever ends up inside a user-provided .asmdef, where Unity's
//  "Editor folder" rule does not apply.
using UnityEngine;
using UnityEditor;

namespace LuxURPEssentials
{
	[CustomEditor(typeof(Decal))]
	public class DecalEditor : Editor {
	    public override void OnInspectorGUI() {
	    	Decal script = (Decal)target;
	        if (GUILayout.Button("Align")) {
	            script.AlignDecal();
	        }
	    }
	}
}
#endif // UNITY_EDITOR
