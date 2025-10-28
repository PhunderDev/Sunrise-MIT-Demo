using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[System.Serializable]
public class KeyboardKeySprite {
    public Key KeyboardKey;
    public Sprite IndicatorSprite;
}


[System.Serializable]
public class GamepadButtonSprite
{
    public GamepadButton KeyboardKey;
    public Sprite IndicatorSprite;
}


[System.Serializable]
public class KeyboardInputButtonIndicator
{
    public DefaultInputHandler.InputDeviceType TypeOfInput;
    public KeyboardKeySprite key;
}


[System.Serializable]
public class GamepadInputButtonIndicator
{
    public DefaultInputHandler.InputDeviceType TypeOfInput;
    public GamepadButtonSprite Button;
}


public class UIHandler : MonoBehaviour
{
    public static UIHandler Instance { get; private set; }

    [SerializeField]
    private GameObject PauseMenu, RemappingPopUp;

    public bool GamePaused;

    //public KeyboardInputButtonIndicator[] KeyboardButtonIndicators, GamepadButtonIndicators;

    public string IndicatorsPath;

    [SerializeField]
    private Animator TransitionAnimator;

    [HideInInspector]
    public bool IsPlayingTransition;

    [HideInInspector]
    public bool TransitionReachedApogee = false;

    private void Awake()
    {
        if(Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        TransitionAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        if(IsPlayingTransition) TransitionReachedApogee = TransitionAnimator.GetCurrentAnimatorStateInfo(0).IsName("HalfWay");

        //if(TransitionReachedApogee) Time.timeScale = 0f;
        //else Time.timeScale = 1f;
    }

    private Sprite[] LoadSpritesFromSpriteSheet(string SpriteSheetPath, string[] spriteNames)
    {
        Sprite[] all = Resources.LoadAll<Sprite>(SpriteSheetPath);

        Sprite[] FoundSprites = new Sprite[spriteNames.Length];
        string[] Prefixes = new string[2] { "Keyboard_", "Xbox_"};

        for (int i = 0; i < spriteNames.Length; i++)
        {
            //Debug.Log(spriteNames[i]);
            foreach (Sprite s in all)
            {
                if (s.name == Prefixes[i] + spriteNames[i])
                {
                    //Debug.Log("Found! " + s.name);
                    FoundSprites[i] = s;
                    break;
                }
            }
        }
        return FoundSprites;
    }


    public void SceneTransition(bool Toggle)
    {
        if(Toggle)
        {
            TransitionAnimator.Play("Start");
        } else
        {
            TransitionAnimator.Play("End");
        }

        TransitionReachedApogee = false;
        IsPlayingTransition = Toggle;
    }

    public InputIndicatorIconSet SpriteArrayToIconSet(Sprite[] sprites)
    {
        InputIndicatorIconSet NewSet = new InputIndicatorIconSet();

        NewSet.CurrentInputIndicatorSprite_Keyboard = sprites[0];
        NewSet.CurrentInputIndicatorSprite_Xbox = sprites[1];

        return NewSet;
    }

    public InputIndicatorIconSet GetRightInputIndicatorSprites(string ActionName)
    {
        InputIndicatorIconSet CorrectSpriteSet;

        List<string> ActionButtonNames = new List<string>();
        for(int i = 0; i < System.Enum.GetNames(typeof(DefaultInputHandler.InputDeviceType)).Length; i++)
        {
            //Debug.Log(i);
            string NewActionButton;

            try
            {
                NewActionButton = DefaultInputHandler.InputData.CurrentInputActionAsset.FindAction(ActionName).bindings[i].path.Split("/")[1];
                //Debug.Log(DefaultInputHandler.InputData.CurrentInputActionAsset.FindAction(ActionName).bindings[i].path);
            } catch
            {
                NewActionButton = "";
                //Debug.Log("None");
            }
            ActionButtonNames.Add(NewActionButton);
        }

        CorrectSpriteSet = SpriteArrayToIconSet(LoadSpritesFromSpriteSheet(IndicatorsPath, ActionButtonNames.ToArray()));

        return CorrectSpriteSet;
    }

    private void TogglePause()
    {
        if (!SettingsManager.Instance.CanTogglePause) return;
        GamePaused = !GamePaused;

        PauseGame(GamePaused);
        DisplayPauseMenu(GamePaused);
    }

    private void PauseGame(bool pause)
    {
        if(pause) Time.timeScale = 0f;
        else Time.timeScale = 1f;
    }

    private void DisplayPauseMenu(bool pause)
    {
        PauseMenu.SetActive(pause);
    }

    public void ToggleMappingMenu(bool toggle)
    {
        if(!toggle)
        {
            RemappingPopUp.SetActive(false);
            return;
        }

        RemappingPopUp.SetActive(true);
    }
}
