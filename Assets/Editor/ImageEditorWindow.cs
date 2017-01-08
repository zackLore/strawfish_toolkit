using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class ImageEditorWindow : EditorWindow {
    
    public Texture2D[][] BitmapArray;
    
    private Color _currentColor;
    public Color CurrentColor
    {
        get
        {
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

    public Vector2 MousePos;

    private string mousePosString_;
    public string MousePosString
    {
        get
        {
            if (MousePos != null)
            {
                mousePosString_ = String.Format("x: {0}, y: {1}", MousePos.x.ToString(), MousePos.y.ToString());
            }
            else
            {
                mousePosString_ = "";
            }
            return mousePosString_;
        }
    }

    [MenuItem("Window/ImageEditorWindow")]
    public static void OpenImageEditorWindow()
    {
        var window = EditorWindow.GetWindow(typeof(ImageEditorWindow));
        var title = new GUIContent("Image Editor");
        window.titleContent = title;
    }

    private void OnGUI()
    {
        //if (Selection.activeGameObject == null) { return; }

        //var selection = Selection.activeGameObject.GetComponent<ImageEditor>();
        //if (selection != null)
        //{
        //    Debug.Log("In Editor Window: " + DateTime.Now);
        //}

        //Cursor.SetCursor(new Texture2D(1, 1), Vector2.zero, CursorMode.ForceSoftware);

        MousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

        GUILayout.Label(MousePosString, EditorStyles.boldLabel);
        GUILayout.Label(position.width + ", " + position.height, EditorStyles.boldLabel);

        int temp = EditorGUILayout.IntField("Image Size", ImageSize);
        ImageSize = temp % 4 == 0 ? temp : 0;

        CurrentColor = EditorGUILayout.ColorField("Color: ", CurrentColor, null);

        //EditorGUILayout.RectField(new Rect(10, 10, 100, 100), null);
        int adjustedSize = ImageSize * 4;
        EditorGUI.DrawRect(new Rect(100, 100, adjustedSize, adjustedSize), new Color(0,0,0));
        DrawingArea = new ImageCanvas(adjustedSize, adjustedSize, new Vector3(position.x + 100, position.y + 100), position.x + 100, position.y + 100);

        GUILayout.Label(DrawingArea.L + ", " + DrawingArea.R + ", " + DrawingArea.T + ", " + DrawingArea.B + " | " + "Window Pos: " + position.x + ", " + position.y + " width: " + position.width + " height: " + position.height, EditorStyles.boldLabel);
        

        //Mouse is within drawing area
        if (MousePos != null &&
            MousePos.x <= DrawingArea.R &&
            MousePos.x >= DrawingArea.L &&
            MousePos.y >= DrawingArea.T &&
            MousePos.y <= DrawingArea.B)
        {
            Debug.Log("In Drawing Area");
            EditorGUI.DrawRect(new Rect(MousePos.x, MousePos.y, 12, 12), CurrentColor);            
        }
    }

    public void InitializeBitmapArray()
    {
        BitmapArray = new Texture2D[ImageSize][];
        for (int i = 0; i < BitmapArray.Length; i++)
        {
            BitmapArray[i] = new Texture2D[ImageSize];
        }
    }
}

public class ImageCanvas
{
    public int Height;
    public int Width;
    public Vector3 Position;
    public float XBuffer;
    public float YBuffer;

    private float b_;
    public float B
    {
        get
        {
            return YBuffer + Height;
        }
    }

    private float l_;
    public float L
    {
        get
        {
            return XBuffer;
        }
    }

    private float r_;
    public float R
    {
        get
        {
            return XBuffer + Width;
        }
    }

    private float t_;
    public float T
    {
        get
        {
            return YBuffer;
        }
    }

    public ImageCanvas()
    {
    }

    public ImageCanvas(int height, int width, Vector3 position, float xBuffer, float yBuffer)
    {
        Height = height;
        Width = width;
        Position = position;
        XBuffer = xBuffer;
        YBuffer = yBuffer;
    }
}
