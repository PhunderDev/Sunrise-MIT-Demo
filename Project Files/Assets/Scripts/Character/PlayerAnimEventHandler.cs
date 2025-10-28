using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    [SerializeField] PlayerController player;

    public void HandleHit()
    {
        player.HandleHit();
    }

    public void HandleAttackEnd()
    {
        player.HandleAttackEnd();
    }

    public void HandleSlideRecovery()
    {
        player.CanCancelDash = true;
    }
}
