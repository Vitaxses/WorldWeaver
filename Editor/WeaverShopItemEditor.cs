using UnityEditor;
using WorldWeaver.Data;

namespace WorldWeaver.Editor
{
    [CustomEditor(typeof(WeaverShopItem))]
    public class WeaverShopItemEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.name == "cost" || property.name == "costReference" || property.name == "currencyType")
                    continue;
                    
                if (property.name == "rosaryCost" || property.name == "shellShardCost")
                    continue;

                if (property.name == "typeFlags")
                {
                    EditorGUILayout.PropertyField(property, true);
                    
                    EditorGUILayout.Space(4);

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rosaryCost"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shellShardCost"));
                    
                    EditorGUILayout.Space(4);
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

}