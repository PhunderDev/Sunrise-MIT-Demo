using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class RopeAnchor : Interactable
{
    private RopeAnchorInputHandler RopeAnchorInputHandler;
    public Transform RotatingHook;
    public float Range;


    private void OnValidate()
    {
        this.GetComponent<CircleCollider2D>().radius = Range + 0.1f;
    }

    public override void OnAwake()
    {
        base.OnAwake();

        if (RopeAnchorInputHandler == null)
        {
            RopeAnchorInputHandler = FindAnyObjectByType<RopeAnchorInputHandler>();
        }
    }

    public override void OnPlayerEnterTrigger(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>().RopeAnchorObject != null) return;
        ButtonIndicator.SetActive(true);
        collision.GetComponent<PlayerController>().RopeAnchorObject = this;
        ButtonIndicatorImage.sprite = RopeAnchorInputHandler.CurrentInputIndicatorSprites.GetIconForCurrentDevice(DefaultInputHandler.InputData.CurrentInputDeviceType);
    }

    public override void OnPlayerExitTrigger(Collider2D collision)
    {
        ButtonIndicator.SetActive(false);
        if (collision.GetComponent<PlayerController>().RopeAnchorObject != this) return;
        collision.GetComponent<PlayerController>().RopeAnchorObject = null;
    }

    public override void Interact()
    {
        Debug.Log("Rope Anchor Interaction");
    }

    public void SetAngle(float angle)
    {
        RotatingHook.eulerAngles = new Vector3(0, 0, angle + 180f);
    }
}
