using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(WorldWeaver.Data.MonoBehaviours.Pane))]
public class WeaverCreditsPaneEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty keyProperty = property.FindPropertyRelative("Label");

        if (string.IsNullOrEmpty(keyProperty.FindPropertyRelative("Key").stringValue))
            keyProperty = property.FindPropertyRelative("Name");

        keyProperty = keyProperty.FindPropertyRelative("Key");

        if (keyProperty != null && !string.IsNullOrEmpty(keyProperty.stringValue))
        {
            label = new GUIContent($"Pane {label.text.Replace("Element ", "")} ({keyProperty.stringValue})");
        }

        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }
}