using PlasticGui;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TransitionArrayElement))]
public class TransitionPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CustomAnimator animator = property.serializedObject.targetObject as CustomAnimator;
        List<string> localStateNames = animator.stateNames;

        EditorGUI.BeginProperty(position, label, property);
        // Draw the label
        Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, "To State");

        // Draw the dropdown for ToState
        SerializedProperty toStateProp = property.FindPropertyRelative("ToState");
        int selectedIndex = Mathf.Max(0, System.Array.IndexOf(localStateNames.ToArray(), toStateProp.stringValue));
        Rect dropdownRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

        selectedIndex = EditorGUI.Popup(dropdownRect, selectedIndex, localStateNames.ToArray());
        toStateProp.stringValue = localStateNames[selectedIndex];

        // Draw the AnimationName field
        SerializedProperty animationNameProp = property.FindPropertyRelative("AnimationName");
        Rect animRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(animRect, animationNameProp, new GUIContent("Animation Name"));

        EditorGUI.EndProperty();
    }
}

[CustomEditor(typeof(CustomAnimator))]
public class CustomAnimatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CustomAnimator CustomAnimator = (CustomAnimator)serializedObject.targetObject;

        if (CustomAnimator.stateNames == null)
        {
            CustomAnimator.stateNames = new List<string> { "" };
        }

        base.OnInspectorGUI();
        if (GUILayout.Button("Update dropdown selection"))
        {
            CustomAnimator.stateNames.Clear();
            SerializedProperty AnimationStates = serializedObject.FindProperty("AnimationStates");
            for (int i = 0; i < AnimationStates.arraySize; i++)
            {
                CustomAnimator.stateNames.Add(AnimationStates.GetArrayElementAtIndex(i).FindPropertyRelative("StateName").stringValue);
            }
            if (CustomAnimator.stateNames.Count == 0)
            {
                CustomAnimator.stateNames.Add("");
            }
            Debug.Log(CustomAnimator.stateNames.Count);
        }
    }
}