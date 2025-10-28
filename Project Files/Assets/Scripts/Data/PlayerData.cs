using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackStat
{
    //public CustomAnimator.AnimationStatesEnum AnimationState;
    public int Damage;
    public Vector2 AttackHitboxRelativePosition;
    public float AttackHitboxSize;
    public Collider2D AttackHitbox;
    public Color DebugColor;
    // (1/Framerate) * AmountOfFrames
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "Data/PlayerStats")]
public class PlayerData : ScriptableObject
{
    [Header("Character Setup")]
    public LayerMask CollisionRaycastLayers;
    //public LayerMask IgnoreForWallslide;
    public float MaxDistanceForWallContact;
    public float WallCheckHeight;
    public float MaxWallDistanceForLedgeGrab;
    public float LedgeGrabFreeSpaceCheckHeight;
    public float LedgeGrabOccupiedSpaceCheckHeight;
    public float MinGroundAndCeilingDistance;
    public float SlopeCheckRaycastLength;
    public float VerticalChecksWidth;

    /////////////////////////////

    [Header("Base Movement")]
    public float HorizontalAcceleration;
    public float HorizontalDeceleration;
    public float ForcedDeceleration;
    public float HorizontalAirAcceleration;
    public float HorizontalAirDeceleration;

    [Header("Jumping")]
    public float JumpGravityMultiplier;
    public float JumpCutoffHeight;
    public float LowJumpHeight;
    public float HighJumpHeight;

    /////////////////////////////

    [Header("Dash")]
    public float DashVelocity;
    public float DashTime;
    public float DashCooldown;

    /// /////////////////////////

    [Header("Limiters")]
    public float MaxHorizontalVelocity;
    public float TerminalVelocity;
    public float MaxSlopeAngle;
    public float WallSlideEntryVelocity;

    /////////////////////////////

    [Header("Wall Movement")]
    public float MaxYVelocityToEnterWallSlide;
    public float WallSlideGravity;
    public float WallSlideTerminalVelocity;
    public Vector2 WallJumpVelocity;
    public Vector2 LedgeClimbVelocities;
    public Vector2 LedgeClimbTargetOffsets;

    /////////////////////////////

    [Header("Rope Movement")]
    public float RopeEnterAndExitTimeMargin;
    public float MinDistanceFromRopeAnchor;
    public float MaxRopeLength;
    public float MaxRopeAngle;
    public float MaxRopeAngleForAscentAndDescent;
    public float RopeAscentAndDescentSpeed;
    public float RopePassiveDeceleration;
    public float RopeActiveAcceleration;
    public float RopeBoostedAcceleration;
    public float RopePeriodCoefficient;
    public float RopeJumpMultiplier;

    /////////////////////////////

    [Header("Advanced Movement")]
    public float BaseVaultingVelocity;
    public float SlideColliderHeight;

    /////////////////////////////

    [Header("Attacks")]
    public int MaxCombo;
    public float AttackChainMargin;
    public AttackStat[] AttackStats;
    [Range(0, 1)]
    public float AttackMovementSpeedFactor;
    public Vector2 ShurikenSpawnOffset;
    public float ShurikenSpeed;
    [Range(0, 1)]
    public float ThrowingShurikenMovementFactor;
    public float ShurikenMaxDistanceFromPlayer;

    /////////////////////////////

    [Header("Timers")]
    public float CoyoteTime;
    public float LedgeClimbDelay;
    public float LedgeToWallSlideDelay;
    public float TimeInWallSlideToWallJump;
    public float WallJumpMovementDelay;

    /////////////////////////////

    //[Header("Debug")]
}
