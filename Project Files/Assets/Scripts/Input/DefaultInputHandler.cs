using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Threading;

public class GlobalInputData
{
    public DefaultInputHandler.InputDeviceType CurrentInputDeviceType = DefaultInputHandler.InputDeviceType.Keyboard;
    public InputActionAsset CurrentInputActionAsset;

}

[System.Serializable]
public class InputIndicatorIconSet
{
    public Sprite CurrentInputIndicatorSprite_Keyboard;
    public Sprite CurrentInputIndicatorSprite_Xbox;
    public Sprite CurrentInputIndicatorSprite_Playstation;
    public Sprite CurrentInputIndicatorSprite_Nintendo;

    public Sprite GetIconForCurrentDevice(DefaultInputHandler.InputDeviceType DeviceType)
    {
        Sprite CorrectSprite = null;

        switch(DeviceType)
        {
            case DefaultInputHandler.InputDeviceType.Keyboard:
                CorrectSprite = CurrentInputIndicatorSprite_Keyboard;
                break;
            case DefaultInputHandler.InputDeviceType.XboxController:
                CorrectSprite = CurrentInputIndicatorSprite_Xbox;
                break;
        }

        return CorrectSprite;
    }
}


[System.Serializable]
public class InputState
{
    public Vector2 MovementVector = Vector2.zero;
    public bool JumpInput = false;
    public bool HasReleasedJumpButton = false;
    public bool AttackInput = false;
    public bool ShurikenInput = false;
    public bool CounterInput = false;
    public bool InteractInput = false;
    public bool DashInput = false;
    public bool RopeInput = false;
}



public abstract class InputHandler : MonoBehaviour
{
    [Header("Assigned in OnStart, only set for debug purposes and after launching the game")]
    public InputIndicatorIconSet CurrentInputIndicatorSprites;

    public string ActionName = "";

    private void Start()
    {
        UpdateButtonIndicatorSprite();
    }

    public virtual InputState ProcessInput(InputState input)
    {
        return null;
    }

    public virtual void UpdateButtonIndicatorSprite()
    {
        if (ActionName == "") return;
        Debug.Log("Updating Action Mapping Information: " + ActionName);
        CurrentInputIndicatorSprites = UIHandler.Instance.GetRightInputIndicatorSprites(ActionName);
    }
}


public class DefaultInputHandler : InputHandler
{
    public static DefaultInputHandler Instance { get; private set; }

    public enum InputDeviceType
    {
        Keyboard = 0,
        XboxController = 1,
    }
    public static GlobalInputData InputData { get; private set; } = new GlobalInputData();
    public PlayerInput input;
    [SerializeField] PlayerData playerData;
    [SerializeField] float inputBufferTime;
    private float jumpInputTime;
    private float attackInputTime;
    private float shurikenInputTime;
    private float dashInputTime;
    [SerializeField]
    private TextMeshProUGUI DebugInputText;

    public InputState Inputs = new InputState();
    [SerializeField]
    private string ControllerInputDeviceControlSchemeName;

    public UnityEvent UpdatePrompt;

    private void Awake()
    {
        Instance = this;
        UpdateActionAsset();
    }

    private void Update()
    {
        BufferJump();
        BufferAttack();
        BufferShurikenThrow();
        BufferDash();
    }

    public void UpdateActionAsset()
    {
        InputData.CurrentInputActionAsset = input.actions;
        SettingsManager.Instance.LoadBindings(InputData.CurrentInputActionAsset);
    }

    public void OnControlsChanged()
    {
        if (input.currentControlScheme == null) return;
        UpdatePrompt.Invoke();

        Debug.Log("Changed Current Input Device to: " + input.currentControlScheme);
        DebugInputText.text = input.currentControlScheme;
        if (input.currentControlScheme == ControllerInputDeviceControlSchemeName)
        {
            InputData.CurrentInputDeviceType = InputDeviceType.XboxController;
            Debug.Log((int)InputData.CurrentInputDeviceType);
        }
        else InputData.CurrentInputDeviceType = InputDeviceType.Keyboard;
    }



    #region Actions
    public void OnMove(InputAction.CallbackContext context)
    {
        Inputs.MovementVector = context.ReadValue<Vector2>();
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Inputs.AttackInput = true;
            attackInputTime = Time.time;
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Inputs.JumpInput = true;
            Inputs.HasReleasedJumpButton = false;
            jumpInputTime = Time.time;
        }
        if (context.canceled)
        {
            Inputs.HasReleasedJumpButton = true;
        }
    }
    public void OnShurikenThrow(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Inputs.ShurikenInput = true;
            shurikenInputTime = Time.time;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Inputs.InteractInput = true;

        }
    }

    public void OnRopeThrow(InputAction.CallbackContext context)
    {
        Debug.Log("Rope Input");
        if (context.started)
        {
            Inputs.RopeInput = true;

        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {

        if (context.started)
        {
            Inputs.DashInput = true;
            dashInputTime = Time.time;
        }
    }

#if UNITY_EDITOR
    public void OnDebugSlowDown(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Time.timeScale = 0.1f;
        }
        if (context.canceled)
        {
            Time.timeScale = 1f;
        }
    }
#endif

#endregion

    
    #region Buffers
    private void BufferJump()
    {
        if (!Inputs.JumpInput) return;
        if (Time.time > jumpInputTime + inputBufferTime)
        {
            Inputs.JumpInput = false;
        }
    }
    private void BufferAttack()
    {
        if (!Inputs.AttackInput) return;
        if (Time.time > attackInputTime + inputBufferTime)
        {
            Inputs.AttackInput = false;
        }
    }
    private void BufferShurikenThrow()
    {
        if (!Inputs.ShurikenInput) return;
        if (Time.time > shurikenInputTime + inputBufferTime)
        {
            Inputs.ShurikenInput = false;
        }
    }

    private void BufferDash()
    {
        if (!Inputs.DashInput) return;
        if (Time.time > dashInputTime + inputBufferTime)
        {
            Inputs.DashInput = false;
        }
    }
    #endregion


    public override InputState ProcessInput(InputState input)
    {
        return Inputs;
    }
}
