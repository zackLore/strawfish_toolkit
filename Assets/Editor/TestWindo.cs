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

    Rect BrushRect;    
    Color[] Colors;
    Texture2D CurrentImage = null;
    Rect ImageArea;
    Vector3 MousePos = new Vector3(0, 0);

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
                _brushSize = value;
                UpdatePixelColorArray();
            }
        }
    }
    #endregion

    #region CurrentColor
    private Color _currentColor;
    public Color CurrentColor
    {
        get
        {
            if (_currentColor == null)
            {
                _currentColor = new Color();
            }
            return _currentColor;
        }
        set
        {
            if (value != _currentColor)
            {
                _currentColor = value;
                if (_currentColor != null)
                {
                    UpdatePixelColorArray();
                }
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

    #region ZoomedBrushSize
    private int _zoomedBrushSize;
    public int ZoomedBrushSize
    {
        get
        {
            _zoomedBrushSize = BrushSize * (ZoomSize > 0 ? ZoomSize : 1);
            return _zoomedBrushSize;
        }
    }
    #endregion

    int adjustedSize = 0;
    int col = 0;
    int row = 0;
    int positionBuffer = 100;

    private void OnEnable()
    {
        ZoomSize = 4;
        CurrentColor = new Color(0, 0, 0);
        ImageSize = 32;
        BrushSize = 1;
        UpdateImageSize(new Texture2D(1,1));
        BrushRect = new Rect(0, 0, 1, 1);      
    }

    private void OnGUI()
    {
        ImageSize = EditorGUILayout.IntField("Image Size: ", ImageSize);
        BrushSize = EditorGUILayout.IntField("Brush Size: ", BrushSize);

        MousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        EditorGUI.DrawPreviewTexture(ImageArea, CurrentImage);

        CurrentColor = EditorGUILayout.ColorField("Color: ", CurrentColor, null);
        BrushRect = new Rect(MousePos.x, MousePos.y - position.y, ZoomedBrushSize, ZoomedBrushSize);

        if (!Cursor.visible)
        {
            EditorGUI.DrawRect(BrushRect, CurrentColor);
        }
        
        if (Event.current.type == EventType.MouseDown)//detect mousedown event
        {
            int x = (int)MousePos.x - positionBuffer;
            int y = (int)(MousePos.y - (-MousePos.y)) - positionBuffer;
            
            x = x >= 0 ? x : 0;

            //adjust y value
            y = y >= 0 ? x : 0;
            y = (int)Mathf.Clamp(ImageArea.height, 0, ImageSize * ZoomSize) - y;
            //y = (int)ImageArea.height - y;

            Debug.Log("MousePos: " + MousePos + " Adjusted x: " + x + " y: " + y);

            //CurrentImage.SetPixel((int)MousePos.x - positionBuffer, (int)MousePos.y - positionBuffer, CurrentColor);

            //TODO: Need to adjust the brush size for the boundaries

            //Testing the cursor position
            CurrentImage.SetPixels(x,
                                    y,
                                    ZoomedBrushSize, 
                                    ZoomedBrushSize, 
                                    GetPixelColorArray(ZoomedBrushSize * ZoomedBrushSize, Color.red));
            
            CurrentImage.Apply();
        }
        Repaint();
    }

    private void Update()
    {
        UpdateBrush();
    }

    private Color[] GetPixelColorArray(int size, Color color)
    {
        Color[] ColorArray = new Color[size];
        for (int i = 0; i < ColorArray.Length; i++)
        {
            Color c = ColorArray[i];
            c = Clone(color);
        }
        return ColorArray;
    }

    private void UpdatePixelColorArray()
    {
        Colors = new Color[ZoomedBrushSize * ZoomedBrushSize];
        for(int i=0; i<Colors.Length; i++)
        {
            Color c = Colors[i];
            c = Clone(CurrentColor);
        }
    }

    private void UpdateBrush()
    {
        if (ImageArea.Overlaps(BrushRect))
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
    }

    #region UpdateImageSize
    private void UpdateImageSize()
    {
        CurrentImage = new Texture2D(adjustedSize, adjustedSize);
        ImageArea = new Rect(100, 100, CurrentImage.height, CurrentImage.width);
    }

    private void UpdateImageSize(Texture2D texture)
    {
        CurrentImage = texture;
        ImageArea = new Rect(100, 100, CurrentImage.height, CurrentImage.width);
    }
    #endregion UpdateImageSize

    //Clones a color
    public Color Clone(Color color)
    {
        return new Color(color.r, color.g, color.b, color.a);
    }
}
