using GlobalEnums;
using UnityEditor;
using UnityEngine;
using WorldWeaver.Data;

// Some very cursed code
[CustomPropertyDrawer(typeof(WeaverAchievement))]
public class WeaverAchievementEditor : PropertyDrawer
{
    private static readonly string[] children =
    [
        "PlatformKey",
        "Type",
        
        "Icon",

        "NormalTitle",
        "NormalDescription",
        
        "HiddenIcon",
        "HiddenTitle",
        "HiddenDescription",
        
        "AwardedIcon",
        "AwardedTitle",
        "AwardedDescription"
    ];

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var typeProperty = property.FindPropertyRelative("Type");
        var type = (AchievementType)typeProperty.enumValueIndex - 1;

        float y = position.y;

        var foldoutRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);

        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // Don't draw children when collapsed
        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;

        foreach (var childName in children)
        {
            var child = property.FindPropertyRelative(childName);

            if (!ShouldShow(child, property, type))
                continue;

            float height = EditorGUI.GetPropertyHeight(child, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), child, new GUIContent(ObjectNames.NicifyVariableName(childName).Replace("Normal ", "")), true);
            y += height + EditorGUIUtility.standardVerticalSpacing;
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        var type = (AchievementType)property.FindPropertyRelative("Type").enumValueIndex - 1;

        float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        foreach (var childName in children)
        {
            var child = property.FindPropertyRelative(childName);

            if (!ShouldShow(child, property, type))
                continue;

            height += EditorGUI.GetPropertyHeight(child, true);
            height += EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }

    private static bool ShouldShow(SerializedProperty? property, SerializedProperty achievementProperty, AchievementType type)
    {
        if (property == null)
            return false;

        if (type != AchievementType.Normal && type != AchievementType.Hidden)
            return true; // Show all for custom impl

        return property.name switch
        {
            "PlatformKey" => true,
            "Type" => true,

            "Icon" => true,
            "NormalTitle" => true,
            "NormalDescription" => true,

            "HiddenIcon" => type == AchievementType.Hidden,
            "HiddenTitle" => type == AchievementType.Hidden,
            "HiddenDescription" => type == AchievementType.Hidden,

            "AwardedIcon" => true,
            "AwardedTitle" => achievementProperty.FindPropertyRelative("AwardedIcon")?.objectReferenceValue != null,
            "AwardedDescription" => achievementProperty.FindPropertyRelative("AwardedIcon")?.objectReferenceValue != null,

            _ => true
        };
    }
}