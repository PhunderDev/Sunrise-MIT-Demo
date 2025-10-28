using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Data/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy Setup")]
    public LayerMask CollisionRaycastLayers;
    public float VerticalChecksWidth;
    public float MinGroundAndCeilingDistance;
    public float SlopeCheckRaycastLength;
    public float ViewDistance;
    public float ViewAngle;
    public float TerminalVelocity;
    [Header("Patrol Setup")]
    public float MinRestTime;
    public float MaxRestTime;
    public float PatrolAcceleration;
    public float PatrolSpeed;
    [Header("Chase Setup")]
    public float ChaseAcceleration;
    public float ChaseSpeed;
    [Header("Attack Setup")]
    public AttackStat[] AttackStats;
}
