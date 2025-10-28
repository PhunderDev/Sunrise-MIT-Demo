using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AnimationCFG", menuName = "Animator/AnimationConfig")]
public class AnimationConfig : ScriptableObject
{
    [Header("Is used as a BLEND animation")]
    public bool IsTransitionAnimation;

    //[Header("Possible ANIMATED Transitions (separate animation as blend)\nleave empty if IsTransitionAnimation is TRUE")]
    //public TransitionData[] PossibleAnimatedTransitions;
}
