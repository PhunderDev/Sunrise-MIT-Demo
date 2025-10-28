using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [HideInInspector]
    public bool IsChangingScene = false;

    private Vector2 PlayerEntryOffset;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeScene(string SceneName, Vector2 PlayerOffset, Vector2 ExitPos)
    {
        if (IsChangingScene) return;

        PlayerEntryOffset = PlayerOffset;
        string InitialSceneName = SceneManager.GetSceneAt(1).name;
        StartCoroutine(LoadSceneAsync(SceneName));
        if (InitialSceneName != "") StartCoroutine(OnSceneChanged(InitialSceneName, ExitPos));
    }

    private IEnumerator LoadSceneAsync(string SceneName)
    {
        IsChangingScene = true;
        UIHandler.Instance.SceneTransition(true);
        AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        AsyncOperation sceneUnloading = null;
        string CurrentScene = SceneManager.GetSceneAt(1).name;
        if (CurrentScene != "") sceneUnloading = SceneManager.UnloadSceneAsync(CurrentScene);
        sceneLoading.allowSceneActivation = false;


        while(sceneLoading.progress < 0.9f || !UIHandler.Instance.TransitionReachedApogee) yield return null;

        if(sceneUnloading != null) sceneUnloading.allowSceneActivation = true;
        sceneLoading.allowSceneActivation = true;
        UIHandler.Instance.SceneTransition(false);
    }

    private IEnumerator OnSceneChanged(string PreviousSceneName, Vector2 TargetPos)
    {
        while (SceneManager.GetSceneAt(1).name == PreviousSceneName) yield return null;
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        Player.transform.position = new Vector3(TargetPos.x - PlayerEntryOffset.x, TargetPos.y - PlayerEntryOffset.y, Player.transform.position.z);
        Debug.Log("Exiting at: " + TargetPos);
    }
}
