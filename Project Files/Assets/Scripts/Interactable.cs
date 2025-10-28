using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public abstract class Interactable : MonoBehaviour
{
    public GameObject ButtonIndicator;

    [HideInInspector]
    public Image ButtonIndicatorImage;

    [SerializeField]
    private LayerMask PlayerLayer;

    private void Awake()
    {
        OnAwake();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (CompareLayers(PlayerLayer, collision.gameObject.layer)) OnPlayerEnterTrigger(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (CompareLayers(PlayerLayer, collision.gameObject.layer)) OnPlayerExitTrigger(collision);
    }

    public virtual void OnAwake()
    {
        ButtonIndicatorImage = ButtonIndicator.GetComponent<Image>();
    }

    public virtual void OnPlayerEnterTrigger(Collider2D collision)
    {
        if(ButtonIndicator != null) ButtonIndicator.SetActive(true);
        collision.GetComponent<PlayerController>().InteractableObject = this;
    }

    public virtual void OnPlayerExitTrigger(Collider2D collision)
    {
        if(ButtonIndicator != null) ButtonIndicator.SetActive(false);
        collision.GetComponent<PlayerController>().InteractableObject = null;
    }

    public virtual void Interact()
    {

    }

    private bool CompareLayers(LayerMask LayerFromArray, int LayerFromInt)
    {
        return ((LayerFromArray.value & (1 << LayerFromInt)) > 0);
    }
}
