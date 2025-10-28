using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneEntryScriptableObject : ScriptableObject
{
    public string EntryName;
    public SceneReferenceData ExitReference;
    public Vector2 Direction;
    public Vector2 WorldPosition;
}
