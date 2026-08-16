#if UNITY_EDITOR
//  Editor only. Guarded so the file also stays out of player builds if this
//  package ever ends up inside a user-provided .asmdef, where Unity's
//  "Editor folder" rule does not apply.
using UnityEngine;
using UnityEditor;

namespace LuxURPEssentials
{
    [CustomEditor(typeof(ApplyProceduralTextureProperties))]
    public class ApplyProceduralTexturePropertiesEditor : Editor {
        public override void OnInspectorGUI() {
        	DrawDefaultInspector();

        	ApplyProceduralTextureProperties script = (ApplyProceduralTextureProperties)target;

        	if(GUILayout.Button("Apply")) {
        		script.SyncMatWithProceduralTextureAsset();
        	}
        }
    }
}
#endif // UNITY_EDITOR
