using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
#region Abstract classes
public abstract class PlayerState : BaseState
{
    protected PlayerController Player;
    protected PlayerData stats;
    protected CustomAnimator animator;
    protected string AnimationStateName;
    protected string PreviousAnimationState;

    protected InputState Inputs = new InputState();
    public PlayerState(PlayerController Player, StateMachine stateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName)
    {
        this.StateMachine = stateMachine;
        this.Player = Player;
        this.stats = stats;
        this.animator = animator;
        this.AnimationStateName = AnimationStateName;
        this.Inputs = Player.InputState;
    }

    public override void EnterState()
    {
        base.EnterState();
        //PROSZÊ NIE USUWAÆ OKAZUJE SIÊ ¯E TO S¥ NAJWA¯NIEJSZE 3 LINIJKI W CA£YM STATEMACHINE I NAPRAWIAJ¥ WIÊKSZOŒÆ B£ÊDÓW
        //UPDATE: TERAZ TE 3 LINIJKI KTÓRE TAK NAPRAWDĘ BYŁY 4 LINIJKAMI SĄ JEDNĄ LINIJKĄ
        Inputs = Player.InputState;
        PreviousAnimationState = animator.CurrentStateName;
        if (AnimationStateName != "")
        {
            animator.PlayAnimation(AnimationStateName, Player.FacingDirection);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        Inputs = Player.InputState;
        if(Inputs.InteractInput)
        {
            if (Player.InteractableObject != null)
            {
                Player.InputProcessor.UseInteractInput();
                Player.InteractableObject.Interact();
            }
            else
            {
                Player.InputProcessor.UseInteractInput();
            }
        }
    }
}
public abstract class AbilityState : PlayerState
{
    public AbilityState(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    protected bool isAbilityDone;
    public override void EnterState()
    {
        base.EnterState();
        isAbilityDone = false;
    }
}
#endregion

#region States
public class PlayerIdle : PlayerState
{
    public PlayerIdle(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    public override void EnterState()
    {
        base.EnterState();
        Player.NewHandleGroundMovement(0);
        //Player.CurrentVelocity = Vector2.zero;
        //Player.RelativeVelocity = 0;
        Player.IsForciblyDeccelerating = false;
    }
    public override void PlayerChecks()
    {
        base.PlayerChecks();

        if ((animator.CurrentStateName != "Idle" && animator.animationTime >= 1f) || animator.CurrentStateName == "Run")
        {
            Player.DirectionChanged = false;
            animator.PlayAnimation("Idle", Player.FacingDirection);
        }

        if (Inputs.MovementVector.x != 0)
        {
            if (Player.DirectionChanged)
            {
                if (animator.CurrentStateName != "Rotate")
                {
                    animator.PlayAnimation("Rotate", Player.FacingDirection);
                }
                else if (animator.animationTime >= 1f)
                {
                    Player.DirectionChanged = false;
                    StateMachine.ChangeState(Player.Move);
                }
            }
            else
            {
                if (!(Inputs.MovementVector.x < 0 && Player.CurrentWall == -1) && !(Inputs.MovementVector.x > 0 && Player.CurrentWall == 1)) StateMachine.ChangeState(Player.Move);
            }
        }

        if (Player.CanPressDownInput && Player.CanDropDownWhenIdle && Inputs.MovementVector.y < 0)
        {
            Player.StartCoroutine(Player.DropDown());
            StateMachine.ChangeState(Player.InAir);
        }

        if (Inputs.JumpInput)
        {
            StateMachine.ChangeState(Player.Jump);
        }

        if (Inputs.AttackInput)
        {
            Debug.Log("Attack Input While Idle");
            StateMachine.ChangeState(Player.Attack);
        }

        if (Inputs.ShurikenInput)
        {
            Debug.Log("Shuriken Input While Idle");
            StateMachine.ChangeState(Player.ShurikenThrow);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (Player.CurrentVelocity.x != 0)
        {
            Player.NewHandleGroundMovement(0);
        }
    }

}
public class PlayerMove : PlayerState
{
    public PlayerMove(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    public override void EnterState()
    {
        base.EnterState();
        Player.NewHandleGroundMovement(Inputs.MovementVector.x);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (Inputs.MovementVector.x == 0)
        {
            if (Mathf.Abs(Player.CurrentVelocity.x) < 0.5f * Player.PlayerData.MaxHorizontalVelocity && animator.CurrentStateName != "Stop" && animator.CurrentStateName != "Land" && animator.CurrentStateName != "Dash")
            {
                if (Player.CurrentVelocity.x == 0)
                    animator.PlayAnimation("Stop", Player.VelocityDirection, 0, 3);
                else
                    animator.PlayAnimation("Stop", Player.VelocityDirection);
            }
            if (animator.CurrentStateName == "Stop" && StateMachine.CurrentState == Player.Ascend)
            {
                animator.PlayAnimation("Jump", Player.FacingDirection);
            }
        }
        else
        {
            if (animator.CurrentStateName != "Run" && animator.CurrentStateName != "Jump" && animator.CurrentStateName != "Dash")
                animator.PlayAnimation("Run", Player.FacingDirection);
        }


        if (Player.VaultableObject != null)
        {
            float CloserLedge = 0;
            CloserLedge = Vector2.Distance(Player.transform.position, Player.VaultableObject.LeftEntry) < Vector2.Distance(Player.transform.position, Player.VaultableObject.RightEntry) ? -1 : 1;


            if ((CloserLedge == 1 && Inputs.MovementVector.x < 0) || (CloserLedge == -1 && Inputs.MovementVector.x > 0) || (Inputs.MovementVector.x == 0 && Player.CurrentVelocity.x != 0))
            {
                StateMachine.ChangeState(Player.Vault);
                return;
            }
        }

        if (Inputs.MovementVector.x != 0 && Mathf.Sign(Inputs.MovementVector.x) != Mathf.Sign(Player.CurrentVelocity.x) && Player.CurrentVelocity.x != 0)
        {
            StateMachine.ChangeState(Player.MoveDecelerate);
        }
        if (Player.CanPressDownInput && Inputs.MovementVector.y < 0)
        {
            Player.StartCoroutine(Player.DropDown());
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (!Player.IsGrounded && !Player.IsOnSlope)
        {
            if (Mathf.Abs(Player.FloorAngle) >= stats.MaxSlopeAngle)
            {
                Debug.Log("Change to slide");
            }
            else if (Player.FloorAngle > 0)
            {
                
            }
            else if (Player.FloorAngle == 0)
            {
                StateMachine.ChangeState(Player.InAir);
            }   
        }
        Player.NewHandleGroundMovement(Inputs.MovementVector.x);
    }
    public override void PlayerChecks()
    {
        base.PlayerChecks();
        if (Player.CurrentVelocity.x == 0)
        {
            StateMachine.ChangeState(Player.Idle);
        }
        if (Inputs.JumpInput)
        {
            StateMachine.ChangeState(Player.Jump);
        }
        if (Inputs.AttackInput)
        {
            StateMachine.ChangeState(Player.Attack);
        }
        if (Inputs.ShurikenInput)
        {
            StateMachine.ChangeState(Player.ShurikenThrow);
        }
        if (Inputs.DashInput && Player.CanDash)
        {
            Debug.Log("dash");
            StateMachine.ChangeState(Player.Dash);
        }
    }
}

public class PlayerMoveDecelerate : PlayerMove
{
    public PlayerMoveDecelerate(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        Player.ForceDecceleration();
        animator.PlayAnimation(AnimationStateName, Player.VelocityDirection);
    }
    public override void LogicUpdate()
    {
        if (animator.animationTime >= 0.5f && Player.CurrentVelocity.x == 0 && animator.CurrentStateName != "Land" && animator.CurrentStateName != "Stop")
        {
            if (Player.FacingDirection == Player.VelocityDirection && animator.animationDirection == Player.FacingDirection)
            {
                animator.PlayAnimation(AnimationStateName, -Player.FacingDirection);
            }
            else
            {
                StateMachine.ChangeState(Player.Move);
            }
        }
        if (Inputs.JumpInput)
        {
            StateMachine.ChangeState(Player.Jump);
        }
        if (Player.CanPressDownInput && Inputs.MovementVector.y < 0)
        {
            Player.StartCoroutine(Player.DropDown());
        }
    }
    public override void PhysicsUpdate()
    {
        Player.ForceDecceleration();
    }


}

public class PlayerAscend : PlayerInAir
{
    public PlayerAscend(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    private float timeSinceOnGround;
    private bool HasCutJump;
    private Vector3 JumpStartPoint;
    public override void EnterState()
    {
        base.EnterState();
        Inputs = Player.InputState;
        timeSinceOnGround = 0;
        HasCutJump = false;
        JumpStartPoint = Player.transform.position;
        Player.CurrentGravityMultiplier = stats.JumpGravityMultiplier;
        if (StateMachine.PreviousState == Player.WallJump)
        {
            animator.PlayAnimation("Wall Jump", Player.FacingDirection);
        }
        if (animator.CurrentStateName == "Stop")
        {
            animator.PlayAnimation(AnimationStateName, Player.FacingDirection);
        }
    }
    public override void PlayerChecks()
    {
        base.PlayerChecks();
        if (StateMachine.PreviousState == Player.Jump)
        {
            if (!HasCutJump && Player.transform.position.y >= JumpStartPoint.y + stats.JumpCutoffHeight)
            {
                if (Inputs.HasReleasedJumpButton)
                {
                    Player.CutJump();
                    HasCutJump = true;
                    Inputs.HasReleasedJumpButton = false;
                }
                else
                {
                    HasCutJump = true;
                }
            }
        }

        if (!Player.IsGrounded && Player.CurrentVelocity.y <= 0f && StateMachine.CurrentState == Player.Ascend && StateMachine.CurrentState.time > 0.05f)
        {
            Player.InputProcessor.UseJumpInput();
            StateMachine.ChangeState(Player.InAir);
        }
        if (timeSinceOnGround > 0.1f && Player.IsGrounded)
        {
            timeSinceOnGround = 0f;
            StateMachine.ChangeState(Player.Land);
            return;
        }
        timeSinceOnGround += Time.deltaTime;

    }
}


public class PlayerInAir : PlayerState
{
    public PlayerInAir(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    public override void EnterState()
    {
        base.EnterState();
        Player.CurrentGravityMultiplier = 1f;
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        Player.HandleAirMovement(Inputs.MovementVector.x);
    }
    public override void PlayerChecks()
    {
        base.PlayerChecks();
        if (Inputs.JumpInput && (!Player.IsGrounded && Player.CountingCoyote))
        {
            StateMachine.ChangeState(Player.Jump);
        }

        //Go Into WallSlide

        if (Player.CurrentWall == -1)
        {

            if (/*(Player.CurrentVelocity.y < stats.MaxYVelocityToEnterWallSlide && */Inputs.MovementVector.x < 0/*)*/ || Player.CurrentVelocity.x < 0f)
            {
                StateMachine.ChangeState(Player.WallSlide);
            }
        }
        else if (Player.CurrentWall == 1)
        {
            if (/*(Player.CurrentVelocity.y < stats.MaxYVelocityToEnterWallSlide && */Inputs.MovementVector.x > 0/*)*/ || Player.CurrentVelocity.x > 0)
            {
                StateMachine.ChangeState(Player.WallSlide);
            }
        }


        //Go Into Ledge Grab
        if (((Player.CurrentWall == 1 && Player.CanLedgeGrabRight) || (Player.CurrentWall == -1 && Player.CanLedgeGrabLeft)) && StateMachine.PreviousState != Player.LedgeGrab)
        {
            StateMachine.ChangeState(Player.LedgeGrab);
        }

        //Go Into Rope Swing
        if (Player.InputState.RopeInput)
        {
            if (Player.RopeAnchorObject != null) StateMachine.ChangeState(Player.RopeThrow);
            Player.InputProcessor.UseRopeInput();
        }

        if (Player.IsGrounded && StateMachine.CurrentState.time > 0.05f && StateMachine.CurrentState != Player.Ascend)
        {
            if (Inputs.JumpInput)
            {
                StateMachine.ChangeState(Player.Jump);
            }
            else
            {
                StateMachine.ChangeState(Player.Land);
            }
        }
        if (Player.DirectionChanged && StateMachine.CurrentState == Player.InAir && !animator.IsDuringTransitionAnimation)
        {
            Player.DirectionChanged = false;
            animator.PlayAnimation(AnimationStateName, Player.FacingDirection, 0);
        }
    }
}

public class PlayerWallSlide : PlayerState
{
    public PlayerWallSlide(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)  
    {
    }

    public override void EnterState()
    {
        Player.HandleWallSlideEnter();
        base.EnterState();
    }

    public override void PlayerChecks()
    {
        base.PlayerChecks();

        
        if (Player.IsGrounded && Player.CurrentVelocity.y <= 0) StateMachine.ChangeState(Player.Idle);

        if (Player.CurrentWall == 0 || (Player.CurrentWall == 1 && Inputs.MovementVector.x < 0) || (Player.CurrentWall == -1 && Inputs.MovementVector.x > 0))
        {
            if (StateMachine.PreviousState == Player.LedgeGrab)
            {
                if(StateMachine.CurrentState.time >= stats.LedgeToWallSlideDelay)
                {
                    StateMachine.ChangeState(Player.InAir);
                }
            }
            else
            {
                StateMachine.ChangeState(Player.InAir);
            }
        }

        if (((Player.CurrentWall == 1 && Player.CanLedgeGrabRight) || (Player.CurrentWall == -1 && Player.CanLedgeGrabLeft)) && StateMachine.PreviousState != Player.LedgeGrab)
        {
            StateMachine.ChangeState(Player.LedgeGrab);
        }


        //if (((MovementInputVector.x != 0 && Mathf.Sign(MovementInputVector.x) != Player.CurrentWall) || Player.CurrentWall == 0))
        //{
        //    if(StateMachine.PreviousState != Player.LedgeGrab || (StateMachine.PreviousState == Player.LedgeGrab && StateMachine.CurrentStateOld.time > stats.LedgeClimbDelay)) StateMachine.ChangeAnimState(Player.InAir);
        //}

        /*if (((Player.IsLeftLedgeCollision && Player.IsLeftLedgeFree) || (Player.IsRightLedgeCollision && Player.IsRightLedgeFree)) && Player.CurrentVelocity.y < 0f)
        {
            StateMachine.ChangeAnimState(Player.LedgeGrab);
        }*/

        if (Inputs.JumpInput && (StateMachine.PreviousState == Player.LedgeGrab || (animator.animationTime >= 0.5f && StateMachine.PreviousState != Player.LedgeGrab)))
        {
            StateMachine.ChangeState(Player.WallJump);
        }
    }
}
public class PlayerLedgeGrab : PlayerState
{
    public PlayerLedgeGrab(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        Player.HandleLedgeGrabMovement();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        Player.HandleLedgeGrabMovement();
    }

    public override void PlayerChecks()
    {
        base.PlayerChecks();
        

        if ((Inputs.MovementVector.x > 0 && Player.CurrentWall > 0) || (Inputs.MovementVector.x < 0 && Player.CurrentWall < 0))
        {
            if(StateMachine.CurrentState.time > stats.LedgeClimbDelay && Inputs.JumpInput)
            {
                StateMachine.ChangeState(Player.LedgeClimb);
                return;
            }
        }


        if ((Inputs.MovementVector.x < 0 && Player.CurrentWall > 0) || (Inputs.MovementVector.x > 0 && Player.CurrentWall < 0))
        {
            if (StateMachine.CurrentState.time > stats.LedgeClimbDelay)
            {
                Debug.Log("Back To Wallslide");
                StateMachine.ChangeState(Player.WallSlide);
                return;
            }
        }

        if (Inputs.JumpInput && StateMachine.CurrentState.time >= stats.TimeInWallSlideToWallJump)
        {
            StateMachine.ChangeState(Player.WallJump);
            return;
        }

    }
}

public class PlayerLand : PlayerState
{
    public PlayerLand(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    public override void PlayerChecks()
    {
        base.PlayerChecks();
        if (Player.CurrentVelocity.x != 0)
        {
            animator.PlayAnimation("Land", Player.FacingDirection);
            if (Player.FacingDirection != Mathf.Sign(Player.CurrentVelocity.x))
            {
                Player.RelativeVelocity = 0;
                Player.CurrentVelocity.x = 0;
            }
            StateMachine.ChangeState(Player.Move);
        }
        else
        {
            animator.PlayAnimation("Land", Player.FacingDirection);
            StateMachine.ChangeState(Player.Idle);
        }
    }
    public override void EnterState()
    {
        base.EnterState();
        Player.CanPressDownInput = false;
    }
}

public class PlayerRopeSwing : PlayerState
{
    public PlayerRopeSwing(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }

    public override void EnterState()
    {
        base.EnterState();

        Player.IsHooked = true;
        Player.RopeAnchorObject.Interact();
        Player.PlayerRopeEntryPos = Player.transform.position;
        CameraController.Instance.FollowLocation(Player.RopeAnchorObject.transform.position);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        bool WantsToJumpOff = (Inputs.JumpInput || Inputs.RopeInput) && StateMachine.CurrentState.time >= stats.RopeEnterAndExitTimeMargin;
        if (WantsToJumpOff || !Player.IsHooked)
        {
            StateMachine.ChangeState(Player.InAir);
            Player.InputProcessor.UseRopeInput();
            Player.InputProcessor.UseJumpInput();
            return;
        }

        Player.HandleRopeMovementLogic();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if(Player.IsHooked && Player.IsSwinging) Player.HandleRopeMovementPhysics();
        else Player.HandleAirMovement(Inputs.MovementVector.x);
    }
    public override void ExitState()
    {
        base.ExitState();
        Player.IsHooked = false;
        Player.PlayerRopeExitPos = Player.transform.position;
        Player.RopeAnchorObject.SetAngle(0);
        Player.transform.parent = null;
        SceneManager.MoveGameObjectToScene(Player.gameObject, SceneManager.GetSceneAt(0));
        Player.transform.eulerAngles = Vector3.zero;
        Player.transform.position = Player.PlayerRopeExitPos;
        if(Player.IsSwinging)
        {
            float XtoYRatio = Mathf.Abs(Player.CurrentAngle / (Player.LastTargetRopeAngle * Player.CurrentRopeSwingDirection));
            float SwingRatio = Player.LastTargetRopeAngle / stats.MaxRopeAngle;

            float VelocityY = XtoYRatio * -Physics2D.gravity.y * stats.RopeJumpMultiplier * Player.CurrentRopeLength * SwingRatio;
            if (Player.CurrentRopeSwingDirection != Mathf.Sign(Player.CurrentAngle)) VelocityY *= -1;

            float VelocityX = (1f - XtoYRatio) * -Physics2D.gravity.y * Player.CurrentRopeSwingDirection * stats.RopeJumpMultiplier * Player.CurrentRopeLength * SwingRatio;
            Player.ExecuteRopeJump(new Vector2(VelocityX, VelocityY));
        }
        Player.IsSwinging = false;
        CameraController.Instance.FollowTransform(Player.transform);
        Debug.Log("Exit Velocity: " + Player.CurrentVelocity);
        Debug.Log("Launch Towards The Rope Swing Direction");
    }
}


#endregion

#region Abilities

public class PlayerJump : AbilityState
{
    public PlayerJump(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    public override void EnterState()
    {
        base.EnterState();
        Player.ExecuteJump();
        isAbilityDone = true;
        StateMachine.ChangeState(Player.Ascend);
    }
}

public class PlayerDash : AbilityState
{
    public PlayerDash(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    float TimeToDashRecovery;
    private float PlayerHitboxHeight;
    private float DashTime;
    bool QueueJump = false;
    public override void EnterState()
    {
        base.EnterState();
        Player.ExecuteDash();
        isAbilityDone = true;
        //0.25f = 3 frames/12 fps
        TimeToDashRecovery = stats.DashTime - 0.25f;
        PlayerHitboxHeight = Player.col.size.y;
        Player.col.size = new Vector2(Player.col.size.x, stats.SlideColliderHeight);
        Player.col.offset = new Vector2(0f, -((PlayerHitboxHeight - stats.SlideColliderHeight) / 2f));
        Player.CalculateOffsets();
        Player.CanStandUpAfterDash = false;
        DashTime = 0f;
    }
    public override void ExitState()
    {
        base.ExitState();
        Player.CanCancelDash = false;
        Player.col.offset = Vector2.zero;
        Player.col.size = new Vector2(Player.col.size.x, PlayerHitboxHeight);
        Player.CalculateOffsets();
        Player.StartCoroutine(Player.CountDashCooldown());
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (!Player.IsGrounded && !Player.IsOnSlope)
        {
            if (Player.FloorAngle == 0)
            {
                StateMachine.ChangeState(Player.InAir);
            }
        }
        if (Player.CanStandUpAfterDash) Player.NewHandleGroundMovement(0);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        Player.CheckCeilingWhileDash(PlayerHitboxHeight);
        if (Player.CanStandUpAfterDash)
        {
            DashTime += Time.deltaTime;
        }
    }
    public override void PlayerChecks()
    {
        base.PlayerChecks();
        if (Inputs.JumpInput)
        {
            QueueJump = true;
        }
        if (DashTime >= 0.25f && QueueJump)
        {
            QueueJump = false;
            Player.CanCancelDash = false;
            StateMachine.ChangeState(Player.Jump);
        }
        if (DashTime >= TimeToDashRecovery && animator.CurrentStateName != "Dash Recovery")
        {
            animator.PlayAnimation("Dash Recovery", Player.VelocityDirection);
        }
        if (animator.CurrentStateName == "Dash Recovery")
        {
            if (Player.CanCancelDash)
            {
                if (Inputs.MovementVector.x != 0)
                {
                    Player.CanCancelDash = false;
                    StateMachine.ChangeState(Player.Move);
                }
                else if (animator.animationTime > 1f)
                {
                    StateMachine.ChangeState(Player.Idle);
                }
            }
        }
        if (Player.CanPressDownInput && Inputs.MovementVector.y < 0)
        {
            Player.StartCoroutine(Player.DropDown());
        }
    }
}
public class PlayerVault : AbilityState
{
    public PlayerVault(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    public override void EnterState()
    {
        base.EnterState();

        ObjData = Player.VaultableObject;
        PlayerHitbox = Player.GetComponent<BoxCollider2D>();
        PlayerHitboxHeight = PlayerHitbox.size.y;
        PlayerHitbox.size = new Vector2(PlayerHitbox.size.x, stats.SlideColliderHeight * PlayerHitboxHeight);
        PlayerHitbox.offset = new Vector2(0f, -(stats.SlideColliderHeight/2f) * PlayerHitboxHeight);
        if (Player.transform.position.x < ObjData.transform.position.x)
        {
            Direction = 1;
            ReachedEntry = Player.transform.position.x >= ObjData.LeftEntry.x;
            if(ReachedEntry) Player.transform.position = ObjData.LeftEntry;
        }
        else
        {
            Direction = -1;
            ReachedEntry = Player.transform.position.x <= ObjData.RightEntry.x;
            if(ReachedEntry) Player.transform.position = ObjData.RightEntry;
        }

        if (!ReachedEntry)
        {
            // If haven't reached the entry, force the running animation (No Transition)
            //animator.SendPlayCommandToAnimator(animator.FindAnimationNameFromState(CustomAnimator.AnimationStatesEnum.Run));
        }
        else
        {
            // Once reached the entry, change animation state to vaulting
            //animator.ChangeAnimationStateOld(CustomAnimator.AnimationStatesEnum.Vault);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (!ReachedEntry)
        {
            if (Direction == 1) ReachedEntry = Player.transform.position.x >= ObjData.LeftEntry.x;
            else ReachedEntry = Player.transform.position.x <= ObjData.RightEntry.x;


            if(!ReachedEntry)
            {
                Player.NewHandleGroundMovement(0);
            } else
            {
                Player.transform.position = Direction == 1 ? ObjData.LeftEntry : ObjData.RightEntry;
                //animator.ChangeAnimationStateOld(CustomAnimator.AnimationStatesEnum.Vault);
            }

            return;
        }


        if ((Direction == -1 && Player.transform.position.x <= ObjData.LeftEntry.x) || (Direction == 1 && Player.transform.position.x >= ObjData.RightEntry.x))
        {
            StateMachine.ChangeState(Player.Move);
            return;
        }
        Player.HandleVaulting(Direction);
    }

    public override void ExitState()
    {
        base.ExitState();
        PlayerHitbox.offset = Vector2.zero;
        PlayerHitbox.size = new Vector2(PlayerHitbox.size.x, PlayerHitboxHeight);
    }

    private VaultableObject ObjData;
    private int Direction = -1;
    private bool ReachedEntry = false;
    private float PlayerHitboxHeight;
    private BoxCollider2D PlayerHitbox;
}

public class PlayerWallJump : AbilityState
{
    public PlayerWallJump(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        Player.CurrentGravityMultiplier = stats.JumpGravityMultiplier;
        Player.ExecuteWallJump();
        Player.InputProcessor.UseJumpInput();
        animator.PlayAnimation(AnimationStateName, Player.FacingDirection);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (animator.animationTime >= 1f)
        {
            isAbilityDone = true;
            StateMachine.ChangeState(Player.Ascend);
        }
    }
}

public class PlayerRopeThrow : AbilityState
{
    public PlayerRopeThrow(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("Throwing the rope");
        Player.InputProcessor.UseRopeInput();
        if (Player.RopeAnchorObject == null) return;
        Player.CurrentRopeLength = Vector2.Distance(new Vector2(Player.transform.position.x, Player.transform.position.y), new Vector2(Player.RopeAnchorObject.transform.position.x, Player.RopeAnchorObject.transform.position.y));
        StateMachine.ChangeState(Player.RopeSwing);
    }
}

public class PlayerLedgeClimb : AbilityState
{
    public PlayerLedgeClimb(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("Climbing Up");
        ReachedYOffset = false;
        Direction = Player.CurrentWall;
        TargetPosition.y = Player.transform.position.y + stats.LedgeClimbTargetOffsets.y;
        TargetPosition.x = Player.transform.position.x + stats.LedgeClimbTargetOffsets.x * Direction;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (Player.transform.position.y >= TargetPosition.y) ReachedYOffset = true;
        Debug.Log(Player.transform.position.y + "\n" + TargetPosition.y + "\n" + ReachedYOffset);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (!ReachedYOffset)
        {
            Player.CurrentVelocity = new Vector2(0f, stats.LedgeClimbVelocities.y);
        }
        else
        {
            Player.CurrentVelocity = new Vector2(stats.LedgeClimbVelocities.x * Direction, 0f);
            if ((Direction == -1 && Player.transform.position.x <= TargetPosition.x) || (Direction == 1 && Player.transform.position.x >= TargetPosition.x)) StateMachine.ChangeState(Player.Idle);
        }

    }

    public override void DrawGizmos()
    {
        base.DrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(TargetPosition, 0.5f);
    }

    private bool ReachedYOffset = false;
    private Vector2 TargetPosition;
    private int Direction;
}

public class PlayerAttack : AbilityState
{
    public PlayerAttack(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    private int CurrentAttack;
    private float TimeElapsed;
    
    public override void EnterState()
    {
        base.EnterState();
        CurrentAttack = -1;
        PlayNextAttack();
        Debug.Log("Attack State Initiated");
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        Player.NewHandleGroundMovement(0);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        CountAttackTime();
    }

    private void CountAttackTime()
    {
       if (!Player.IsAttacking)
        {
            TimeElapsed += Time.deltaTime;
        }

        if(!Player.IsAttacking && Inputs.AttackInput && TimeElapsed < stats.AttackChainMargin)
        {
            PlayNextAttack();
        }
        else if(TimeElapsed >= stats.AttackChainMargin)
        {
            StateMachine.ChangeState(Player.Idle);
        }
    }

    private void PlayNextAttack()
    {
        Player.ChangeVelocity(15 * Inputs.MovementVector.x);
        
        Player.InputProcessor.UseAttackInput();
        Player.IsAttacking = true;
        TimeElapsed = 0f;
        // Shift Attack Data Index
        CurrentAttack += 1;
        CurrentAttack = CurrentAttack % stats.AttackStats.Length;
        Debug.LogWarning(CurrentAttack + " atak");

        // Play Animation
        //Player.PlayerAnimator.ChangeAnimationStateOld(stats.AttackStats[CurrentAttack].AnimationState);
    }
    public override void DrawGizmos()
    {
        base.DrawGizmos();
        if(CurrentAttack > -1 || (CurrentAttack < stats.MaxCombo - 1 && !Player.IsAttacking))
        {
            Gizmos.color = stats.AttackStats[CurrentAttack].DebugColor;
            Gizmos.DrawWireSphere(Player.transform.position + new Vector3(stats.AttackStats[CurrentAttack].AttackHitboxRelativePosition.x * Player.FacingDirection, stats.AttackStats[CurrentAttack].AttackHitboxRelativePosition.y, 0f), stats.AttackStats[CurrentAttack].AttackHitboxSize);
        }
    }
}

public class PlayerShurikenThrow : AbilityState
{
    public PlayerShurikenThrow(PlayerController Player, StateMachine StateMachine, PlayerData stats, CustomAnimator animator, string AnimationStateName) : base(Player, StateMachine, stats, animator, AnimationStateName)
    {
    }
    public override void EnterState()
    {
        base.EnterState();
        Player.ThrowShuriken();
        Player.InputProcessor.UseShurikenInput();
        //Initiate the same animation as previously
        Player.ForceDecceleration();
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        Player.ForceDecceleration();
        if (Player.CurrentVelocity.x == 0)
        {
            StateMachine.ChangeState(StateMachine.PreviousState);
        }
    }
}
#endregion