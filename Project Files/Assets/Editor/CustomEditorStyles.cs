using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomEditorStyles
{

    public static GUIStyle ArrayManipulatorBoxStyle;
    public static GUIStyle boxStyle;
    public static GUIStyle LabelStyle;

    static CustomEditorStyles()
    {
        ArrayManipulatorBoxStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
        };

        boxStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            padding = new RectOffset(10, 10, 10, 10),
            margin = new RectOffset(0, 0, 0, 0),
        };

        LabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            padding = new RectOffset(0, 0, 0, 0),
            margin = new RectOffset(0, 0, 0, 0),
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };
    }

}
