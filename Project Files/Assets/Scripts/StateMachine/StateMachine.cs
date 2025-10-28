using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public BaseState CurrentState { get; private set; }
    public BaseState PreviousState { get; private set; }
    public void Initialize(BaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.EnterState();
        Debug.Log(startingState);
    }
    public void ChangeState(BaseState newState)
    {
        Debug.Log(CurrentState + " -> " + newState + " after " + CurrentState.time.ToString("0.00") + " s");
        CurrentState.ExitState();
        PreviousState = CurrentState;
        CurrentState = newState;
        CurrentState.EnterState();
    }
}
