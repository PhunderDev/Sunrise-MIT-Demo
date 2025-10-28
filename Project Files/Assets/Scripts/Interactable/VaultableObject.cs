using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
public class VaultableObject : Interactable
{
    [SerializeField]
    private float DesiredYPosition;

    [SerializeField]
    private float OffsetFromCenter;

    [HideInInspector]
    public Vector3 LeftEntry, RightEntry;

    private BoxCollider2D Col;

    private void OnValidate()
    {
        Col = GetComponent<BoxCollider2D>();
        RightEntry = new Vector3(transform.position.x + OffsetFromCenter, DesiredYPosition, transform.position.z);
        LeftEntry = new Vector3(transform.position.x - OffsetFromCenter, DesiredYPosition, transform.position.z);
        Col.size = new Vector2((2f * OffsetFromCenter) + 1f, 0.6f);
    }

    public override void OnPlayerEnterTrigger(Collider2D collision)
    {
        //base.OnPlayerEnterTrigger(collision);
        collision.GetComponent<PlayerController>().VaultableObject = this;
    }


    public override void OnPlayerExitTrigger(Collider2D collision)
    {
        //base.OnPlayerExitTrigger(collision);
        collision.GetComponent<PlayerController>().VaultableObject = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + OffsetFromCenter, DesiredYPosition, transform.position.z), 0.25f);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x - OffsetFromCenter, DesiredYPosition, transform.position.z), 0.25f);

        Gizmos.DrawLine(new Vector3(transform.position.x + OffsetFromCenter, DesiredYPosition, transform.position.z), new Vector3(transform.position.x - OffsetFromCenter, DesiredYPosition, transform.position.z));
    }
}
