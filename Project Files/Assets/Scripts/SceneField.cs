using UnityEngine;

[System.Serializable]
public class SceneField
{
    [SerializeField]
    private Object sceneAsset;
    [SerializeField]
    private string sceneName = "";

    public string SceneName
    {
        get { return sceneName; }
    }

    public Object SceneAsset
    {
        get { return sceneAsset; }
    }

    private void OnValidate()
    {
        if (sceneAsset != null) sceneName = sceneAsset.name;
    }
}