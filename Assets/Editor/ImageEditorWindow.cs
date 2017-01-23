using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ImageEditorWindow : EditorWindow {

    // ================================================================================
    // Properties
    // ================================================================================
    #region Properties
    public Texture2D[][] BitmapArray;

    public Rect BrushPreview = new Rect(new Vector2(10, 10), new Vector2(20, 20));

    private Color _currentColor;
    public Color CurrentColor
    {
        get
        {
            if (_currentColor == null)
            {
                _currentColor = new Color(0, 0, 0);
            }
            return _currentColor;
        }
        set
        {
            if (_currentColor != value)
            {
                _currentColor = value;
            }
        }
    }

    public Texture2D CurrentPixel;

    public ImageCanvas DrawingArea;

    private int _imageSize;
    public int ImageSize
    {
        get
        {
            return _imageSize;
        }
        set
        {
            if (_imageSize != value)
            {
                _imageSize = value;
                if (_imageSize > 0 && _imageSize % 4 == 0)
                {
                    Debug.Log("New Array Created");
                    InitializeBitmapArray();
                }
            }
        }
    }

    public Vector3 MousePos = new Vector3(1,1);

    private string mousePosString_;
    public string MousePosString
    {
        get
        {
            if (MousePos != null)
            {
                mousePosString_ = String.Format("x: {0}, y: {1}", MousePos.x.ToString(), MousePos.y.ToString());
            }
            return mousePosString_;
        }
    }
    #endregion Properties

    [MenuItem("Window/ImageEditorWindow")]
    public static void OpenImageEditorWindow()
    {
        var window = EditorWindow.GetWindow(typeof(ImageEditorWindow));
        var title = new GUIContent("Image Editor");
        window.titleContent = title;
    }
    
    // ================================================================================
    // Events
    // ================================================================================
    private void OnGUI()
    {
        //Repaint();
        //GUI.Box(new Rect(100, 100, 100, 100), GUIContent.none);      

        //Cursor.SetCursor(new Texture2D(1, 1), Vector2.zero, CursorMode.ForceSoftware);

        MousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

        EditorGUILayout.LabelField(MousePosString, EditorStyles.boldLabel);
        EditorGUILayout.LabelField(position.width + ", " + position.height, EditorStyles.boldLabel);

        int temp = EditorGUILayout.IntField("Image Size", ImageSize);
        ImageSize = temp % 4 == 0 ? temp : 0;

        CurrentColor = EditorGUILayout.ColorField("Color: ", CurrentColor, null);
        
        int adjustedSize = ImageSize * 4;        
        GUI.Box(new Rect(100, 100, adjustedSize, adjustedSize), GUIContent.none);
        
        DrawingArea = new ImageCanvas(adjustedSize, adjustedSize, new Vector3(position.x + 100, position.y + 100), position.x + 100, position.y + 100);

        EditorGUILayout.LabelField(DrawingArea.L + ", " + DrawingArea.R + ", " + DrawingArea.T + ", " + DrawingArea.B + " | " + "Window Pos: " + position.x + ", " + position.y + " width: " + position.width + " height: " + position.height, EditorStyles.boldLabel);
        BrushPreview = EditorGUI.RectField(new Rect(200, 150, 100, 20), BrushPreview);

        //Mouse is within drawing area
        if (MousePos != null &&
            MousePos.x <= DrawingArea.R &&
            MousePos.x >= DrawingArea.L &&
            MousePos.y >= DrawingArea.T &&
            MousePos.y <= DrawingArea.B)
        {
            Debug.Log("In Drawing Area: " + MousePos.x + ", (" + MousePos.y + ")" + (MousePos.y - position.height));
            //BrushPreview = new Rect(new Vector2(MousePos.x, MousePos.y - position.y), new Vector2(20, 20));
            EditorGUI.DrawRect(new Rect(new Vector2(MousePos.x, MousePos.y - position.y), new Vector2(20, 20)), CurrentColor);                 
            
            //EditorGUI.DrawRect(new Rect(MousePos.x, MousePos.y, 12, 12), CurrentColor);
            //EditorGUI.DrawRect(new Rect(new Vector2(MousePos.x, MousePos.y - position.height), new Vector2(20, 20)), CurrentColor);            
            //EditorGUI.DrawRect(new Rect(new Vector2(MousePos.x, MousePos.y - position.y), new Vector2(20, 20)), CurrentColor);
        }
        Debug.Log(BrushPreview);
        Repaint();
    }

    //private void Update()
    //{

    //}

    // ================================================================================
    // Public Methods
    // ================================================================================
    public void InitializeBitmapArray()
    {
        BitmapArray = new Texture2D[ImageSize][];
        for (int i = 0; i < BitmapArray.Length; i++)
        {
            BitmapArray[i] = new Texture2D[ImageSize];
        }
    }
}
