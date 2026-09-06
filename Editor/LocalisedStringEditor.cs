using TeamCherry.Localization;
using UnityEditor;
using UnityEngine;

namespace WorldWeaver.Editor 
{
    [CustomPropertyDrawer(typeof(LocalisedString))]
    public class LocalisedStringEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty keyProperty = property.FindPropertyRelative("Key");

            if (keyProperty != null && !string.IsNullOrEmpty(keyProperty.stringValue))
            {
                label = new GUIContent($"{label.text} ({keyProperty.stringValue})");
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }   
}