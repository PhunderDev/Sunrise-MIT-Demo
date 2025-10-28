using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    #region Components
    [Header("Components")]
    public Vector3 GroundCheckOffset;
    public Vector3 CeilingCheckOffset, LeftWallCheckOffset, RightWallCheckOffset, LeftLedgeCheckOffset, RightLedgeCheckOffset;
    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public BoxCollider2D col;
    public InputProcessor InputProcessor { get; private set; }
    public InputState InputState { get; private set; }
    public StateMachine StateMachine { get; private set; }
    public CustomAnimator PlayerAnimator;
    public PlayerData PlayerData;
    [SerializeField] TMP_Text StateText;
    [SerializeField] GameObject shurikenPrefab;
    #endregion

    [Header("Basic Movement")]
    public float CurrentGravityMultiplier = 1f;
    public Vector2 CurrentVelocity;
    public float RelativeVelocity;
    private float VerticalVelocity;
    public float lowJumpVelocity;
    public float highJumpVelocity;
    public bool IsForciblyDeccelerating;
    public bool IsTouchingCeiling { get; private set; } = false;
    public bool IsGrounded { get; private set; } = false;

    #region Slopes
    [Header("Slopes")]
    private Vector3 LeftSlopeCheckOffset;
    private Vector3 RightSlopeCheckOffset;
    private Vector2 SlopeParallel;
    public bool IsOnSlope { get; private set; } = false;
    public float FloorAngle { get; private set; } = 0f;
    #endregion

    #region Ledges
    [Header("Ledges")]
    /*public bool IsLeftLedgeFree { get; private set; } = false;
    public bool IsRightLedgeFree { get; private set; } = false;*/
    private bool RightLedgeOccupiedOverlap;
    private bool RightLedgeFreeOverlap;
    private bool LeftLedgeOccupiedOverlap;
    private bool LeftLedgeFreeOverlap;
    public int CurrentWall { get; private set; } = 0;
    public bool CanLedgeGrabLeft { get; private set; } = false;
    public bool CanLedgeGrabRight { get; private set; } = false;

    /*public bool IsLeftLedgeCollision { get; private set; }
    public bool IsRightLedgeCollision { get; private set; }*/
    #endregion

    #region Ropes
    [Header("Ropes")]
    public bool IsHooked = false;
    public bool IsSwinging = false;
    public Vector3 PlayerRopeEntryPos;
    public Vector3 PlayerRopeExitPos;
    public float CurrentRopeLength = 0f;
    public float CurrentAngle = 0f;
    public float CurrentTargetRopeAngle = 0f;
    public float LastTargetRopeAngle = 0f;
    public float CurrentRopePeriod = 0f;
    public float CurrentRopeSwingTimeElapsed = 0f;
    public bool HasChangedHalf = false;
    public int CurrentRopeSwingDirection = 1; // -1 = Left | 1 = Right
    #endregion

    #region One Way Platforms
    [Header("One Way Platforms")]
    public bool DroppingDown;
    public bool CanPressDownInput;
    public bool IsGoingThroughPlatform;
    public bool CanDropDownWhenIdle;
    private LayerMask DefaultMask;
    private LayerMask OneWayMask;
    private LayerMask RaycastMask;
    #endregion

    #region Player States
    [Header("Player States")]
    public PlayerIdle Idle;
    public PlayerMove Move;
    public PlayerMoveDecelerate MoveDecelerate;
    public PlayerInAir InAir;
    public PlayerJump Jump;
    public PlayerLand Land;
    public PlayerDash Dash;
    public PlayerRopeThrow RopeThrow;
    public PlayerRopeSwing RopeSwing;
    public PlayerWallJump WallJump;
    public PlayerWallSlide WallSlide;
    public PlayerLedgeClimb LedgeClimb;
    public PlayerLedgeGrab LedgeGrab;
    public PlayerAscend Ascend;
    public PlayerAttack Attack;
    public PlayerVault Vault;
    public PlayerShurikenThrow ShurikenThrow;
    #endregion

    [Header("Other")]
    public VaultableObject VaultableObject;
    public Interactable InteractableObject;
    public RopeAnchor RopeAnchorObject;

    public int FacingDirection { get; private set; } = 1;
    public int VelocityDirection { get; private set; } = 1;
    public float CurrentCoyoteTime { get; private set; } = 0f;
    public bool CountingCoyote { get; private set; } = false;
    public bool IsAttacking { get; set; } = false;
    public bool DirectionChanged { get; set;} = false;
    public bool VelocityDirectionChanged { get; set; } = false;
    public bool CanDash = true;
    public bool CanCancelDash = false;
    public bool CanStandUpAfterDash = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        CalculateOffsets();

        DefaultMask = PlayerData.CollisionRaycastLayers;
        OneWayMask = PlayerData.CollisionRaycastLayers;
        OneWayMask |= (1 << LayerMask.NameToLayer("OneWayPlatform"));

        lowJumpVelocity = Mathf.Sqrt(2 * (PlayerData.LowJumpHeight - PlayerData.JumpCutoffHeight) * -Physics2D.gravity.y * PlayerData.JumpGravityMultiplier);
        highJumpVelocity = Mathf.Sqrt(2 * PlayerData.HighJumpHeight * -Physics2D.gravity.y * PlayerData.JumpGravityMultiplier);
        
        InputProcessor = GetComponent<InputProcessor>();
        InputState = new InputState();
        #region StateMachine setup
        StateMachine = new StateMachine();

        Idle = new PlayerIdle(this, StateMachine, PlayerData, PlayerAnimator, "" /* "Idle" */);
        Move = new PlayerMove(this, StateMachine, PlayerData, PlayerAnimator, "" /* "Run" */);
        MoveDecelerate = new PlayerMoveDecelerate(this, StateMachine, PlayerData, PlayerAnimator, "Change Direction");


        Jump = new PlayerJump(this, StateMachine, PlayerData, PlayerAnimator, "");
        Ascend = new PlayerAscend(this, StateMachine, PlayerData, PlayerAnimator, "Jump");
        InAir = new PlayerInAir(this, StateMachine, PlayerData, PlayerAnimator, "Fall");
        Land = new PlayerLand(this, StateMachine, PlayerData, PlayerAnimator, "");

        Dash = new PlayerDash(this, StateMachine, PlayerData, PlayerAnimator, "Dash");

        RopeThrow = new PlayerRopeThrow(this, StateMachine, PlayerData, PlayerAnimator, "Rope Swing");
        RopeSwing = new PlayerRopeSwing(this, StateMachine, PlayerData, PlayerAnimator, "Rope Swing");


        WallJump = new PlayerWallJump(this, StateMachine, PlayerData, PlayerAnimator, "Wall Jump");
        WallSlide = new PlayerWallSlide(this, StateMachine, PlayerData, PlayerAnimator, "Wall Slide");
        LedgeGrab = new PlayerLedgeGrab(this, StateMachine, PlayerData, PlayerAnimator, "Ledge Grab");
        LedgeClimb = new PlayerLedgeClimb(this, StateMachine, PlayerData, PlayerAnimator, "Idle");

        Attack = new PlayerAttack(this, StateMachine, PlayerData, PlayerAnimator, "Attack1");
        Vault = new PlayerVault(this, StateMachine, PlayerData, PlayerAnimator, "Run");
        ShurikenThrow = new PlayerShurikenThrow(this, StateMachine, PlayerData, PlayerAnimator, "Idle");

        StateMachine.Initialize(Idle);
        #endregion
    }

    private void Update()
    {
        InputState = InputProcessor.GetInputState();
        UpdateDirection();
        UpdateVelocityDirection();
        //PlayerAnimator.SetAnimParameters(FacingDirection, CurrentVelocity, InputState);

        CheckCollisions();
        StateText.text = StateMachine.CurrentState.ToString();
        StateMachine.CurrentState.LogicUpdate();
        if (CountingCoyote)
        {
            CurrentCoyoteTime += Time.deltaTime;
            if(CurrentCoyoteTime >= PlayerData.CoyoteTime)
            {
                CountingCoyote = false;
            }
        }
    }

    private void FixedUpdate()
    {
        HandleGravity(CurrentGravityMultiplier);
        StateMachine.CurrentState.PhysicsUpdate();
        ExecuteMovement();
    }

    public void CalculateOffsets()
    {
        GroundCheckOffset = new Vector3(0, -col.size.y / 2 + col.offset.y, 0);
        CeilingCheckOffset = new Vector3(0, col.size.y / 2 + col.offset.y, 0);
        LeftWallCheckOffset = new Vector3(-col.size.x / 2, -0.875f, 0); // temp values
        RightWallCheckOffset = new Vector3(col.size.x / 2, -0.875f, 0);
        LeftLedgeCheckOffset = new Vector3(-col.size.x / 2, -0.8f, 0);
        RightLedgeCheckOffset = new Vector3(col.size.x / 2, -0.8f, 0);
        LeftSlopeCheckOffset = new Vector3(-col.size.x / 2, -col.size.y / 2 + col.offset.y, 0);
        RightSlopeCheckOffset = new Vector3(col.size.x / 2, -col.size.y / 2 + col.offset.y, 0);
    }

    public IEnumerator CountDashCooldown()
    {
        yield return new WaitForSeconds(PlayerData.DashCooldown);
        CanDash = true;
    }
    
    public void NewHandleGroundMovement(float xAxis, float speedCoefficient = 1)
    {
        float NewXVelocity;
        float AccelerationOrDecceleration = PlayerData.HorizontalAcceleration;
        if (xAxis == 0)
        {
            AccelerationOrDecceleration = PlayerData.HorizontalDeceleration;
            NewXVelocity = Mathf.Clamp(Mathf.Abs(RelativeVelocity) - (AccelerationOrDecceleration * Time.fixedDeltaTime), 0, Mathf.Abs(RelativeVelocity)) * Mathf.Sign(RelativeVelocity);
        }
        else
        {
            NewXVelocity = Mathf.Clamp(RelativeVelocity + (AccelerationOrDecceleration * xAxis * Time.fixedDeltaTime), -PlayerData.MaxHorizontalVelocity, PlayerData.MaxHorizontalVelocity);
        }
        RelativeVelocity = NewXVelocity;

        CurrentVelocity.x = RelativeVelocity * SlopeParallel.x * speedCoefficient;
        CurrentVelocity.y = RelativeVelocity * SlopeParallel.y * speedCoefficient + VerticalVelocity;

        if ((NewXVelocity < 0f && CurrentWall == -1) || (NewXVelocity > 0f && CurrentWall == 1))
        {
            RelativeVelocity = 0f;
            CurrentVelocity.x = 0f;
        }
    }
    public void HandleAirMovement(float xAxis)
    {
        float NewXVelocity;
        float AccelerationOrDeceleration = PlayerData.HorizontalAirAcceleration;
        if (xAxis == 0)
        {
            AccelerationOrDeceleration = PlayerData.HorizontalAirDeceleration;
            NewXVelocity = Mathf.Clamp(Mathf.Abs(CurrentVelocity.x) - (AccelerationOrDeceleration * Time.fixedDeltaTime), 0, Mathf.Abs(CurrentVelocity.x)) * Mathf.Sign(CurrentVelocity.x);
        }
        else
        {
            NewXVelocity = Mathf.Clamp(CurrentVelocity.x + (AccelerationOrDeceleration * xAxis * Time.fixedDeltaTime), -PlayerData.MaxHorizontalVelocity, PlayerData.MaxHorizontalVelocity);
        }
        CurrentVelocity.x = NewXVelocity;
        CurrentVelocity.y = VerticalVelocity;
        RelativeVelocity = NewXVelocity;
    }
    public void ForceDecceleration()
    {
        IsForciblyDeccelerating = true;
        float Decceleration = PlayerData.ForcedDeceleration;
        float NewXVelocity;
        NewXVelocity = Mathf.Clamp(Mathf.Abs(RelativeVelocity) - (Decceleration * Time.deltaTime), 0, Mathf.Abs(RelativeVelocity)) * Mathf.Sign(RelativeVelocity);
        RelativeVelocity = NewXVelocity;
        CurrentVelocity.x = RelativeVelocity * SlopeParallel.x;
        CurrentVelocity.y = RelativeVelocity * SlopeParallel.y + VerticalVelocity;
        if (CurrentVelocity.x == 0)
        {
            IsForciblyDeccelerating = false;
        }
    }

    #region Collisions
    public void CheckCollisions()
    {
        CheckPlatforms();
        CheckGround();
        CheckCeiling();
        CheckWalls();
        CheckSlopes();
        CheckLedges();
    }

    private void CheckPlatforms()
    {
        if (!CanPressDownInput && !IsGoingThroughPlatform && !DroppingDown && InputState.MovementVector.y >= 0f)
        {
            CanPressDownInput = true;
        }
        if (DroppingDown || IsGoingThroughPlatform) Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("OneWayPlatform"), true);
        else Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("OneWayPlatform"), false);

        RaycastHit2D hit1 = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + (col.size.y / 2)), Vector2.down, col.size.y, OneWayMask);
        if (hit1 && hit1.collider.gameObject.layer == LayerMask.NameToLayer("OneWayPlatform") || DroppingDown) IsGoingThroughPlatform = true;
        else IsGoingThroughPlatform = false;

        if (CurrentVelocity.y > 0.01f)
        {
            RaycastMask = DefaultMask;
        }
        else
        {
            if (IsGoingThroughPlatform || DroppingDown) RaycastMask = DefaultMask;
            else RaycastMask = OneWayMask;
        }
    }
    private void CheckGround()
    {
        bool OldIsGrounded = IsGrounded;
        IsGrounded = Physics2D.OverlapBox(transform.position + GroundCheckOffset, new Vector2(PlayerData.VerticalChecksWidth, PlayerData.MinGroundAndCeilingDistance), 0f, RaycastMask);
        Collider2D[] Collisions = Physics2D.OverlapBoxAll(transform.position + GroundCheckOffset, new Vector2(PlayerData.VerticalChecksWidth, PlayerData.MinGroundAndCeilingDistance), 0f, OneWayMask);
        for (int i = 0; i < Collisions.Length; i++)
        {
            if (Collisions[i].gameObject.layer != LayerMask.NameToLayer("OneWayPlatform"))
            {
                CanDropDownWhenIdle = false; break;
            }
            else CanDropDownWhenIdle = true;
        }
        if (!IsGrounded && OldIsGrounded)
        {
            CountingCoyote = true;
            CurrentCoyoteTime = 0f;
        }
    }
    private void CheckCeiling()
    {
        IsTouchingCeiling = Physics2D.OverlapBox(transform.position + CeilingCheckOffset, new Vector2(col.size.x, PlayerData.MinGroundAndCeilingDistance), 0f, RaycastMask);
        if (IsTouchingCeiling && CurrentVelocity.y > 0)
        {
            int LeftCorner = 0;
            for (int i = 0; i <= 6; i++)
            {
                if (Physics2D.Raycast(transform.position + CeilingCheckOffset + Vector3.left * 0.5f * col.size.x + Vector3.right * i * 0.0625f, Vector3.up, 0.125f, OneWayMask))
                {
                    LeftCorner = i + 1;
                }
            }
            int RightCorner = 0;
            for (int j = 0; j <= 6; j++)
            {
                if (Physics2D.Raycast(transform.position + CeilingCheckOffset + Vector3.right * 0.5f * col.size.x + Vector3.left * j * 0.0625f, Vector3.up, 0.125f, OneWayMask))
                {
                    RightCorner = j + 1;
                }
            }
            if (LeftCorner == 0 && RightCorner < 6)
            {
                transform.position = transform.position + Vector3.left * (RightCorner) * 0.0625f;
            }
            else if (RightCorner == 0 &&  LeftCorner < 6)
            {
                transform.position = transform.position + Vector3.right * (LeftCorner) * 0.0625f;
            }
            else
            {
                VerticalVelocity = Mathf.Min(0f, VerticalVelocity);
            }
        }
    }
    private void CheckWalls()
    {
        if (Physics2D.OverlapBox(transform.position + LeftWallCheckOffset, new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.WallCheckHeight), 0f, RaycastMask)) CurrentWall = -1;
        else if (Physics2D.OverlapBox(transform.position + RightWallCheckOffset, new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.WallCheckHeight), 0f, RaycastMask)) CurrentWall = 1;
        else CurrentWall = 0;
    }
    private void CheckSlopes()
    {
        RaycastHit2D verLeft = Physics2D.Raycast(transform.position + LeftSlopeCheckOffset, Vector2.down, PlayerData.SlopeCheckRaycastLength, RaycastMask);
        RaycastHit2D verRight = Physics2D.Raycast(transform.position + RightSlopeCheckOffset, Vector2.down, PlayerData.SlopeCheckRaycastLength, RaycastMask);
        RaycastHit2D horLeft = Physics2D.Raycast(transform.position + LeftSlopeCheckOffset, Vector2.left, PlayerData.MaxHorizontalVelocity * Time.deltaTime / Time.timeScale, RaycastMask);
        RaycastHit2D horRight = Physics2D.Raycast(transform.position + RightSlopeCheckOffset, Vector2.right, PlayerData.MaxHorizontalVelocity * Time.deltaTime / Time.timeScale, RaycastMask);

        if (RelativeVelocity > 0)
        {
            if (horRight)
            {
                IsOnSlope = true;
                SlopeParallel = -Vector2.Perpendicular(horRight.normal).normalized;
            }
            else if (verLeft)
            {
                IsOnSlope = true;
                if (verRight && verRight.distance < verLeft.distance) SlopeParallel = -Vector2.Perpendicular(verRight.normal).normalized;
                else
                {
                    SlopeParallel = -Vector2.Perpendicular(verLeft.normal).normalized;
                }
            }
            else
            {
                if (verRight) SlopeParallel = -Vector2.Perpendicular(verRight.normal).normalized;
                else
                {
                    IsOnSlope = false;
                    SlopeParallel = Vector2.right;
                }  
            }
        }
        else if (RelativeVelocity < 0)
        {
            if(horLeft)
            {
                IsOnSlope = true;
                SlopeParallel = -Vector2.Perpendicular(horLeft.normal).normalized;
            }
            else if (verRight)
            {
                IsOnSlope = true;
                if (verLeft && verLeft.distance < verRight.distance) SlopeParallel = -Vector2.Perpendicular(verLeft.normal).normalized;
                else
                {
                    SlopeParallel = -Vector2.Perpendicular(verRight.normal).normalized;
                }
            }
            else
            {
                if (verLeft) SlopeParallel = -Vector2.Perpendicular(verLeft.normal).normalized;
                else
                {
                    IsOnSlope = false;
                    SlopeParallel = Vector2.right;
                }
            }
        }
        else
        {
            if (verLeft) SlopeParallel = -Vector2.Perpendicular(verLeft.normal).normalized;
            else if (verRight) SlopeParallel = -Vector2.Perpendicular(verRight.normal).normalized;
        }
        FloorAngle = Mathf.Round(Vector2.SignedAngle(Vector2.right, SlopeParallel));
        if (Mathf.Abs(FloorAngle) > PlayerData.MaxSlopeAngle) SlopeParallel = Vector2.right;

        //Correct corners
        if (CurrentVelocity.y > 0 && !IsOnSlope)
        {
            if (horLeft && CurrentVelocity.x < 0)
            {
                for (int i = 1; i < 5; i++)
                {
                    if (!Physics2D.Raycast(transform.position + LeftSlopeCheckOffset + (Vector3.up * i * 0.0625f), Vector2.left, 0.0625f, OneWayMask))
                    {
                        transform.position = transform.position + (Vector3.up * i * 0.0625f);
                        break;
                    }
                }
            }
            else if (horRight && CurrentVelocity.x > 0)
            {
                for (int i = 1; i < 5; i++)
                {
                    if (!Physics2D.Raycast(transform.position + RightSlopeCheckOffset + (Vector3.up * i * 0.0625f), Vector2.right, 0.0625f, OneWayMask))
                    {
                        transform.position = transform.position + (Vector3.up * i * 0.0625f);
                        break;
                    }
                }
            }
        }
    }
    private void CheckLedges()
    {
        float FreeRightLedgeCheckPosition = transform.position.y + RightLedgeCheckOffset.y + (PlayerData.LedgeGrabFreeSpaceCheckHeight / 2f) + (PlayerData.LedgeGrabOccupiedSpaceCheckHeight / 2f);
        float FreeLeftLedgeCheckPosition = transform.position.y + LeftLedgeCheckOffset.y + (PlayerData.LedgeGrabFreeSpaceCheckHeight / 2f) + (PlayerData.LedgeGrabOccupiedSpaceCheckHeight / 2f);

        RightLedgeOccupiedOverlap = Physics2D.OverlapBox(transform.position + RightLedgeCheckOffset, new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.LedgeGrabOccupiedSpaceCheckHeight), 0, RaycastMask);
        RightLedgeFreeOverlap = Physics2D.OverlapBox(new Vector2(transform.position.x + RightLedgeCheckOffset.x, FreeRightLedgeCheckPosition), new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.LedgeGrabFreeSpaceCheckHeight), 0, RaycastMask);
        LeftLedgeOccupiedOverlap = Physics2D.OverlapBox(transform.position + LeftLedgeCheckOffset, new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.LedgeGrabOccupiedSpaceCheckHeight), 0, RaycastMask);
        LeftLedgeFreeOverlap = Physics2D.OverlapBox(new Vector2(transform.position.x + LeftLedgeCheckOffset.x, FreeLeftLedgeCheckPosition), new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.LedgeGrabFreeSpaceCheckHeight), 0, RaycastMask);

        CanLedgeGrabRight = (RightLedgeOccupiedOverlap && !RightLedgeFreeOverlap);
        CanLedgeGrabLeft = (LeftLedgeOccupiedOverlap && !LeftLedgeFreeOverlap);

        /*IsLeftLedgeFree = !Physics2D.OverlapBox(LeftLedgeCheckTransform.position, new Vector2(PlayerData.MaxWallDistanceForLedgeGrab, PlayerData.LedgeCheckHeight), 0f, ~PlayerData.PlayerLayer);
        IsRightLedgeFree = !Physics2D.OverlapBox(RightLedgeCheckTransform.position, new Vector2(PlayerData.MaxWallDistanceForLedgeGrab, PlayerData.LedgeCheckHeight), 0f, ~PlayerData.PlayerLayer);
        IsLeftLedgeCollision = Physics2D.OverlapBox(LeftLedgeCheckTransform.position - new Vector3(0f, PlayerData.LedgeWallCollisionCheckYOffset, 0f), new Vector2(PlayerData.MaxWallDistanceForLedgeGrab, PlayerData.LedgeCheckHeight * PlayerData.LedgeWallCollisionCheckSizeFactor), 0f, ~PlayerData.PlayerLayer);
        IsRightLedgeCollision = Physics2D.OverlapBox(RightLedgeCheckTransform.position - new Vector3(0f, PlayerData.LedgeWallCollisionCheckYOffset, 0f), new Vector2(PlayerData.MaxWallDistanceForLedgeGrab, PlayerData.LedgeCheckHeight * PlayerData.LedgeWallCollisionCheckSizeFactor), 0f, ~PlayerData.PlayerLayer);*/
    }

    public void CheckCeilingWhileDash(float PlayerHitboxHeight)
    {
        if (CurrentVelocity.x > 0)
        {
            CanStandUpAfterDash = !Physics2D.Raycast(transform.position + CeilingCheckOffset + Vector3.right * 0.5f * col.size.x, Vector3.up, PlayerHitboxHeight - col.size.y, OneWayMask);
        }
        if (CurrentVelocity.x < 0)
        {
            CanStandUpAfterDash = !Physics2D.Raycast(transform.position + CeilingCheckOffset + Vector3.left * 0.5f * col.size.x, Vector3.up, PlayerHitboxHeight - col.size.y, OneWayMask);
        }
    }
    #endregion

    #region Executes
    public void ExecuteJump()
    {
        if (IsGrounded || (CurrentCoyoteTime < PlayerData.CoyoteTime && CountingCoyote))
        {
            VerticalVelocity = highJumpVelocity;
            CountingCoyote = false;
        }
    }

    public void CutJump()
    {
        VerticalVelocity = lowJumpVelocity;
    }

    public void ExecuteWallJump()
    {
        CurrentVelocity.x = CurrentWall == -1 ? PlayerData.WallJumpVelocity.x : -PlayerData.WallJumpVelocity.x;
        VerticalVelocity = PlayerData.WallJumpVelocity.y;
        CurrentVelocity.y = VerticalVelocity;
        FacingDirection = -CurrentWall;
    }

    public void ExecuteRopeJump(Vector2 NewVelocity)
    {
        CurrentVelocity.x = NewVelocity.x;
        VerticalVelocity = NewVelocity.y;
        CurrentVelocity.y = VerticalVelocity;
    }

    public void ExecuteDash()
    {
        CanDash = false;
        RelativeVelocity = PlayerData.DashVelocity * FacingDirection;
    }

    public IEnumerator DropDown()
    {
        RaycastMask = DefaultMask;
        CanPressDownInput = false;
        DroppingDown = true;
        IsGoingThroughPlatform = true;
        yield return new WaitForSeconds(0.25f);
        DroppingDown = false;
        while (IsGoingThroughPlatform)
        {
            yield return null;
        }
        CanPressDownInput = true;

    }
    private void ExecuteMovement()
    {
        rb.velocity = CurrentVelocity;
        CameraController.Instance.SetSpeedCoefficient(RelativeVelocity, PlayerData.MaxHorizontalVelocity);
    }
    
    #endregion
    public void HandleGravity(float GravityMultiplier = 1f)
    {
        float Acceleration = -Physics2D.gravity.y * GravityMultiplier;
        float MinVelocity = -PlayerData.TerminalVelocity;
        if (IsGrounded)
        {
            MinVelocity = Mathf.Clamp(VerticalVelocity, 0f, VerticalVelocity);
        }

        if (StateMachine.CurrentState == WallSlide)
        {
            if (VerticalVelocity < 0f)
            {
                Acceleration = PlayerData.WallSlideGravity;
                MinVelocity = -PlayerData.WallSlideTerminalVelocity;
            }
        }
        VerticalVelocity = Mathf.Clamp(VerticalVelocity - (Acceleration * Time.fixedDeltaTime), MinVelocity, VerticalVelocity);
        if (!IsOnSlope)
        {
            CurrentVelocity.y = VerticalVelocity;
        }
    }

    public void HandleWallSlideEnter()
    {
        VerticalVelocity = Mathf.Max(-PlayerData.WallSlideEntryVelocity, VerticalVelocity);
        CurrentVelocity.y = VerticalVelocity;
        CurrentVelocity.x = 0f;
        RelativeVelocity = 0f;
        IsOnSlope = false;
        SlopeParallel = Vector2.zero;
        FloorAngle = 0f;
        FacingDirection = CurrentWall;
    }

    public void HandleLedgeGrabMovement()
    {
        VerticalVelocity = 0f;
        CurrentVelocity.y = 0f;
        CurrentVelocity.x = 0f;
    }

    #region Ropes
    public void HandleRopeMovementPhysics()
    {
        if(IsHooked && IsSwinging)
        {
            VerticalVelocity = 0f;
            CurrentVelocity.y = 0f;
            CurrentVelocity.x = 0f;
        }
    }

    public void HandleRopeMovementLogic()
    {
        //Start Swingning once the rope is tensioned enough

        if (RopeAnchorObject == null)
        {
            IsHooked = false;
            IsSwinging = false;
            return;
        }

        if (!IsSwinging && IsHooked) {
            float Distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(RopeAnchorObject.transform.position.x, RopeAnchorObject.transform.position.y));
            if (Distance >= CurrentRopeLength && PlayerRopeEntryPos != transform.position)
            {
                IsSwinging = true;
                CurrentAngle = FindRopeAngle();
                CurrentTargetRopeAngle = Mathf.Abs(CurrentAngle);
                LastTargetRopeAngle = CurrentTargetRopeAngle;
                UpdateRopePeriod();
                CurrentRopeSwingTimeElapsed = CurrentRopePeriod;

                CurrentRopeSwingDirection = (int)Mathf.Sign(transform.position.x - RopeAnchorObject.transform.position.x);


                RopeAnchorObject.SetAngle(CurrentAngle);
                transform.parent = RopeAnchorObject.RotatingHook;
                transform.localPosition = new Vector2(0f, transform.localPosition.y);
                transform.localEulerAngles = new Vector3(0f, 0f, -180f);
            }
            else return;
        }

        CurrentRopeLength = Mathf.Clamp(CurrentRopeLength, PlayerData.MinDistanceFromRopeAnchor, RopeAnchorObject.Range);



        //Set Current Angle
        float Progress = CurrentRopeSwingTimeElapsed / CurrentRopePeriod;
        CurrentAngle = Mathf.Lerp(-LastTargetRopeAngle * CurrentRopeSwingDirection, LastTargetRopeAngle * CurrentRopeSwingDirection, RopeSwingEaseInOutFunction(Progress));
        RopeAnchorObject.SetAngle(CurrentAngle);


        // Decelerate Passively if the Player does not keep on actively swinging
        if (InputState.MovementVector.x == 0)
        {
            if (!HasChangedHalf) CurrentTargetRopeAngle = CurrentTargetRopeAngle - (PlayerData.RopePassiveDeceleration * Time.deltaTime);
        }
        // Accelerate/Deccelerate depending on the Movement Vector
        else if(!HasChangedHalf)
        {
            if (InputState.MovementVector.x == CurrentRopeSwingDirection)
            {
                if (CurrentTargetRopeAngle <= PlayerData.MaxRopeAngleForAscentAndDescent)
                {
                    CurrentTargetRopeAngle = Mathf.Clamp(CurrentTargetRopeAngle + (PlayerData.RopeBoostedAcceleration * Time.deltaTime), 0, PlayerData.MaxRopeAngle);
                    LastTargetRopeAngle = CurrentTargetRopeAngle;
                }
                else CurrentTargetRopeAngle = CurrentTargetRopeAngle + (PlayerData.RopeActiveAcceleration * Time.deltaTime);
            }
            else
            {
                CurrentTargetRopeAngle = CurrentTargetRopeAngle - (PlayerData.RopeActiveAcceleration + PlayerData.RopePassiveDeceleration) * Time.deltaTime;
            }
        }

        CurrentTargetRopeAngle = Mathf.Clamp(CurrentTargetRopeAngle, 0f, PlayerData.MaxRopeAngle);

        if (CurrentRopeSwingDirection == Mathf.Sign(CurrentAngle) && !HasChangedHalf)
        {
            LastTargetRopeAngle = CurrentTargetRopeAngle;
            HasChangedHalf = true;            
        }

        // Adjust Swing Distance Dependin
        if (InputState.MovementVector.y != 0 && CurrentTargetRopeAngle <= PlayerData.MaxRopeAngleForAscentAndDescent)
        {
            CurrentRopeLength = Mathf.Clamp(CurrentRopeLength - (PlayerData.RopeAscentAndDescentSpeed * InputState.MovementVector.y * Time.deltaTime), PlayerData.MinDistanceFromRopeAnchor, PlayerData.MaxRopeLength);
            UpdateRopePeriod();
        }

        // Update Location Relative to the Hook
        transform.localPosition = new Vector3(transform.localPosition.x, CurrentRopeLength, transform.localPosition.z);

        // Update Time Elapsed Since Changing The Direction
        CurrentRopeSwingTimeElapsed += Time.deltaTime;

        if(Mathf.Sign(InputState.MovementVector.x) == CurrentRopeSwingDirection && InputState.MovementVector.x != 0f) CameraController.Instance.SetSpeedCoefficient(CurrentRopeSwingTimeElapsed * CurrentRopeSwingDirection, CurrentRopePeriod);
        else CameraController.Instance.SetSpeedCoefficient(0, 1f);

        if (CurrentRopeSwingTimeElapsed >= CurrentRopePeriod)
        {
            CurrentRopeSwingDirection *= -1;
            CurrentRopeSwingTimeElapsed = 0f;
            HasChangedHalf = false;
            CameraController.Instance.ChangeDirection(CurrentRopeSwingDirection);
        }

    }


    public void UpdateRopePeriod()
    {
        CurrentRopePeriod = PlayerData.RopePeriodCoefficient * Mathf.PI * Mathf.Sqrt(CurrentRopeLength/-Physics2D.gravity.y);
    }

    public float FindRopeAngle()
    {
        Vector3 direction = RopeAnchorObject.transform.position - transform.position;
        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return angle - 90f;
    }

    public float RopeSwingEaseInOutFunction(float x)
    {
        return (0.5f * Mathf.Sin((Mathf.PI * x) - (0.5f * Mathf.PI))) + 0.5f;
    }
    #endregion

    private void UpdateDirection()
    {
        if (InputState.MovementVector.x != 0 && FacingDirection != Mathf.Sign(InputState.MovementVector.x))
        {
            FacingDirection = (int)Mathf.Sign(InputState.MovementVector.x);
            DirectionChanged = true;
            if(!IsSwinging) CameraController.Instance.ChangeDirection(FacingDirection);
        }
    }
    private void UpdateVelocityDirection()
    {
        if (CurrentVelocity.x != 0 && VelocityDirection != Mathf.Sign(CurrentVelocity.x))
        {
            VelocityDirection = (int)Mathf.Sign(CurrentVelocity.x);
            VelocityDirectionChanged = true;
        }
        if (CurrentVelocity.x == 0)
        {
            VelocityDirection = FacingDirection;
        }
    }

    public void HandleVaulting(int Direction = -1)
    {
        CurrentVelocity.x = Mathf.Clamp(Mathf.Abs(CurrentVelocity.x), PlayerData.BaseVaultingVelocity, PlayerData.MaxHorizontalVelocity) * Direction;
        CurrentVelocity.y = 0f;
    }
    public void HandleHit()
    {
        Debug.Log("Handling Dealing Damage");
    }
    public void HandleAttackEnd()
    {
        Debug.Log("Attack finished");
        IsAttacking = false;
    }
    public void ChangeVelocity(float velocity)
    {
        RelativeVelocity = velocity;

        CurrentVelocity.x = RelativeVelocity * SlopeParallel.x;
        CurrentVelocity.y = RelativeVelocity * SlopeParallel.y + VerticalVelocity;

        if ((velocity < 0f && CurrentWall == -1) || (velocity > 0f && CurrentWall == 1))
        {
            RelativeVelocity = 0f;
            CurrentVelocity.x = 0f;
        }
    }
    public void ThrowShuriken()
    {
        //Debug.Log("Throwing Shuriken\nDirection: " + FacingDirection);
        GameObject Shuriken = Instantiate(shurikenPrefab);
        ProjectileHandler ShurikenPH = Shuriken.GetComponent<ProjectileHandler>();
        ShurikenPH.InitialPosition = transform.position + new Vector3(PlayerData.ShurikenSpawnOffset.x * FacingDirection, PlayerData.ShurikenSpawnOffset.y, 0f);
        ShurikenPH.Speed = PlayerData.ShurikenSpeed;
        ShurikenPH.MaxDistance = PlayerData.ShurikenMaxDistanceFromPlayer;
        ShurikenPH.PlayerSpeed = CurrentVelocity.x;
        ShurikenPH.direction = FacingDirection;
        ShurikenPH.rb.transform.position = transform.position;
        ShurikenPH.InitiateProjectile();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + LeftWallCheckOffset, new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.WallCheckHeight));
        Gizmos.DrawWireCube(transform.position + RightWallCheckOffset, new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.WallCheckHeight));

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + CeilingCheckOffset, new Vector2(PlayerData.VerticalChecksWidth, PlayerData.MinGroundAndCeilingDistance));
        Gizmos.DrawWireCube(transform.position + GroundCheckOffset, new Vector2(PlayerData.VerticalChecksWidth, PlayerData.MinGroundAndCeilingDistance));

        //CurrentVelocity
        Gizmos.DrawRay(transform.position, CurrentVelocity);

        //Slopes
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, SlopeParallel);
        Gizmos.DrawRay(transform.position + LeftSlopeCheckOffset, Vector2.down * PlayerData.SlopeCheckRaycastLength);
        Gizmos.DrawRay(transform.position + RightSlopeCheckOffset, Vector2.down * PlayerData.SlopeCheckRaycastLength);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + RightLedgeCheckOffset, new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.LedgeGrabOccupiedSpaceCheckHeight));
        Gizmos.DrawWireCube(transform.position + LeftLedgeCheckOffset, new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.LedgeGrabOccupiedSpaceCheckHeight));
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + RightLedgeCheckOffset.x, transform.position.y + RightLedgeCheckOffset.y + (PlayerData.LedgeGrabFreeSpaceCheckHeight / 2f) + (PlayerData.LedgeGrabOccupiedSpaceCheckHeight / 2f)), new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.LedgeGrabFreeSpaceCheckHeight));
        Gizmos.DrawWireCube(new Vector2(transform.position.x + LeftLedgeCheckOffset.x, transform.position.y + LeftLedgeCheckOffset.y + (PlayerData.LedgeGrabFreeSpaceCheckHeight / 2f) + (PlayerData.LedgeGrabOccupiedSpaceCheckHeight / 2f)), new Vector2(PlayerData.MaxDistanceForWallContact, PlayerData.LedgeGrabFreeSpaceCheckHeight));

        if (FacingDirection == 0) FacingDirection = 1;
        Gizmos.DrawWireSphere(transform.position + new Vector3(PlayerData.ShurikenSpawnOffset.x * FacingDirection, PlayerData.ShurikenSpawnOffset.y, 0f), 0.5f);


        if (!Application.isPlaying)
        {
            for (int i = 0; i < PlayerData.AttackStats.Length; i++)
            {
                Gizmos.color = PlayerData.AttackStats[i].DebugColor;
                Gizmos.DrawWireSphere(transform.position + new Vector3(PlayerData.AttackStats[i].AttackHitboxRelativePosition.x, PlayerData.AttackStats[i].AttackHitboxRelativePosition.y, 0f), PlayerData.AttackStats[i].AttackHitboxSize);
            }
        }

        StateMachine?.CurrentState?.DrawGizmos();
    }
#endif
}
