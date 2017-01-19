using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

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
    Rect ImageArea;
    Vector3 MousePos = new Vector3(0, 0);
    Texture2D PreviousFrame;

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

    #region CurrentImage
    private Texture2D _currentImage;
    public Texture2D CurrentImage
    {
        get
        {
            if (_currentImage == null)
            {
                _currentImage = new Texture2D(1,1);
            }
            return _currentImage;
        }
        set
        {
            if (value != _currentImage)
            {
                _currentImage = value;
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
    int positionBuffer = 100;
    float xCorrect = 1f;
    float yCorrect = 10f;
    Vector2 offset;

    private void OnEnable()
    {
        offset = new Vector2(xCorrect, yCorrect);
        ZoomSize = 4;
        CurrentColor = new Color(0, 0, 0);
        ImageSize = 64;
        BrushSize = 4;
        UpdateImageSize(new Texture2D(ImageSize * ZoomSize, ImageSize * ZoomSize));        
        BrushRect = new Rect(0, 0, 1, 1);      
    }

    private void OnGUI()
    {
        ImageSize = EditorGUILayout.IntField("Image Size: ", ImageSize);
        BrushSize = EditorGUILayout.IntField("Brush Size: ", BrushSize);

        MousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        EditorGUI.DrawPreviewTexture(ImageArea, CurrentImage);

        CurrentColor = EditorGUILayout.ColorField("Color: ", CurrentColor, null);
        
        EditorGUILayout.LabelField("Window Pos: " + this.position + " ImagePos: " + ImageArea.position);
        Vector3 adj = new Vector3( Mathf.Clamp(MousePos.x - this.position.x - ImageArea.position.x, 0, ImageArea.width),
                                   Mathf.Clamp(MousePos.y - this.position.y - ImageArea.position.y, 0, ImageArea.height) );

        //Used to calculate the square to draw on the image
        Vector2 previewCoord = FindCoord(adj);        
        //Used to calculate the square to draw on the screen as a preview
        Vector2 previewMouseCoord = FindCoord(MousePos, true);
        
        BrushRect = new Rect(previewMouseCoord.x, previewMouseCoord.y, ZoomedBrushSize, ZoomedBrushSize);

        EditorGUILayout.LabelField("Mouse Pos: " + MousePos + " Relative Mouse Pos: " + adj);
        EditorGUILayout.LabelField("Grid Pos: " + previewCoord + " Preview Mouse: " + previewMouseCoord);

        //Draw the preview brush
        if (!Cursor.visible)
        {
            EditorGUI.DrawRect(BrushRect, CurrentColor);
        }

        //var pixels = CurrentImage.GetPixels32();
        //PreviousFrame = new Texture2D(adjustedSize, adjustedSize);
        //PreviousFrame.SetPixels32(pixels);

        //DrawPixels(previewCoord);

        if (Event.current.type == EventType.MouseDown)//detect mousedown event
        {
            //TODO: This section currently draws one square of the correct size and color but it is not limited to a grid in any way.
            //      The next steps are to take the width of the image, make each brush size = 1 part of the total width so 128 size 
            //      means the brush size of 1 = 1 / 128 and so on.
            
            //Testing the cursor position
            DrawPixels(previewCoord);

            CurrentImage.Apply();
        }
        
        Repaint();
    }

    private void DrawPixels(Vector3 pos)
    {
        int x = 0;
        int y = 0;
        for (int i = 0; i < ZoomedBrushSize; i++)
        {
            x = (int)pos.x + i;
            y = (int)ImageArea.height - (int)pos.y;

            if (x < ImageArea.height && x >= 0 &&
                    y < ImageArea.width && y >= 0)
            {
                CurrentImage.SetPixel(x, y, CurrentColor);
            }

            for (int j = 0; j < ZoomedBrushSize; j++)
            {
                y = (int)ImageArea.height - (int)pos.y - j;

                if (x < ImageArea.height && x >= 0 &&
                    y < ImageArea.width && y >= 0)
                {
                    CurrentImage.SetPixel(x, y, CurrentColor);
                }
            }
        }
    }

    private void DrawPixels(Vector3 pos, Texture2D target)
    {
        int x = 0;
        int y = 0;
        for (int i = 0; i < ZoomedBrushSize; i++)
        {
            x = (int)pos.x + i;
            y = (int)target.height - (int)pos.y;

            if (x < target.height && x >= 0 &&
                    y < target.width && y >= 0)
            {
                target.SetPixel(x, y, CurrentColor);
            }

            for (int j = 0; j < ZoomedBrushSize; j++)
            {
                y = (int)target.height - (int)pos.y - j;

                if (x < target.height && x >= 0 &&
                    y < target.width && y >= 0)
                {
                    target.SetPixel(x, y, CurrentColor);
                }
            }
        }
    }

    private Vector2 FindCoord(Vector3 coords, bool global = false)
    {
        float w = ImageArea.width;
        float h = ImageArea.height;
        //float xRemainder = (coords.x - w);
        //float yRemainder = (coords.y - h);
        //float halfWidth = (w / 2);
        //float halfHeight = (h / 2);
        
        float newX = 0;
        float newY = 0;

        if (global)
        {
            var x = coords.x;
            var y = coords.y - position.y;

            var blockRemainderX = (x % BrushSize);
            var blockRemainderY = (y % BrushSize);            

            newX = blockRemainderX <= BrushSize ? x - blockRemainderX : ((x - blockRemainderX) + BrushSize);
            newY = blockRemainderY <= BrushSize ? y - blockRemainderY : ((coords.y - blockRemainderY) + BrushSize);
        }
        else
        {
            var blockRemainderX = (coords.x % BrushSize);
            var blockRemainderY = (coords.y % BrushSize);

            newX = blockRemainderX <= BrushSize ? Mathf.Clamp(coords.x - blockRemainderX, 0, w) : Mathf.Clamp((coords.x - blockRemainderX) + BrushSize, 0, w);
            newY = blockRemainderY <= BrushSize ? Mathf.Clamp(coords.y - blockRemainderY, 0, h) : Mathf.Clamp((coords.y - blockRemainderY) + BrushSize, 0, h);
        }               
        
        return new Vector2(newX, newY);
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
