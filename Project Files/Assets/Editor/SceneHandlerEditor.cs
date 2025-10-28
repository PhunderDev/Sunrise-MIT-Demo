using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#region Scene Scriptable Objects Editor

[CustomEditor(typeof(SceneHandler))]
public class SceneHandlerEditor : Editor
{
    SerializedProperty Scene;
    List<SceneEntryScriptableObject> SubAssets = new List<SceneEntryScriptableObject>();

    private void OnEnable()
    {
        Scene = serializedObject.FindProperty("Scene");
        //sceneEntries = serializedObject.FindProperty("sceneEntries");

        LoadSubAssets();
    }

    private void LoadSubAssets()
    {
        SubAssets.Clear();
        SceneHandler handler = (SceneHandler)target;

        // Find all sub-assets of type SceneEntryScriptableObject
        
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(target));
        List<SceneEntryScriptableObject> Entries = new List<SceneEntryScriptableObject>();
        foreach (Object asset in assets)
        {
            if (asset is SceneEntryScriptableObject entry)
            {
                SubAssets.Add(entry);
                Entries.Add(entry);
            }
        }
        handler.Entries = Entries.ToArray();

        EditorUtility.SetDirty(target);
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(Scene);

        foreach (SceneEntryScriptableObject entry in SubAssets)
        {
            DrawSceneEntryScriptableObject(entry);
        }




        if (GUILayout.Button("+"))
        {
           // Debug.Log("Add New Entry/Exit to scene: " + target.name);

            SceneEntryScriptableObject newSceneEntryScriptableObject = ScriptableObject.CreateInstance<SceneEntryScriptableObject>();
            newSceneEntryScriptableObject.name = "Entry/Exit";
            newSceneEntryScriptableObject.EntryName = newSceneEntryScriptableObject.name;
            AssetDatabase.AddObjectToAsset(newSceneEntryScriptableObject, target);
            AssetDatabase.SaveAssets();
            LoadSubAssets();
        }

        serializedObject.ApplyModifiedProperties();
    }




    private void DrawSceneEntryScriptableObject(SceneEntryScriptableObject entry)
    {
        int OutsideMargin = 2;

        Rect RectOutline = EditorGUILayout.BeginVertical();
        RectOutline.xMin -= OutsideMargin;
        RectOutline.width += OutsideMargin;
        RectOutline.height += OutsideMargin;

        Rect ActualRect = RectOutline;
        ActualRect.yMin += 1;
        ActualRect.xMin += 1;
        ActualRect.width -= 1;
        ActualRect.height -= 1;

        EditorGUI.DrawRect(RectOutline, new Color(0f, 0f, 0f, 1.0f));

        EditorGUI.DrawRect(ActualRect, new Color(0.27f, 0.27f, 0.27f, 1.0f));

        GUILayout.Space(5);

        //EditorGUILayout.LabelField("Scene Entry: " + entry.name);
        Editor editor = CreateEditor(entry);
        editor.OnInspectorGUI();

        GUILayout.Space(5);


        EditorGUILayout.EndVertical();
        GUILayout.Space(10);
    }
}







[CustomEditor(typeof(SceneEntryScriptableObject))]
public class SceneEntryScriptableObjectEditor : Editor
{

    SerializedProperty EntryName;
    SerializedProperty ExitReference;
    SerializedProperty Direction;
    SerializedProperty WorldPosition;

    private void OnEnable()
    {
        EntryName = serializedObject.FindProperty("EntryName");
        ExitReference = serializedObject.FindProperty("ExitReference");
        Direction = serializedObject.FindProperty("Direction");
        WorldPosition = serializedObject.FindProperty("WorldPosition");
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        EditorGUILayout.PropertyField(EntryName);
        EditorGUILayout.PropertyField(ExitReference);
        EditorGUILayout.PropertyField(Direction);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(WorldPosition);
        EditorGUI.EndDisabledGroup();


        GUILayout.Space(10);

        if (GUILayout.Button("Delete"))
        {
            Debug.Log("Remove an Entry/Exit from SceneHandler: " + target.name);

            if (Selection.objects[0] == target) Selection.objects = new Object[0];

            AssetDatabase.RemoveObjectFromAsset(target);

            AssetDatabase.SaveAssets();
        }

        serializedObject.ApplyModifiedProperties();

        if (target.name != EntryName.stringValue)
        {
            target.name = EntryName.stringValue;
            ForceRefreshView();
        }



        EditorUtility.SetDirty(target);
        Repaint();
    }

    private void ForceRefreshView()
    {
        Debug.Log("Refreshing View");
        SceneEntryScriptableObject PlaceHolderEntry = ScriptableObject.CreateInstance<SceneEntryScriptableObject>();
        PlaceHolderEntry.name = "PlaceHolder";
        AssetDatabase.AddObjectToAsset(PlaceHolderEntry, AssetDatabase.GetAssetPath(target));
        AssetDatabase.Refresh();
        AssetDatabase.RemoveObjectFromAsset(PlaceHolderEntry);
    }
}


[CustomEditor(typeof(SceneEntry))]
public class SceneEntryWorldObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();




        if (GUILayout.Button("Update World Position"))
        {
            Debug.Log("Updating The World Position Of: " + target.name);

            SceneEntry Entry = (SceneEntry)target;
            Entry.sceneReferenceData.SceneEntry.WorldPosition = Entry.transform.position;
            EditorUtility.SetDirty(target);
        }
    }
}



#endregion


#region Scene Scriptable Objects Reference Drawer

[CustomPropertyDrawer(typeof(SceneReferenceData))]
public class SceneReferenceDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float width = (position.width + EditorGUIUtility.labelWidth) / 2;
        float EntryWidth = (position.width - EditorGUIUtility.labelWidth) / 2;

        // Calculate rects
        var handlerRect = new Rect(position.x, position.y, width - 5, EditorGUIUtility.singleLineHeight);
        var entryRect = new Rect(position.x + width + 5, position.y, EntryWidth - 5, EditorGUIUtility.singleLineHeight);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        SerializedProperty SceneHandlerProperty = property.FindPropertyRelative("SceneHandler");

        if (SceneHandlerProperty.objectReferenceValue == null)
        {
            EditorGUI.PropertyField(handlerRect, SceneHandlerProperty, new GUIContent(property.name));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.Popup(entryRect, 0, new string[1] { "None" });
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUI.PropertyField(handlerRect, SceneHandlerProperty, new GUIContent(property.name));

            SceneHandler sceneHandler = (SceneHandler)SceneHandlerProperty.objectReferenceValue;
            SerializedProperty SceneEntryScriptableObjectProp = property.FindPropertyRelative("SceneEntry");

            var entries = sceneHandler.Entries;
            string[] entryNames = new string[entries.Length];
            for (int i = 0; i < entries.Length; i++)
            {
                entryNames[i] = entries[i].EntryName;
            }

            int currentIndex = 0;
            if (SceneEntryScriptableObjectProp.objectReferenceValue != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i] == SceneEntryScriptableObjectProp.objectReferenceValue)
                    {
                        currentIndex = i;
                        break;
                    }
                }
            }

            int newIndex = EditorGUI.Popup(entryRect, currentIndex, entryNames);
            if (newIndex >= 0 && newIndex < entries.Length)
            {
                SceneEntryScriptableObjectProp.objectReferenceValue = entries[newIndex];
            }
        }

        EditorGUI.EndProperty();
    }
}









[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty sceneAssetProp = property.FindPropertyRelative("sceneAsset");
        SerializedProperty sceneNameProp = property.FindPropertyRelative("sceneName");

        EditorGUI.BeginChangeCheck();
        Object sceneAsset = EditorGUI.ObjectField(position, label, sceneAssetProp.objectReferenceValue, typeof(SceneAsset), false);

        if (EditorGUI.EndChangeCheck())
        {
            sceneAssetProp.objectReferenceValue = sceneAsset;

            if (sceneAsset != null)
            {
                sceneNameProp.stringValue = sceneAsset.name;
            }
        }

        EditorGUI.EndProperty();
    }
}



#endregion
