using System;
using UnityEditor;
using UnityEngine;

namespace WorldWeaver.Editor
{
    public abstract class ChannelInfoEditor<T> : PropertyDrawer where T : Enum
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int index = GetArrayIndex(property);
            Array values = Enum.GetValues(typeof(T));

            if (index >= 0 && index < values.Length)
            {
                T channel = (T)values.GetValue(index);
                label.text = ObjectNames.NicifyVariableName(channel.ToString());
            }

            SerializedProperty clipProperty = property.FindPropertyRelative("clip");

            if (clipProperty != null)
                if (clipProperty.objectReferenceValue == null || string.IsNullOrEmpty(clipProperty.objectReferenceValue.name))
                    label.text += " (None)";
                else
                    label.text += $" ({clipProperty.objectReferenceValue.name})";
            
            EditorGUI.PropertyField(position, property, label, true);
        }

        private int GetArrayIndex(SerializedProperty property)
        {
            string path = property.propertyPath;
            //Debug.Log(path); channelInfos.Array.data[?]

            if (int.TryParse(path[24..path.IndexOf(']', 24)], out int i))
                return i;

            return -1;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, true);
    }
}