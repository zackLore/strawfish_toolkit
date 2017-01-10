using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestWindo : EditorWindow {
    [MenuItem("Window/ Test Editor Window")]
    public static void OpenTestWindo()
    {
        var window = EditorWindow.GetWindow(typeof(TestWindo));
        var title = new GUIContent("Test");
        window.titleContent = title;
    }

    Color currentColor;
    Rect[] pixels;
    Color[] colors;

    private void OnEnable()
    {
        currentColor = new Color(0, 0, 0);
        pixels = new Rect[24 * 24];
        colors = new Color[pixels.Length];
    }

    private void OnGUI()
    {
        int adustedSize = 24 * 4;
        Vector3 MousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

        GUI.Box(new Rect(100, 100, adustedSize, adustedSize), GUIContent.none);
        EditorGUILayout.IntField("Size: ", 24);
        currentColor = EditorGUILayout.ColorField("Color: ", currentColor, null);
        EditorGUI.DrawRect(new Rect(MousePos.x, MousePos.y - position.y, 4, 4), currentColor);
        Repaint();
                
        if (Event.current.type == EventType.MouseDown)//detect mousedown event
        {
            int col = (int)((MousePos.x - position.x) - 100) % adustedSize;
            int row = (int)((MousePos.y - position.y) - 100) % adustedSize;

            row = row / 4;
            col = col / 4;

            Debug.Log("MousePos: " + MousePos + " " + row + ", " + col);
            int entry = (row * 24) + col;
            if (entry <= pixels.Length)
            {
                Debug.Log("Draw Pixel");
                pixels[entry] = new Rect(MousePos.x, MousePos.y, 4, 4);
                colors[entry] = new Color(currentColor.r, currentColor.g, currentColor.b, currentColor.a);
                EditorGUI.DrawRect(pixels[entry], colors[entry]);
            }
        }

        //Draw Colors
        //for (int i = 0; i < pixels.Length; i++)
        //{
        //    if (pixels[i] != null)
        //    {
        //        EditorGUI.DrawRect(pixels[i], colors[i]);
        //        Debug.Log(colors[i]);
        //    }
        //}
    }
}
