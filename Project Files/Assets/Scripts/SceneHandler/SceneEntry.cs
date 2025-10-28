using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneEntry : MonoBehaviour
{
    public SceneReferenceData sceneReferenceData;
    [SerializeField]
    private LayerMask PlayerLayer;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (LevelManager.Instance.IsChangingScene) {
            LevelManager.Instance.IsChangingScene = false;
            return;
        }

        if(CompareLayers(PlayerLayer, collision.gameObject.layer)) {

            Vector3 PlayerOffset = transform.position - collision.gameObject.transform.position;
            LevelManager.Instance.ChangeScene(sceneReferenceData.SceneEntry.ExitReference.SceneHandler.Scene.SceneName, new Vector2(PlayerOffset.x, PlayerOffset.y), sceneReferenceData.SceneEntry.ExitReference.SceneEntry.WorldPosition);
        }
    }

    private bool CompareLayers(LayerMask LayerFromArray, int LayerFromInt)
    {
        return ((LayerFromArray.value & (1 << LayerFromInt)) > 0);
    }
}
