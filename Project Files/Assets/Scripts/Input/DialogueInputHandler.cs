using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInputHandler : InputHandler
{
    public bool IsConversating;
    public bool CanInterrupt;

    public override InputState ProcessInput(InputState input)
    {

        if (IsConversating)
        {
            input.ShurikenInput = false;
            input.JumpInput = false;
            input.AttackInput = false;
            input.CounterInput = false;
            if (!CanInterrupt)
            {
                input.MovementVector = Vector3.zero;
            }
        }
        return input;
    }
}
