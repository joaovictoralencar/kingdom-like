using HelloDev.IDs;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace KingdomLike.Utils
{
    [CreateAssetMenu(fileName = "ScriptableObjectWithID", menuName = "Scriptable Objects/ScriptableObjectWithID")]
    public class ScriptableObjectWithID : ScriptableObject
    {
        [FoldoutGroup("ID"), SerializeField] private ID_SO id;
        public ID_SO Id => id;

#if UNITY_EDITOR
        [FoldoutGroup("ID")]
        [ShowIf("@id == null")]
        [Button("Generate ID")]
        private void GenerateId()
        {
            string assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning($"'{name}' isn't a saved asset yet — save it before generating an ID.", this);
                return;
            }

            string folder = Path.GetDirectoryName(assetPath);
            string strippedName = name.StartsWith("SO_") ? name.Substring(3) : name;
            string idAssetName = $"SO_ID_{strippedName}";
            string idAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{idAssetName}.asset");

            ID_SO newId = CreateInstance<ID_SO>();
            AssetDatabase.CreateAsset(newId, idAssetPath);
            AssetDatabase.SaveAssets();

            var serializedObject = new SerializedObject(this);
            var idProperty = serializedObject.FindProperty("id");
            idProperty.objectReferenceValue = newId;
            serializedObject.ApplyModifiedProperties();
        }
#endif
    }
}