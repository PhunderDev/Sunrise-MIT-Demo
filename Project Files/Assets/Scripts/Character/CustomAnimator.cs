using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransitionArrayElement
{
    public string ToState;
    public string AnimationName;
}
[System.Serializable]
public class AnimationState
{
    public string StateName = "Default";
    public string[] AnimationsArray;
    public TransitionArrayElement[] Transitions;
}

[RequireComponent(typeof(Animator))]
public class CustomAnimator : MonoBehaviour
{
    [HideInInspector]
    public AnimationState CurrentState = new AnimationState();
    [Tooltip("Set to a name of one of the states to handle exceptions")]
    public string DefaultState = "Idle";
    [Tooltip("Shows the name of the current state")]
    public string CurrentStateName;
    [SerializeField]
    private AnimationState[] AnimationStates;
    private string QueueAnimation = "";
    private int QueueStartFromFrame;
    private float QueueSpeed;
    public float animationTime { get; private set; }
    public float animationDirection { get; private set; }
    public Animator animator {get; private set;}
    public bool IsDuringTransitionAnimation { get; private set; } = false;
    private AnimationState NextState;
    private int NextDirection;
    //Used inside the Custom Editor
    [HideInInspector]
    public List<string> stateNames = new List<string> { "" };

    private void Awake()
    {
        CurrentStateName = DefaultState;
        animator = GetComponent<Animator>();
        PlayAnimation(CurrentStateName);
    }

    private void Update()
    {
        animationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (IsDuringTransitionAnimation)
        {
            if(animationTime >= 1f)
            {
                PlayAnimation(NextState.StateName, NextDirection);
                IsDuringTransitionAnimation = false;
            }
        }
    }


    public void PlayAnimation(string StateName, int AnimationDirection = 0, int AnimationIndex = 0, int StartFromFrame = 1, int AnimationSpeed = 0)
    {
        AnimationState PreviousState = CurrentState;
        if (StateName != PreviousState.StateName)
        {
            IsDuringTransitionAnimation = false;
            FindStateFromName(StateName);
        }
        string AnimationToPlay = CurrentState.AnimationsArray[AnimationIndex];
        if (PreviousState.Transitions.Length != 0)
        {
            foreach (var transition in PreviousState.Transitions)
            {
                if (transition.ToState == CurrentState.StateName)
                {
                    AnimationToPlay = transition.AnimationName;
                    NextState = CurrentState;
                    NextDirection = AnimationDirection;
                    IsDuringTransitionAnimation = true;
                }
            }
        }
        else
        {
            IsDuringTransitionAnimation = false;
        }

        if (AnimationDirection == 1)
        {
            AnimationToPlay = AnimationToPlay + "_right";
        }
        else if (AnimationDirection == -1)
        {
            AnimationToPlay = AnimationToPlay + "_left";
        }
        animationDirection = AnimationDirection;
        animator.speed = 0f;
        animator.Play(AnimationToPlay, 0);
        QueueAnimation = AnimationToPlay;
        QueueStartFromFrame = StartFromFrame;
        QueueSpeed = AnimationSpeed;
        Debug.Log(AnimationToPlay);
    }
    private void LateUpdate()
    {
        if (QueueAnimation != "")
        {
            float Framerate = animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;
            float ClipLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length * Framerate;
            animator.Play(QueueAnimation, 0, (QueueStartFromFrame - 1) / ClipLength);
            animationTime = 0f;
            if (QueueSpeed != 0)
            {
                animator.speed = QueueSpeed / Framerate;
            }
            else
            {
                animator.speed = 1;
            }
            QueueAnimation = "";
        }
    }
    public void FindStateFromName(string Name)
    {
        foreach (var state in AnimationStates)
        {
            if (state.StateName == Name)
            {
                CurrentState = state;
                CurrentStateName = state.StateName;
                return;
            }
        }
        CurrentStateName = DefaultState;
        Debug.LogError("Cannot find Animation State with name: " + Name + ", check if the state is in the Inspector");
    }
    public void PauseAnimation()
    {
        animator.speed = 0;
    }
}