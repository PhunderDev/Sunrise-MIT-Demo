using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHandler : MonoBehaviour
{
    public Rigidbody2D rb;

    public float Speed;

    public Vector2 InitialPosition;

    [HideInInspector]
    public int direction = 1;

    [HideInInspector]
    public float PlayerSpeed;

    [HideInInspector]
    public float MaxDistance;

    private bool ShouldBreak = false;

    private void Awake()
    {
        rb.GetComponent<Rigidbody2D>();
        rb.useFullKinematicContacts = true;
    }

    public void InitiateProjectile()
    {
        transform.position = InitialPosition;
        rb.velocity = new Vector2(Speed * direction + PlayerSpeed, 0);
    }


    private void Update()
    {
        if (Vector2.Distance(transform.position, InitialPosition) > MaxDistance) ShouldBreak = true;
        OnHit();
    }


    private void OnHit()
    {
        if (!ShouldBreak) return;
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Shuriken hit detected: " + collision.gameObject.name);
        ShouldBreak = true;
    }

}
