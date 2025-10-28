using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;






[CreateAssetMenu(fileName = "New Dialogue Scheme", menuName = "Dialogue System/Dialogue Scheme", order = 1)]
public class DialogueObject : ScriptableObject
{
    public bool IsReplayable;
    public DialogueCondition[] Conditions;
    public string[] InterruptionReactions;
    public string[] DialogueReentry;
    public string[] ForgetMeNots;
    public DialogueSentence[] Sentences;
}

[System.Serializable]
public class DialogueCondition
{
    public string Name;
    public int Value;
}

[System.Serializable]
public class DialogueVariableModifier
{
    public DialogueAction Dialogue;
    public string VariableName;
    public int VariableValue;
}

[System.Serializable]
public class DialogueConditionCheck
{
    public enum DialogueConditions
    {
        LessThan,
        LessOrEqual,
        Equal,
        MoreOrEqual,
        MoreThan
    }

    public string ConditionName = "Normal";
    public DialogueConditions ConditionRule;
    public int ConditionValue;
}

[System.Serializable]
public class SentenceVariant
{
    public DialogueConditionCheck Condition;
    public string Text = "New Sentence";
}


[System.Serializable]
public class DialogueSentence
{
    public enum Speakers
    {
        ThisCharacter,
        Player
    }
    public Speakers Speaker;
    public SentenceVariant[] Variants = {
        new SentenceVariant()
        {
            Condition = new DialogueConditionCheck()
            {
                ConditionName = "Normal",
                ConditionRule = DialogueConditionCheck.DialogueConditions.Equal,
                ConditionValue = 0
            },
            Text = "New Sentence"
        }
    };
    public bool IsInterruptable = false;
    //public CustomAnimator.Expressions Expression = CustomAnimator.Expressions.None;
}