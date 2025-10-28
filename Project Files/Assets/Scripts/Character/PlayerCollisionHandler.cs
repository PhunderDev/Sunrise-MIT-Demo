using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PlayerCollisionInteraction
{
    public string ObjectTag;
    public UnityEvent OnCollisionEnter;
    public UnityEvent OnCollisionExit;
    public UnityEvent OnTriggerEnter;
    public UnityEvent OnTriggerExit;
}

public class PlayerCollisionHandler : MonoBehaviour
{
    [SerializeField]
    private PlayerCollisionInteraction[] AvailableInteractions;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach(PlayerCollisionInteraction interaction in AvailableInteractions)
        {
            if(interaction.ObjectTag == collision.transform.tag)
            {
                interaction.OnCollisionEnter?.Invoke();
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (PlayerCollisionInteraction interaction in AvailableInteractions)
        {
            if (interaction.ObjectTag == collision.transform.tag)
            {
                interaction.OnTriggerEnter?.Invoke();
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        foreach (PlayerCollisionInteraction interaction in AvailableInteractions)
        {
            if (interaction.ObjectTag == collision.transform.tag)
            {
                interaction.OnCollisionExit?.Invoke();
                break;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        foreach (PlayerCollisionInteraction interaction in AvailableInteractions)
        {
            if (interaction.ObjectTag == collision.transform.tag)
            {
                interaction.OnTriggerExit?.Invoke();
                break;
            }
        }
    }
}
