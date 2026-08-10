using UnityEditor;
using UnityEngine;
using static PlayerDataTest;

[CustomPropertyDrawer(typeof(PlayerDataTest.Test))]
public class PlayerDataTestTestEditor : PropertyDrawer
{
    private static readonly string[] children =
    [
        "FieldName",
        "Type",
        "BoolValue",
        "NumType",
        "IntValue",
        "FloatValue",
        "StringValue",
        "StringType"
    ];

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var type = (TestType)property.FindPropertyRelative("Type").enumValueIndex;
        float y = position.y;

        foreach (var childName in children)
        {
            if (!ShouldShow(childName, type))
                continue;

            var child = property.FindPropertyRelative(childName);

            float height = EditorGUI.GetPropertyHeight(child);

            EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), child);

            y += height + EditorGUIUtility.standardVerticalSpacing;
        }

        EditorGUI.EndProperty();
    }

    private static bool ShouldShow(string propertyName, TestType type) => propertyName switch
    {
        "FieldName" => true,
        "Type" => true,

        "BoolValue" => type == TestType.Bool,

        "NumType" => type == TestType.Int || type == TestType.Float,

        "IntValue" => type == TestType.Int,

        "FloatValue" => type == TestType.Float,

        "StringValue" => type == TestType.String,

        "StringType" => type == TestType.String,

        _ => true
    };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var type = (TestType)property.FindPropertyRelative("Type").enumValueIndex;

        float height = 0f;

        foreach (var childName in children)
        {
            if (!ShouldShow(childName, type))
                continue;

            var child = property.FindPropertyRelative(childName);

            height += EditorGUI.GetPropertyHeight(child);
            height += EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }
}