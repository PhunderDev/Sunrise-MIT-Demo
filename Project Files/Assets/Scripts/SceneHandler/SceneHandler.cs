using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scene", menuName = "SceneManager/SceneHandler")]
public class SceneHandler : ScriptableObject
{
    public SceneField Scene;
    public SceneEntryScriptableObject[] Entries;
}

[System.Serializable]
public class SceneReferenceData
{
    public SceneHandler SceneHandler;
    public SceneEntryScriptableObject SceneEntry;
}
