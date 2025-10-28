using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueAction : Interactable
{

    [SerializeField]
    public DialogueObject DialogueScheme;

    [SerializeField]
    private GameObject CanvasGO, ProtagonistCanvasGO;

    [SerializeField]
    [Tooltip("The appearing speed of the text (in characters per second)")]
    private float TextAppearSpeed;
    [SerializeField]
    private TextMeshProUGUI SpeechBubbleText, ProtagonistSpeechBubbleText;



    private bool IsDisplayingText;
    private bool DisplayImmediately = false;

    private bool IsInRange = false;

    private int CurrentSentenceIndex = -1;

    private int PreviouslyChosenInterruptionReactionIndex = 0;
    private int PreviouslyChosenReentryReactionIndex = 0;
    private int PreviouslyChosenForgetMeNotIndex = 0;

    private bool IsSpeaking = false;
    private bool WasInterrupted = false;
    private bool ConversationFinished = false;

    private DialogueInputHandler DialogueInputHandler;
    private CustomAnimator ThisCharacterAnimator, PlayerAnimator;




    public override void OnAwake()
    {
        base.OnAwake();

        if (DialogueInputHandler == null)
        {
            DialogueInputHandler = FindAnyObjectByType<DialogueInputHandler>();
        }

        if (ThisCharacterAnimator == null)
        {
            ThisCharacterAnimator = GetComponentInChildren<CustomAnimator>();
        }
    }

    public override void OnPlayerEnterTrigger(Collider2D collision)
    {
        base.OnPlayerEnterTrigger(collision);
        PlayerAnimator = collision.GetComponentInChildren<CustomAnimator>();
        IsInRange = true;
        ButtonIndicatorImage.sprite = DialogueInputHandler.CurrentInputIndicatorSprites.GetIconForCurrentDevice(DefaultInputHandler.InputData.CurrentInputDeviceType);
    }

    public override void OnPlayerExitTrigger(Collider2D collision)
    {
        base.OnPlayerExitTrigger(collision);
        IsInRange = false;
        DialogueInputHandler.IsConversating = false;
        if (IsSpeaking)
        {
            WasInterrupted = true;
        }

        // Handle the interruption messages when it's the character's turn




        if(CurrentSentenceIndex >= DialogueScheme.Sentences.Length - 1)
        {
            WasInterrupted = false;
            ExitConversation();

            ShowSpeechBubble(false, ProtagonistCanvasGO, 1.5f);
            ShowSpeechBubble(false, CanvasGO, 1.5f);
        }


        if(WasInterrupted && CurrentSentenceIndex != -1)
        {
            DisplaySpeechBubble(BetterRandomSentence(DialogueScheme.InterruptionReactions, ref PreviouslyChosenInterruptionReactionIndex), true);
            ShowSpeechBubble(false, ProtagonistCanvasGO, 1.5f);
            ShowSpeechBubble(false, CanvasGO, 1.5f);
        }
    }


    private string BetterRandomSentence(string[] Possibilities, ref int PreviouslyChosenIndex, int Attempts = 10)
    {
        string S = Possibilities[0];

        for(int i = 0; i < Attempts; i++)
        {
            S = Possibilities[Random.Range(0, Possibilities.Length)];
            if (S == Possibilities[PreviouslyChosenIndex]) continue;
            
            PreviouslyChosenIndex = i;
            break;
        }
        return S;
    }

    private void DisplayNextSentence()
    {
        if (!IsInRange) return;

        if (IsDisplayingText)
        {
            DisplayImmediately = true;
            return;
        }

        if (ConversationFinished)
        {
            DisplaySpeechBubble(BetterRandomSentence(DialogueScheme.ForgetMeNots, ref PreviouslyChosenForgetMeNotIndex), true);
            return;
        }

        if (CurrentSentenceIndex >= DialogueScheme.Sentences.Length - 1)
        {
            ExitConversation();

            ShowSpeechBubble(false, CanvasGO);
            ShowSpeechBubble(false, ProtagonistCanvasGO);
            return;
        }


        if (WasInterrupted) // ConversationFinished is checked before so it's not neccessary to check if it is not true
        {
            DisplaySpeechBubble(BetterRandomSentence(DialogueScheme.DialogueReentry, ref PreviouslyChosenReentryReactionIndex), true);
            WasInterrupted = false;
            if (DialogueScheme.Sentences[CurrentSentenceIndex].Speaker == DialogueSentence.Speakers.ThisCharacter) CurrentSentenceIndex -= 1; // Repeat the last sentence so the conversation doesn't feel akward
            return;
        }

        Debug.Log(4);

        CurrentSentenceIndex += 1;
        IsSpeaking = (CurrentSentence().Speaker == DialogueSentence.Speakers.ThisCharacter);
        DisplaySpeechBubble(FindMatchingSentence(ref CurrentSentenceIndex), CurrentSentence().Speaker == DialogueSentence.Speakers.ThisCharacter);

        if (CurrentSentenceIndex != -1)
        {
            DialogueInputHandler.IsConversating = true;
        }

        DialogueInputHandler.CanInterrupt = CurrentSentence().IsInterruptable;

        //if(CurrentSentence().Speaker == DialogueSentence.Speakers.ThisCharacter)
        //{
        //    if(ThisCharacterAnimator != null && ThisCharacterAnimator.CurrentExpression != CurrentSentence().Expression) ThisCharacterAnimator.SetExpression(CurrentSentence().Expression);
        //    if (PlayerAnimator != null && PlayerAnimator.CurrentExpression != CustomAnimator.Expressions.None) PlayerAnimator.SetExpression(CustomAnimator.Expressions.None);
        //}
        //else if (CurrentSentence().Speaker == DialogueSentence.Speakers.Player)
        //{
        //    if(PlayerAnimator != null && PlayerAnimator.CurrentExpression != CurrentSentence().Expression) PlayerAnimator.SetExpression(CurrentSentence().Expression);
        //    if (ThisCharacterAnimator != null && ThisCharacterAnimator.CurrentExpression != CustomAnimator.Expressions.None) ThisCharacterAnimator.SetExpression(CustomAnimator.Expressions.None);
        //}
    }

    public DialogueSentence CurrentSentence()
    {
        return DialogueScheme.Sentences[CurrentSentenceIndex];
    }

    private void DisplaySpeechBubble(string text, bool selfSpeaking)
    {
        StopAllCoroutines();

        if (selfSpeaking)
        {
            CanvasGO.SetActive(true);
            ProtagonistCanvasGO.SetActive(false);
            StartCoroutine(DisplayBitByBit(text, SpeechBubbleText));
        }
        else
        {
            CanvasGO.SetActive(false);
            ProtagonistCanvasGO.SetActive(true);
            StartCoroutine(DisplayBitByBit(text, ProtagonistSpeechBubbleText));
        }
    }

    private IEnumerator DisplayBitByBit(string text, TextMeshProUGUI TextUIRef)
    {
        IsDisplayingText = true;
        string CurrentlyWritten = "";
        int CurrentCharacterIndex = 0;
        while(CurrentlyWritten != text)
        {
            CurrentlyWritten += text[CurrentCharacterIndex];
            if (DisplayImmediately)
            {
                CurrentlyWritten = text;
                Debug.Log(CurrentlyWritten);
                DisplayImmediately = false;
            }
            TextUIRef.text = CurrentlyWritten;
            CurrentCharacterIndex += 1;
            yield return new WaitForSeconds(1 / TextAppearSpeed);
        }
        IsDisplayingText = false;
        yield return null;
    }

    private void ShowSpeechBubble(bool show, GameObject BubbleObject, float SecondsDelay = 0)
    {
        if (SecondsDelay == 0) BubbleObject.SetActive(show);
        else StartCoroutine(ShowSpeechBubbleDelayed(show, BubbleObject, SecondsDelay));
    }

    private IEnumerator ShowSpeechBubbleDelayed(bool show, GameObject BubbleObject, float SecondsDelay)
    {
        yield return new WaitForSeconds(SecondsDelay);
        ShowSpeechBubble(show, BubbleObject, 0);
        yield return null;
    }

    private string FindMatchingSentence(ref int index)
    {
        if (index >= DialogueScheme.Sentences.Length) return "Err";

        string TextToSay = "";


        int Retries = 4;
        while(Retries > 0)
        {
            for (int i = 0; i < DialogueScheme.Sentences[index].Variants.Length; i++)
            {
                if (DialogueScheme.Sentences[index].Variants[i].Condition.ConditionName == "Normal") TextToSay = DialogueScheme.Sentences[index].Variants[i].Text;

                if (CheckCondition(DialogueScheme.Conditions, DialogueScheme.Sentences[index].Variants[i].Condition))
                {
                    TextToSay = DialogueScheme.Sentences[index].Variants[i].Text;
                    break;
                }

            }
            Debug.Log(TextToSay);
            if (TextToSay != "") break;
            index++;
            Retries--;
        }

        return TextToSay;
    }

    private bool CheckCondition(DialogueCondition[] ConditionStates, DialogueConditionCheck ConditionToMeet)
    {
        bool MeetsCondition = false;

        for (int i = 0; i < ConditionStates.Length; i++)
        {
            if (ConditionStates[i].Name != ConditionToMeet.ConditionName) continue;

            MeetsCondition = CompareConditions(ConditionToMeet.ConditionRule, ConditionStates[i].Value, ConditionToMeet.ConditionValue);
        }

        return MeetsCondition;
    }

    private bool CompareConditions(DialogueConditionCheck.DialogueConditions ConditionRule, int CurrentValue, int DesiredValue)
    {
        bool MeetsCondition = false;

        switch(ConditionRule)
        {
            case DialogueConditionCheck.DialogueConditions.LessThan:
                MeetsCondition = CurrentValue < DesiredValue;
                break;

            case DialogueConditionCheck.DialogueConditions.LessOrEqual:
                MeetsCondition = CurrentValue <= DesiredValue;
                break;

            case DialogueConditionCheck.DialogueConditions.Equal:
                MeetsCondition = CurrentValue == DesiredValue;
                break;

            case DialogueConditionCheck.DialogueConditions.MoreOrEqual:
                MeetsCondition = CurrentValue >= DesiredValue;
                break;

            case DialogueConditionCheck.DialogueConditions.MoreThan:
                MeetsCondition = CurrentValue > DesiredValue;
                break;
        }

        return MeetsCondition;
    }

    private void ExitConversation()
    {
        ConversationFinished = true;
        if (DialogueScheme.IsReplayable)
        {
            ConversationFinished = false;
            CurrentSentenceIndex = -1;
        }
        DialogueInputHandler.IsConversating = false;
        //if (PlayerAnimator != null && PlayerAnimator.CurrentExpression != CustomAnimator.Expressions.None) PlayerAnimator.SetExpression(CustomAnimator.Expressions.None);
    }

    public void ModifyVariable(DialogueVariableModifier Var)
    {
        for(int i = 0; i < DialogueScheme.Conditions.Length; i++)
        {
            if (DialogueScheme.Conditions[i].Name == Var.VariableName)
            {
                DialogueScheme.Conditions[i].Value = Var.VariableValue;
                CurrentSentenceIndex = -1;
                break;
            }
        }

        Debug.Log(Var.VariableName + " => " + Var.VariableValue);
    }

    public override void Interact()
    {
        DisplayNextSentence();
    }
}