using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using UnityEditorInternal;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEditor.UIElements;
using System.Xml;
using PlasticPipe.PlasticProtocol.Messages;
using Codice.Client.BaseCommands.Merge.Xml;
using Unity.VisualScripting;

public class DialogueEditor : EditorWindow
{
    public DialogueObject DialogueScheme;



    private DialogueObject OldDialogueScheme;
    private SerializedObject so;
    private Vector2 ScrollPos, ReactionsScrollPos, TextAreaScrollPos, ConditionsScrollPos;
    private int CurrentIndex = -1;
    private ReorderableList InterruptionReactionsReorderableList, DialogueReentryReorderableList, VariantsReorderableList, ConditionsReorderableList, ForgetMeNotsList;


    [MenuItem("Window/Dialogue Editor", priority = 9999)]
    public static void ShowWindow()
    {
        GetWindow<DialogueEditor>("Dialogue Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        DrawLeftPanel();

        DrawSeparator(276, 0, 1, position.height, 0);

        DrawRightPanel();

        EditorGUILayout.EndHorizontal();
    }

    private void OnEnable()
    {
        // Removes errors that were popping out whenever you would compile anything
        if (CurrentIndex != -1) SelectFromSentencesArray(-1);

        // Auto Detect DialogueObject
        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        if(Selection.activeObject is DialogueObject dialogueObject)
        {
            DialogueScheme = (DialogueObject)Selection.activeObject;
        }
        else if(Selection.gameObjects.Length == 1)
        {
            DialogueAction Action = Selection.gameObjects[0].GetComponent<DialogueAction>();
            if(Action != null)
            {
                if (Action.DialogueScheme != null) DialogueScheme = Action.DialogueScheme;
            }
        }
        Repaint();
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(275));

        EditorGUILayout.Space(10);
        DialogueScheme = (DialogueObject)EditorGUILayout.ObjectField(DialogueScheme, typeof(DialogueObject), false);

        GUILayout.Space(5);

        BeginCenterHorizontal();
        if (DialogueScheme != null)
        {
            GUILayout.Label("Is Replayable");
            DialogueScheme.IsReplayable = EditorGUILayout.Toggle(DialogueScheme.IsReplayable);
        }
        EndCenterHorizontal();

        GUILayout.Space(10);
        DrawSeparator(0, GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height, 275, 1, 10);

        if (DialogueScheme != null)
        {
            if (so == null || OldDialogueScheme != DialogueScheme)
            {
                so = new SerializedObject(DialogueScheme);
                LoadInterruptionReactionsList();
                LoadDialogueReentryList();
                LoadForgetMeNotsList();
                LoadConditionsList();
                //Debug.Log("Loading");
                OldDialogueScheme = DialogueScheme;
            }



            ReactionsScrollPos = EditorGUILayout.BeginScrollView(ReactionsScrollPos);


            so.Update();

            BeginCenterHorizontal();
            DisplayInterruptionReactionsList();
            EndCenterHorizontal();

            GUILayout.Space(20);

            BeginCenterHorizontal();
            DisplayDialogueReentryList();
            EndCenterHorizontal();

            GUILayout.Space(20);


            if (!DialogueScheme.IsReplayable)
            {
                BeginCenterHorizontal();
                DisplayForgetMeNotsList();
                EndCenterHorizontal();
            }


            so.ApplyModifiedProperties();

            EditorGUILayout.EndScrollView();

            GUILayout.Space(15);

            DrawSeparator(0, GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height, 275, 1, 15);

            ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

            GUILayout.Label("Sentences Sequence", CustomEditorStyles.LabelStyle);

            GUILayout.Space(15);
            if (DialogueScheme.Sentences.Length > 0)
            {
                for (int i = 0; i < DialogueScheme.Sentences.Length; i++)
                {
                    BeginCenterHorizontal();
                    if (i == CurrentIndex)
                    {

                        char UpArrow = '\u25b2';
                        char DownArrow = '\u25BC';
                        EditorGUILayout.BeginVertical(GUILayout.Width(30));

                        if (i > 0)
                        {
                            if (GUILayout.Button(UpArrow.ToString(), CustomEditorStyles.ArrayManipulatorBoxStyle, new GUILayoutOption[] { GUILayout.Width(20), GUILayout.Height(24) })) RearangeArrayElement(-1);
                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            GUILayout.Button(UpArrow.ToString(), CustomEditorStyles.ArrayManipulatorBoxStyle, new GUILayoutOption[] { GUILayout.Width(20), GUILayout.Height(24) });
                            EditorGUI.EndDisabledGroup();
                        }


                        EditorGUILayout.Space(2);


                        if (i < DialogueScheme.Sentences.Length - 1)
                        {
                            if (GUILayout.Button(DownArrow.ToString(), CustomEditorStyles.ArrayManipulatorBoxStyle, new GUILayoutOption[] { GUILayout.Width(20), GUILayout.Height(24) })) RearangeArrayElement(1);
                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            GUILayout.Button(DownArrow.ToString(), CustomEditorStyles.ArrayManipulatorBoxStyle, new GUILayoutOption[] { GUILayout.Width(20), GUILayout.Height(24) });
                            EditorGUI.EndDisabledGroup();
                        }

                        EditorGUILayout.EndVertical();
                    }

                    string DisplayName;

                    try
                    {
                        DisplayName = CropString(DialogueScheme.Sentences[i].Variants[DialogueScheme.Sentences[i].Variants.Length - 1].Text, 20);
                    } catch
                    {
                        DisplayName = "New Sentence";
                    }

                    if (GUILayout.Button(DisplayName, CustomEditorStyles.boxStyle, new GUILayoutOption[] { GUILayout.Width(215), GUILayout.Height(50) }))
                    {
                        SelectFromSentencesArray(i);
                    }


                    EndCenterHorizontal();
                    EditorGUILayout.Space(5);
                }
            }

            EditorGUILayout.EndScrollView();



            EditorGUILayout.Space(5);

            GUIStyle AddBtnStyle = new GUIStyle(GUI.skin.box)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 28,
                alignment = TextAnchor.MiddleCenter
            };

            BeginCenterHorizontal();
            if (GUILayout.Button("+", AddBtnStyle, GUILayout.Height(50), GUILayout.Width(250))) AddElementToSentencesArray();
            EndCenterHorizontal();

            EditorGUILayout.Space(10);
        } else
        {
            so = null;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawRightPanel()
    {
        int margin = 50;
        BeginCenterVertical(GUILayout.Width(position.width - 275));

        if (so != null)
        {
            so.Update();
            SerializedProperty ConditionsProperty = so.FindProperty("Conditions");

            float ListMargin = 25;
            float width = position.width - 275 - (ListMargin * 2);
            float height = (position.height / 3f) - 1;

            BeginCenterHorizontal();
            BeginCenterVertical(GUILayout.Height(height), GUILayout.Width(width));

            ConditionsScrollPos = GUILayout.BeginScrollView(ConditionsScrollPos, GUILayout.Height(height), GUILayout.Width(width));
            DisplayConditionsList();
            GUILayout.EndScrollView();

            EndCenterVertical();
            EndCenterHorizontal();



            GUILayout.Space(20);
            DrawSeparator(276, GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height, position.width - 275, 1, 20);

            so.ApplyModifiedProperties();
        } else
        {
            CurrentIndex = -1;
        }

        if (CurrentIndex >= 0 && CurrentIndex < DialogueScheme.Sentences.Length)
        {
            BeginCenterHorizontal();

            TextAreaScrollPos = EditorGUILayout.BeginScrollView(TextAreaScrollPos, GUILayout.Height(250), GUILayout.Width(position.width - 275 - (margin * 2)));

            // Begin a vertical layout group with a fixed height
            EditorGUILayout.BeginVertical(GUILayout.Height(250));
            DisplayVariantsList();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            EndCenterHorizontal();



            BeginCenterHorizontal();
            GUILayout.Label("Speaker");
            DialogueScheme.Sentences[CurrentIndex].Speaker = (DialogueSentence.Speakers)EditorGUILayout.EnumPopup(DialogueScheme.Sentences[CurrentIndex].Speaker);
            EndCenterHorizontal();

            if (DialogueScheme.Sentences[CurrentIndex].IsInterruptable)
            {
                //DialogueScheme.Sentences[CurrentIndex].Expression = CustomAnimator.Expressions.None;
            }
            else
            {
                BeginCenterHorizontal();
                GUILayout.Label("Expression");
                //DialogueScheme.Sentences[CurrentIndex].Expression = (CustomAnimator.Expressions)EditorGUILayout.EnumPopup(DialogueScheme.Sentences[CurrentIndex].Expression);
                EndCenterHorizontal();
            }

            BeginCenterHorizontal();
            GUILayout.Label("Can Be Interrupted (e.g. By running away)");
            DialogueScheme.Sentences[CurrentIndex].IsInterruptable = EditorGUILayout.Toggle(DialogueScheme.Sentences[CurrentIndex].IsInterruptable);
            EndCenterHorizontal();

            GUILayout.FlexibleSpace();

            BeginCenterHorizontal();
            GUIContent trashIcon = EditorGUIUtility.IconContent("TreeEditor.Trash");
            if (GUILayout.Button(trashIcon, GUILayout.Width(150), GUILayout.Height(50)))
            {
                RemoveCurrentlySelectedSentence();
            }
            EndCenterHorizontal();

            EditorGUILayout.Space(5);
        }


        if (DialogueScheme != null) EditorUtility.SetDirty(DialogueScheme);



        EndCenterVertical();
    }


    #region Stylistical Voids



    private void DrawSeparator(float x, float y, float width, float height, int SpaceSize)
    {
        EditorGUI.DrawRect(new Rect(x, y, width, height), Color.gray);
        GUILayout.Space(SpaceSize);
    }

    private void BeginCenterHorizontal(params GUILayoutOption[] options)
    {
        EditorGUILayout.BeginHorizontal(options);
        EditorGUILayout.Space();
    }

    private void EndCenterHorizontal()
    {
        EditorGUILayout.Space();
        EditorGUILayout.EndHorizontal();
    }

    private void BeginCenterVertical(params GUILayoutOption[] options)
    {
        EditorGUILayout.BeginVertical(options);
        EditorGUILayout.Space();
    }

    private void EndCenterVertical()
    {
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }
    
    #endregion



    #region Functional Voids

    private void SelectFromSentencesArray(int index)
    {
        CurrentIndex = index;
        GUI.FocusControl(null);
        LoadVariantsList();
        Repaint();
    }

    private string CropString(string Str, int MaxLength)
    {
        Str = Str.Replace("\n", " ");

        if (Str.Length >= MaxLength)
        {
            return Str.Substring(0, MaxLength) + "...";
        }

        return Str;
    }

    private void RearangeArrayElement(int Direction)
    {
        DialogueSentence OldSentence = DialogueScheme.Sentences[CurrentIndex];
        DialogueScheme.Sentences[CurrentIndex] = DialogueScheme.Sentences[CurrentIndex + Direction];
        DialogueScheme.Sentences[CurrentIndex + Direction] = OldSentence;
        EditorUtility.SetDirty(DialogueScheme);
        SelectFromSentencesArray(CurrentIndex + Direction);

    }

    private void AddElementToSentencesArray()
    {
        DialogueSentence NewSentence = new DialogueSentence();

        List<DialogueSentence> SentenceList = DialogueScheme.Sentences.ToList();

        SentenceList.Add(NewSentence);

        DialogueScheme.Sentences = SentenceList.ToArray();

        EditorUtility.SetDirty(DialogueScheme);

        SelectFromSentencesArray(DialogueScheme.Sentences.Length - 1);
    }

    private void RemoveCurrentlySelectedSentence()
    {
        List<DialogueSentence> SentenceList = DialogueScheme.Sentences.ToList();
        SentenceList.Remove(DialogueScheme.Sentences[CurrentIndex]);
        DialogueScheme.Sentences = SentenceList.ToArray();
        SelectFromSentencesArray(CurrentIndex - 1);
        EditorUtility.SetDirty(DialogueScheme);
    }

    private string[] GetConditionsAsStringArray()
    {
        List<string> ConditionStrings = new List<string>();
        for(int i = 0; i < DialogueScheme.Conditions.Length; i++)
        {
            ConditionStrings.Add(DialogueScheme.Conditions[i].Name);
        }
        ConditionStrings.Add("Normal");
        return ConditionStrings.ToArray();
    }


        #region ArrayDisplays

    private void LoadVariantsList()
    {
        if (DialogueScheme.Sentences.Length <= 0) return;
        if (DialogueScheme.Sentences.Length < CurrentIndex) return;



        int margin = 2;
        int LineMargin = 2;
        int TopMargin = 10;
        int LineHeight = 20;

        so.Update();

        VariantsReorderableList = new ReorderableList(so, so.FindProperty("Sentences").GetArrayElementAtIndex(CurrentIndex).FindPropertyRelative("Variants"), true, true, true, true);

        VariantsReorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Sentence Variants");
        };

        VariantsReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = VariantsReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            var condition = element.FindPropertyRelative("Condition");


            float width = (rect.width / 3f) - (margin * 2);

            Rect rect1 = new Rect(rect.x, rect.y, width, LineHeight);
            Rect rect2 = new Rect(rect.x + width + margin, rect.y, width, LineHeight);
            Rect rect3 = new Rect(rect.x + (width * 2) + (margin * 2), rect.y, width, LineHeight);
            Rect rect4 = new Rect(rect.x, rect.y + LineHeight + LineMargin, rect.width, LineHeight);

            string[] ConditionsArray = GetConditionsAsStringArray();

            try
            {
                condition.FindPropertyRelative("ConditionName").stringValue = ConditionsArray[EditorGUI.Popup(rect1, Array.IndexOf(ConditionsArray, condition.FindPropertyRelative("ConditionName").stringValue), ConditionsArray)];
            }
            catch
            {
                condition.FindPropertyRelative("ConditionName").stringValue = ConditionsArray[ConditionsArray.Length - 1];
            }

            EditorGUI.PropertyField(rect2, condition.FindPropertyRelative("ConditionRule"), GUIContent.none);

            EditorGUI.PropertyField(rect3, condition.FindPropertyRelative("ConditionValue"), GUIContent.none);

            EditorGUI.PropertyField(rect4, element.FindPropertyRelative("Text"), GUIContent.none);
        };

        VariantsReorderableList.elementHeightCallback = (index) =>
        {
            return (LineHeight * 2) + LineMargin + TopMargin;
        };
    }

    private void DisplayVariantsList()
    {
        if (so == null) return;
        if (DialogueScheme.Sentences.Length <= 0) return;
        if (CurrentIndex < 0 || CurrentIndex >= DialogueScheme.Sentences.Length) return;

        so.Update();
        VariantsReorderableList.DoLayoutList();
        so.ApplyModifiedProperties();
    }

    private void LoadInterruptionReactionsList()
    {
        int TopMargin = 5;

        so.Update();

        InterruptionReactionsReorderableList = new ReorderableList(so, so.FindProperty("InterruptionReactions"), true, true, true, true);

        InterruptionReactionsReorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Interruption Reactions");
        };

        InterruptionReactionsReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = InterruptionReactionsReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2; ;


            Rect rect1 = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(rect1, element, GUIContent.none);
        };

        InterruptionReactionsReorderableList.elementHeightCallback = (index) =>
        {
            return EditorGUIUtility.singleLineHeight + TopMargin;
        };
    }

    private void DisplayInterruptionReactionsList()
    {
        if (so == null) return;

        so.Update();
        InterruptionReactionsReorderableList.DoLayoutList();
        so.ApplyModifiedProperties();
    }

    private void LoadDialogueReentryList()
    {
        int TopMargin = 5;

        so.Update();

        DialogueReentryReorderableList = new ReorderableList(so, so.FindProperty("DialogueReentry"), true, true, true, true);

        DialogueReentryReorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Dialogue Reentry Phrases");
        };

        DialogueReentryReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = DialogueReentryReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2; ;


            Rect rect1 = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(rect1, element, GUIContent.none);
        };

        DialogueReentryReorderableList.elementHeightCallback = (index) =>
        {
            return EditorGUIUtility.singleLineHeight + TopMargin;
        };
    }

    private void DisplayDialogueReentryList()
    {
        if (so == null) return;

        so.Update();
        DialogueReentryReorderableList.DoLayoutList();
        so.ApplyModifiedProperties();
    }

    private void LoadForgetMeNotsList()
    {
        int TopMargin = 5;

        so.Update();

        ForgetMeNotsList = new ReorderableList(so, so.FindProperty("ForgetMeNots"), true, true, true, true);

        ForgetMeNotsList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Forget Me Not Phrases");
        };

        ForgetMeNotsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = ForgetMeNotsList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2; ;


            Rect rect1 = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(rect1, element, GUIContent.none);
        };

        ForgetMeNotsList.elementHeightCallback = (index) =>
        {
            return EditorGUIUtility.singleLineHeight + TopMargin;
        };
    }

    private void DisplayForgetMeNotsList()
    {
        if (so == null) return;

        so.Update();
        ForgetMeNotsList.DoLayoutList();
        so.ApplyModifiedProperties();
    }






    private void LoadConditionsList()
    {
        int TopMargin = 5;

        so.Update();

        ConditionsReorderableList = new ReorderableList(so, so.FindProperty("Conditions"), true, true, true, true);

        ConditionsReorderableList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Condition Variables");
        };

        ConditionsReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = ConditionsReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var name = element.FindPropertyRelative("Name");
            var value = element.FindPropertyRelative("Value");
            rect.y += 2;


            int margin = 2;
            float width = (rect.width / 3f) - margin;





            if (index == 0)
            {
                Rect rect11 = new Rect(rect.x, rect.y, width * 2, EditorGUIUtility.singleLineHeight);
                Rect rect12 = new Rect(rect.x + (width * 2) + 2, rect.y, width, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(rect11, "Name");
                EditorGUI.LabelField(rect12, "Value");
                rect.y += EditorGUIUtility.singleLineHeight + TopMargin;
            }





            Rect rect1 = new Rect(rect.x, rect.y, width * 2, EditorGUIUtility.singleLineHeight);
            Rect rect2 = new Rect(rect.x + (width * 2) + margin, rect.y, width, EditorGUIUtility.singleLineHeight);



            EditorGUI.PropertyField(rect1, name, GUIContent.none);
            EditorGUI.PropertyField(rect2, value, GUIContent.none);
        };

        ConditionsReorderableList.elementHeightCallback = (index) =>
        {
            if (index == 0) return (EditorGUIUtility.singleLineHeight * 2) + (TopMargin * 2);

            return EditorGUIUtility.singleLineHeight + TopMargin;
        };
    }

    private void DisplayConditionsList()
    {
        if (so == null) return;

        so.Update();
        ConditionsReorderableList.DoLayoutList();
        so.ApplyModifiedProperties();
    }

        #endregion

    #endregion
}

[CustomPropertyDrawer(typeof(SentenceVariant))]
public class SentenceVariantDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var ConditionSO = property.FindPropertyRelative("Condition");
        var ConditionName = ConditionSO.FindPropertyRelative("ConditionName");
        var ConditionRule = ConditionSO.FindPropertyRelative("ConditionRule");
        var ConditionValue = ConditionSO.FindPropertyRelative("ConditionValue");
        var Text = property.FindPropertyRelative("Text");

        int margin = 2;
        float width = (position.width / 4f) - (margin * 3);

        Rect rect = new Rect(position.x, position.y, width, 20);
        Rect rect1 = new Rect(position.x + width + margin, position.y, width, 20);
        Rect rect2 = new Rect(position.x + (width * 2) + (margin * 2), position.y, width, 20);
        Rect rect3 = new Rect(position.x + (width * 3) + (margin * 3), position.y, width, 20);

        EditorGUI.PropertyField(rect, ConditionName, GUIContent.none);
        EditorGUI.PropertyField(rect1, ConditionRule, GUIContent.none);
        EditorGUI.PropertyField(rect2, ConditionValue, GUIContent.none);
        EditorGUI.PropertyField(rect3, Text, GUIContent.none);

        EditorGUI.EndProperty();
    }
}









[CustomPropertyDrawer(typeof(DialogueVariableModifier))]
public class DialogueVariableModifierDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float width = (position.width - EditorGUIUtility.labelWidth) / 3;
        float ActionWidth = width + EditorGUIUtility.labelWidth;


        Rect DialogueRect = new Rect(position.x, position.y, ActionWidth - 5, EditorGUIUtility.singleLineHeight);
        Rect VariableNameRect = new Rect(position.x + ActionWidth + 5, position.y, width - 5, EditorGUIUtility.singleLineHeight);
        Rect VariableValueRect = new Rect(position.x + ActionWidth + width + 5, position.y, width - 5, EditorGUIUtility.singleLineHeight);
        Rect DefaultRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        SerializedProperty dialogueProperty = property.FindPropertyRelative("Dialogue");
        SerializedProperty variableNameProperty = property.FindPropertyRelative("VariableName");
        SerializedProperty variableValueProperty = property.FindPropertyRelative("VariableValue");

        // If Dialogue is assigned, draw the VariableName field as enum
        if (dialogueProperty.objectReferenceValue != null)
        {
            // Draw Dialogue field
            EditorGUI.PropertyField(DialogueRect, dialogueProperty);

            DialogueAction dialogue = dialogueProperty.objectReferenceValue as DialogueAction;
            if (dialogue != null && dialogue.DialogueScheme != null && dialogue.DialogueScheme.Conditions.Length != 0)
            {
                string[] conditionNames = new string[dialogue.DialogueScheme.Conditions.Length];
                for (int i = 0; i < conditionNames.Length; i++)
                {
                    conditionNames[i] = dialogue.DialogueScheme.Conditions[i].Name;
                }

                int selectedIndex = Mathf.Max(0, System.Array.IndexOf(conditionNames, variableNameProperty.stringValue));
                selectedIndex = EditorGUI.Popup(VariableNameRect, selectedIndex, conditionNames);

                EditorGUI.PropertyField(VariableValueRect, variableValueProperty, GUIContent.none);

                variableNameProperty.stringValue = conditionNames[selectedIndex];
            }

        }
        else
        {
            // Draw Dialogue field
            EditorGUI.PropertyField(DefaultRect, dialogueProperty);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty dialogueProperty = property.FindPropertyRelative("Dialogue");

        if (dialogueProperty.objectReferenceValue != null)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
        else
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}