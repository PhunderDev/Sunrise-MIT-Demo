using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestDialogueModifier : Interactable
{
    public UnityEvent<DialogueVariableModifier> ModifyVariable;

    public DialogueVariableModifier modifier;

    public override void OnPlayerEnterTrigger(Collider2D collision)
    {
        //base.OnPlayerEnterTrigger(collision);
        ModifyVariable.Invoke(modifier);
    }
}
