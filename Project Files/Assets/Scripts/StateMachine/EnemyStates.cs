using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region Abstract classes
public abstract class EnemyState : BaseState
{
    protected EnemyScript Enemy;

    public EnemyState(EnemyScript enemy)
    {
        this.Enemy = enemy;
    }
}

public abstract class EnemyAbilityState : EnemyState
{
    protected EnemyAbilityState(EnemyScript enemy) : base(enemy)
    {
    }
}
#endregion

#region States

public class EnemyPatrol : EnemyState
{
    public EnemyPatrol(EnemyScript enemy) : base(enemy)
    {
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        Enemy.ExecuteMovement();
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        Enemy.DoPatrol();
    }
    public override void EnterState()
    {
        base.EnterState();
        Enemy.ChooseClosestPatrolPoint();
    }
}

public class PlayerSpotted : EnemyState
{
    public PlayerSpotted(EnemyScript enemy) : base(enemy) { }

}

public class EnemyAttack : EnemyState
{
    public EnemyAttack(EnemyScript enemy) : base(enemy)
    {
    }
}
#endregion