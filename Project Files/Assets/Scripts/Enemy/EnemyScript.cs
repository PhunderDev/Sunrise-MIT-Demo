using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Seeker))]
public class EnemyScript : MonoBehaviour
{
    public StateMachine StateMachine;
    public EnemyPatrol Patrol;
    private Seeker Seeker;

    public bool IsGrounded { get; private set; }
    public Vector2 CurrentVelocity;
    public float TerminalVelocity;
    private Rigidbody2D Rigidbody2D;
    private BoxCollider2D col;
    [SerializeField] Transform GroundCheckTransform;
    private Vector3 LeftSlopeCheckOffset;
    private Vector3 RightSlopeCheckOffset;
    private Vector3 GroundCheckOffset;

    private LayerMask DefaultMask;
    private LayerMask OneWayMask;
    private LayerMask RaycastMask;
    private bool StandingOnPlatform;
    [SerializeField] EnemyData EnemyData;

    float FloorAngle;
    float RelativeVelocity;
    float VerticalVelocity;
    Vector2 SlopeParallel;
    bool IsOnSlope;

    [Header("Pathfinding")]
    public Transform MovementTarget;
    public float PathUpdateInterval;
    private Path Path;
    private int CurrentPathWaypoint;
    private float timeSinceLastPathUpdate = Mathf.NegativeInfinity;
    private bool reachedEndOfPath = false;

    [Header("Patrol")]
    [SerializeField] GameObject[] PatrolPoints;
    private int NextPatrolPoint = 0;
    private bool IsResting = false;
    private Transform PlayerTrans;
    [SerializeField]
    private TMP_Text StateText;

    private void Awake()
    {
        StateMachine = new StateMachine();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Seeker = GetComponent<Seeker>();
        PlayerTrans = GameObject.FindWithTag("Player").transform;
        MovementTarget = PlayerTrans;
        col = GetComponent<BoxCollider2D>();
        LeftSlopeCheckOffset = new Vector3(-col.size.x / 2, -col.size.y / 2, 0);
        RightSlopeCheckOffset = new Vector3(col.size.x / 2, -col.size.y / 2, 0);
        GroundCheckOffset = new Vector3(0, -col.size.y / 2 + col.offset.y, 0);

        DefaultMask = EnemyData.CollisionRaycastLayers;
        OneWayMask = EnemyData.CollisionRaycastLayers;
        OneWayMask |= (1 << LayerMask.NameToLayer("OneWayPlatform"));

        Patrol = new EnemyPatrol(this);
        StateMachine.Initialize(Patrol);
    }
    void Start()
    {
        UpdatePath();
        ChooseClosestPatrolPoint();
    }
    void Update()
    {
        StateMachine.CurrentState.LogicUpdate();
        if (Path != null)
        {
            if (IsResting)
            {
                StateText.text = "Resting" + " (" + NextPatrolPoint + ")";
            }
            else
            StateText.text = StateMachine.CurrentState.ToString() + " (" + CurrentPathWaypoint + "/" + Path.vectorPath.Count + ")";
        }
        UpdatePath();
        
    }
    private void FixedUpdate()
    {
        CheckCollisions();
        HandleGravity();
        StateMachine.CurrentState.PhysicsUpdate();
        ExecuteMovement();
    }

    private void CheckCollisions()
    {
        
        CheckGround();
        CheckSlopes();
        
    }

    private void CheckSlopes()
    {
        RaycastHit2D verLeft = Physics2D.Raycast(transform.position + LeftSlopeCheckOffset, Vector2.down, EnemyData.SlopeCheckRaycastLength, EnemyData.CollisionRaycastLayers);
        RaycastHit2D verRight = Physics2D.Raycast(transform.position + RightSlopeCheckOffset, Vector2.down, EnemyData.SlopeCheckRaycastLength, EnemyData.CollisionRaycastLayers);
        RaycastHit2D horLeft = Physics2D.Raycast(transform.position + LeftSlopeCheckOffset, Vector2.left, Mathf.Abs(RelativeVelocity) * Time.deltaTime / Time.timeScale, EnemyData.CollisionRaycastLayers);
        RaycastHit2D horRight = Physics2D.Raycast(transform.position + RightSlopeCheckOffset, Vector2.right, Mathf.Abs(RelativeVelocity) * Time.deltaTime / Time.timeScale, EnemyData.CollisionRaycastLayers);
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
                else SlopeParallel = -Vector2.Perpendicular(verLeft.normal).normalized;
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
            if (horLeft)
            {
                IsOnSlope = true;
                SlopeParallel = -Vector2.Perpendicular(horLeft.normal).normalized;

            }
            else if (verRight)
            {
                IsOnSlope = true;
                if (verLeft && verLeft.distance < verRight.distance) SlopeParallel = -Vector2.Perpendicular(verLeft.normal).normalized;
                else SlopeParallel = -Vector2.Perpendicular(verRight.normal).normalized;
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

    }

    private void CheckGround()
    {
        IsGrounded = Physics2D.OverlapBox(GroundCheckTransform.position, new Vector2(EnemyData.VerticalChecksWidth, EnemyData.MinGroundAndCeilingDistance), 0f, EnemyData.CollisionRaycastLayers);
        Collider2D[] Collisions = Physics2D.OverlapBoxAll(transform.position + GroundCheckOffset, new Vector2(EnemyData.VerticalChecksWidth, EnemyData.MinGroundAndCeilingDistance), 0f, OneWayMask);
        for (int i = 0; i < Collisions.Length; i++)
        {
            if (Collisions[i].gameObject.layer != LayerMask.NameToLayer("OneWayPlatform"))
            {
                StandingOnPlatform = false;
            }
            else StandingOnPlatform = true;
        }   
    }

    private void HandleGravity()
    {
        float Acceleration = -Physics2D.gravity.y;
        float MinVelocity = -TerminalVelocity;
        if (IsGrounded)
        {
            MinVelocity = Mathf.Clamp(VerticalVelocity, 0f, VerticalVelocity);
        }
        VerticalVelocity = Mathf.Clamp(VerticalVelocity - (Acceleration * Time.fixedDeltaTime), MinVelocity, VerticalVelocity);
        if (!IsOnSlope)
        {
            CurrentVelocity.y = VerticalVelocity;
        }
        else
        {
            //Debug.LogError("AI ENEMY IS ON SLOPE");
        }
    }


    public void DoPatrol()
    {
        if (!IsResting)
        {
            if (reachedEndOfPath)
            {
                StartCoroutine(Rest());
            }
            else
            {
                MoveTo(PatrolPoints[NextPatrolPoint].transform);
            }

            //MoveTo(PlayerTrans);
        }
    }

    public void MoveTo(Transform TargetTransform)
    {
        if (MovementTarget != TargetTransform)
        {
            MovementTarget = TargetTransform;
            CurrentPathWaypoint = 0;
            UpdatePath();
        }
        if (Path == null)
        {
            return;
        }
        reachedEndOfPath = false;
        if (CurrentPathWaypoint < Path.vectorPath.Count)
        {
            while (true)
            {
                if (Mathf.Abs(transform.position.x - Path.vectorPath[CurrentPathWaypoint].x) < 0.5f)
                {
                    if (CurrentPathWaypoint + 1 < Path.vectorPath.Count)
                    {
                        CurrentPathWaypoint += 1;
                    }
                    else
                    {
                        reachedEndOfPath = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (Path.vectorPath[CurrentPathWaypoint].y < transform.position.y)
            {
                
            }
            else
            {

            }
            HandleGroundMovement(Path.vectorPath[CurrentPathWaypoint]);
        }
    }
    IEnumerator Rest()
    {
        RelativeVelocity = 0;
        CurrentVelocity.x = 0;
        IsResting = true;
        ChooseNextPatrolPoint();
        yield return new WaitForSeconds(UnityEngine.Random.Range(EnemyData.MinRestTime, EnemyData.MaxRestTime));
        IsResting = false;
        reachedEndOfPath = false;
    }

    public void ChooseNextPatrolPoint()
    {
        NextPatrolPoint = (NextPatrolPoint + 1) % PatrolPoints.Length;
        MovementTarget = PatrolPoints[NextPatrolPoint].transform;
    }

    public void ChooseClosestPatrolPoint()
    {
        GameObject ClosestPatrolPoint = null;
        for (int i = 0; i < PatrolPoints.Length; i++)
        {
            if (ClosestPatrolPoint == null) ClosestPatrolPoint = PatrolPoints[i];
            else
            {
                if (Vector2.Distance(transform.position, ClosestPatrolPoint.transform.position) > Vector2.Distance(transform.position, PatrolPoints[i].transform.position))
                {
                    ClosestPatrolPoint = PatrolPoints[i];
                    NextPatrolPoint = i;
                }
            }
        }
        MovementTarget = ClosestPatrolPoint.transform;
        Debug.Log(MovementTarget);
    }

    public void HandleGroundMovement(Vector2 PathWaypoint)
    {
        float xAxis;
        float horAccel;
        float horDecel;
        float MaxVel;
        if (StateMachine.CurrentState == Patrol)
        {
            horAccel = EnemyData.PatrolAcceleration;
            horDecel = -EnemyData.PatrolAcceleration;
            MaxVel = EnemyData.PatrolSpeed;
        }
        else
        {
            horAccel = EnemyData.ChaseAcceleration;
            horDecel = -EnemyData.ChaseAcceleration;
            MaxVel = EnemyData.ChaseSpeed;
        }
        if (PathWaypoint.x  < transform.position.x)
        {
            xAxis = -1;
        }
        else
        {
            xAxis = 1;
        }
        NewHandleGroundMovement(xAxis, horAccel, horDecel, MaxVel);
    }
    public void NewHandleGroundMovement(float xAxis, float HorizontalAcceleration, float HorizontalDeceleration, float MaxHorizontalVelocity, float speedCoefficient = 1)
    {
        float NewXVelocity;
        float AccelerationOrDecceleration = HorizontalAcceleration;
        if (xAxis == 0)
        {
            AccelerationOrDecceleration = HorizontalDeceleration;
            NewXVelocity = Mathf.Clamp(Mathf.Abs(RelativeVelocity) - (AccelerationOrDecceleration * Time.fixedDeltaTime), 0, Mathf.Abs(RelativeVelocity)) * Mathf.Sign(RelativeVelocity);
        }
        else
        {
            NewXVelocity = Mathf.Clamp(RelativeVelocity + (AccelerationOrDecceleration * xAxis * Time.fixedDeltaTime), -MaxHorizontalVelocity, MaxHorizontalVelocity);
        }
        RelativeVelocity = NewXVelocity;

        CurrentVelocity.x = RelativeVelocity * SlopeParallel.x * speedCoefficient;
        CurrentVelocity.y = RelativeVelocity * SlopeParallel.y * speedCoefficient + VerticalVelocity;

        //if ((NewXVelocity < 0f && CurrentWall == -1) || (NewXVelocity > 0f && CurrentWall == 1))
        //{
        //    RelativeVelocity = 0f;
        //    CurrentVelocity.x = 0f;
        //}
    }

    public void ExecuteMovement()
    {
        Rigidbody2D.velocity = CurrentVelocity;
    }




    void UpdatePath()
    {
        if (Seeker.IsDone() && MovementTarget != null && Time.time > timeSinceLastPathUpdate + PathUpdateInterval)
        {
            timeSinceLastPathUpdate = Time.time;
            Seeker.StartPath(transform.position, MovementTarget.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path path)
    {
        path.Claim(this);
        if (!path.error)
        {
            if (Path != null) Path.Release(this);
            Path = path;
            CurrentPathWaypoint = 0;
        }
        else
        {
            path.Release(this);
        }

    }

    public void SetTarget(Transform target)
    {
        MovementTarget = target;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(GroundCheckTransform.position, new Vector2(EnemyData.VerticalChecksWidth, EnemyData.MinGroundAndCeilingDistance));
        if (PatrolPoints.Length != 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, PatrolPoints[0].transform.position);
            for (int point = 0; point < PatrolPoints.Length; point++)
            {
                Gizmos.DrawIcon(PatrolPoints[point].transform.position, "EnemyWaypoint.png", true, Color.red);
                if (point + 1 < PatrolPoints.Length)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(PatrolPoints[point].transform.position, PatrolPoints[point + 1].transform.position);
                }
            }
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, SlopeParallel);
    }
#endif
}
