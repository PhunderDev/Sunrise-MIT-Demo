using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputProcessor : MonoBehaviour
{
    [SerializeField] InputHandler[] InputHandlers;
    InputState InputState = new InputState();

    public InputState GetInputState()
    {

        foreach (InputHandler handler in InputHandlers)
        {
            InputState = handler.ProcessInput(InputState);
        }
        return InputState;
    }

    public void UseJumpInput()
    {
        InputState.JumpInput = false;
        DefaultInputHandler.Instance.Inputs.JumpInput = false;
    }
    public void UseAttackInput()
    {
        InputState.AttackInput = false;
        DefaultInputHandler.Instance.Inputs.AttackInput = false;
    }
    public void UseShurikenInput()
    {
        InputState.ShurikenInput = false;
        DefaultInputHandler.Instance.Inputs.ShurikenInput = false;
    }

    public void UseInteractInput()
    {
        InputState.InteractInput = false;
        DefaultInputHandler.Instance.Inputs.InteractInput = false;
    }

    public void UseRopeInput()
    {
        Debug.Log("USE ROPE INPUT");
        InputState.RopeInput = false;
        DefaultInputHandler.Instance.Inputs.RopeInput = false;
    }

    public void UseDashInput()
    {
        Debug.Log("Use Dash input");
        InputState.DashInput = false;
        DefaultInputHandler.Instance.Inputs.DashInput = false;
    }
}
