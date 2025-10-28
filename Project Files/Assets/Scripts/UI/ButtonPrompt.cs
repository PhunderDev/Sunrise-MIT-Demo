using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonPrompt : MonoBehaviour
{
    [Tooltip("0: Image | 1: Text")]
    [SerializeField] private int DisplayMode = 0;
    private Image PromptImage;
    private TMP_Text PromptText;

    private void OnEnable()
    {
        DefaultInputHandler.Instance.UpdatePrompt.AddListener(() => UpdatePrompt());
        if (DisplayMode == 0)
        {
            PromptImage = GetComponentInChildren<Image>();
        }
        else if (DisplayMode == 1)
        {
            PromptText = GetComponentInChildren<TMP_Text>();
        }
    }

    void UpdatePrompt()
    {
        Debug.Log("Prompt icon changed");
    }
}
