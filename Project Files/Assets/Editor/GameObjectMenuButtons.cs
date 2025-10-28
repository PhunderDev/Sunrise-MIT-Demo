using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;

public static class GameObjectMenuButtons
{
    [MenuItem("GameObject/SceneManager/DynamicSceneLoader", priority = -5)]
    private static void SpawnDynamicSceneLoad()
    {
        Debug.Log("XD");
        /*var item = Addressables.LoadAsset<GameObject>("Assets/Prefabs/Objects/Entry.prefab");
        var instance = PrefabUtility.InstantiatePrefab(item, Selection.activeTransform);

        Undo.RegisterCreatedObjectUndo(instance, $"Create {instance.name}");*/
    }

    [MenuItem("GameObject/SceneManager/SceneEntry", priority = -4)]
    private static void SpawnSceneEntry()
    {
        Debug.Log("XD 2");
    }
}
