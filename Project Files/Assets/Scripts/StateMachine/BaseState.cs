using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    protected StateMachine StateMachine;
    protected float startTime;
    public float time => Time.time - startTime;
    public virtual void EnterState()
    {
        startTime = Time.time;
    }
    public virtual void ExitState() { }
    public virtual void LogicUpdate()
    {
        PlayerChecks();
    }
    public virtual void PhysicsUpdate() { }
    public virtual void PlayerChecks() { }

    public virtual void DrawGizmos() { }
}