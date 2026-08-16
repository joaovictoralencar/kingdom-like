#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace LuxURPEssentials
{
    public sealed class LuxTooltipDrawer : MaterialPropertyDrawer
    {
        private readonly string tooltip;

        public LuxTooltipDrawer(string tooltip)
        {
            this.tooltip = tooltip?.Replace("\\n", "\n") ?? string.Empty;
        }

        public override float GetPropertyHeight(
            MaterialProperty property,
            string label,
            MaterialEditor editor)
        {
            return MaterialEditor.GetDefaultPropertyHeight(property);
        }

        public override void OnGUI(
            Rect position,
            MaterialProperty property,
            string label,
            MaterialEditor editor)
        {

            editor.DefaultShaderProperty(position, property, label);

            if (Event.current.type == EventType.Repaint &&
                !string.IsNullOrEmpty(tooltip))
            {
                GUI.Label(
                    position,
                    new GUIContent(string.Empty, tooltip));
            }
        }
    }
}

#endif