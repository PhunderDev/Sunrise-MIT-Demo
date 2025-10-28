using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;



public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public bool CanTogglePause { get; private set; } = true;

    private void Awake()
    {
        Instance = this; 
    }

    public void Rebind(string Action)
    {
        Debug.Log("Rebinding");
        UIHandler.Instance.ToggleMappingMenu(true);
        CanTogglePause = false;
        StartRebinding(DefaultInputHandler.InputData.CurrentInputActionAsset, "Game", Action);
    }

    public void StartRebinding(InputActionAsset ActionAsset, string actionMapName, string actionName)
    {
        InputActionMap actionMap = ActionAsset.FindActionMap(actionMapName);
        if (actionMap == null)
        {
            Debug.LogError($"Action map '{actionMapName}' not found.");
            return;
        }

        var action = actionMap.FindAction(actionName);
        if (action == null)
        {
            Debug.LogError($"Action '{actionName}' not found in action map '{actionMapName}'.");
            return;
        }

        action.Disable();

        action.PerformInteractiveRebinding()
            .WithControlsExcluding("<Mouse>/position")
            .OnComplete(operation =>
            {
                action.Enable();
                int DeviceType = (int)DefaultInputHandler.InputData.CurrentInputDeviceType;
                string newBinding = action.bindings[DeviceType].effectivePath;
                PlayerPrefs.SetString($"{actionMapName}_{actionName}_binding_{DeviceType}", newBinding);
                PlayerPrefs.Save();

                Debug.Log($"Rebound '{actionName}' in '{actionMapName}' to '{newBinding}' for the input device of id '{DeviceType}'\n{newBinding}");

                CanTogglePause = true;
                LoadBindings(ActionAsset);
                UIHandler.Instance.ToggleMappingMenu(false);
                operation.Dispose();
            })
            .Start();
    }

    public void LoadBindings(InputActionAsset ActionAsset)
    {
        foreach (InputActionMap map in ActionAsset.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                string[] names = System.Enum.GetNames(typeof(DefaultInputHandler.InputDeviceType));
                for (int i = 0; i < names.Length; i++)
                {
                    string binding = PlayerPrefs.GetString($"{map.name}_{action.name}_binding_{i}", null);
                    if (!string.IsNullOrEmpty(binding))
                    {
                        //Debug.Log(binding);
                        action.ApplyBindingOverride(i, binding);
                    }
                }
            }
        }
    }
}
