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

    Color CurrentColor;
    Rect[] pixels;
    Color[] colors;
    Texture2D CurrentImage = null;
    Rect ImageArea;

    #region BrushSize
    private int _brushSize;
    public int BrushSize
    {
        get
        {
            return _brushSize;
        }
        set
        {
            if (value != _brushSize)
            {
                _brushSize = value * (ZoomSize > 0 ? ZoomSize : 1);
            }
        }
    }
    #endregion

    #region ImageSize
    private int _imageSize;
    public int ImageSize
    {
        get
        {
            return _imageSize;
        }
        set
        {
            if (value != _imageSize)
            {
                _imageSize = value;
                adjustedSize = ImageSize * ZoomSize;
                UpdateImageSize();
            }
        }
    }
    #endregion

    #region ZoomSize
    private int _zoomSize;
    public int ZoomSize
    {
        get
        {
            return _zoomSize;
        }
        set
        {
            if (value != _zoomSize)
            {
                _zoomSize = value;
            }
        }
    }
    #endregion

    int adjustedSize = 0;
    int col = 0;
    int row = 0;
    int positionBuffer = 100;

    private void OnEnable()
    {
        //adjustedSize = 32 * 4;
        //pixels = new Rect[24 * 24];
        //colors = new Color[pixels.Length];

        ZoomSize = 4;
        CurrentColor = new Color(0, 0, 0);
        UpdateImageSize();        
    }

    private void OnGUI()
    {
        ImageSize = EditorGUILayout.IntField("Image Size: ", ImageSize);
        BrushSize = EditorGUILayout.IntField("Brush Size: ", BrushSize);
        //adjustedSize = ImageSize * BrushSize;

        Vector3 MousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        //GUI.Box(ImageArea, GUIContent.none);
        EditorGUI.DrawPreviewTexture(ImageArea, CurrentImage);

        CurrentColor = EditorGUILayout.ColorField("Color: ", CurrentColor, null);
        EditorGUI.DrawRect(new Rect(MousePos.x, MousePos.y - position.y, BrushSize, BrushSize), CurrentColor);

        //CurrentImage.SetPixel(0, 0, Color.red);
        
        if (Event.current.type == EventType.MouseDown)//detect mousedown event
        {
            //int col = (int)((MousePos.x - position.x) - 100) % adustedSize;
            //int row = (int)((MousePos.y - position.y) - 100) % adustedSize;

            //row = row / 4;
            //col = col / 4;

            //int col = (int)((MousePos.x - position.x) - 100) % 32;
            //int col = (int)MousePos.x - 100;
            //int row = (int)((MousePos.y - position.y) - 100) % 32;

            //int entry = (row * 24) + col;
            //if (entry <= pixels.Length)
            //{
            Debug.Log("MousePos: " + MousePos);
            //pixels[entry] = new Rect(MousePos.x, MousePos.y, 4, 4);
            //colors[entry] = new Color(currentColor.r, currentColor.g, currentColor.b, currentColor.a);
            //EditorGUI.DrawRect(pixels[entry], colors[entry]);

            //CurrentImage.SetPixel(col, CurrentImage.height - row, currentColor);
            int x = (int)MousePos.x - positionBuffer;
            int y = (int)(MousePos.y - (-MousePos.y)) - positionBuffer;
            CurrentImage.SetPixel((int)MousePos.x - positionBuffer, (int)MousePos.y - positionBuffer, CurrentColor);

            //CurrentImage.SetPixels(col, row, 4, 0, new Color[3] { Color.blue, Color.red, Color.white });
            CurrentImage.Apply();
            //}
        }
        Repaint();

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

    private void UpdateImageSize()
    {
        CurrentImage = new Texture2D(adjustedSize, adjustedSize);
        ImageArea = new Rect(100, 100, CurrentImage.height, CurrentImage.width);
    }
}
