﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.ComponentModel;
using System.Text;

public class TestWindo : EditorWindow {
    [MenuItem("Window/ Test Editor Window")]
    public static void OpenTestWindo()
    {
        var window = EditorWindow.GetWindow(typeof(TestWindo));
        var title = new GUIContent("Test");
        window.titleContent = title;
    }

    Rect BrushRect;
    bool ColorUpdateComplete;
    ColorChange NewPaletteColor;
    bool PaletteColorChanged;
    bool UpdatingColor;
    List<Color> PaletteVerifier = new List<Color>();
    Color[] Colors;
    Color[] NewPixels = null;
    Rect ImageArea;
    Vector3 MousePos = new Vector3(0, 0);
    Texture2D PreviousFrame;

    #region ShowPalette
    private bool _showPalette;
    public bool ShowPalette
    {
        get
        {
            return _showPalette;
        }
        set
        {
            _showPalette = value;
            if (value)
            {
                foreach (var col in ColorPalette)
                {
                    heightGrowthBuffer += 20;
                }
            }
            else
            {
                heightGrowthBuffer = 0;
            }
            UpdateImageSize(CurrentImage);
        }
    }
    #endregion ShowPalette

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

    #region ColorPalette
    private ObservedList<Color> _colorPalette;
    public ObservedList<Color> ColorPalette
    {
        get
        {
            if (_colorPalette == null)
            {
                _colorPalette = new ObservedList<Color>();
                Action<int> a = new Action<int>(PalleteChanged);//when this is displayed in the GUI it fires several times because it is reassigned.  Needs to only fire on the real change.
                _colorPalette.Changed += a;
            }
            return _colorPalette;
        }
        set
        {
            _colorPalette = value;
        }
    }
    #endregion ColorPalette

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
                    if (!ColorPalette.Contains(_currentColor))
                    {
                        //Debug.Log("Color added: " + _currentColor.ToString());
                        ShowPalette = false;
                        //ColorPalette.Add(Clone(_currentColor));
                        //PaletteVerifier.Add(Clone(_currentColor));
                    }
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
    int heightGrowthBuffer = 0;
    int positionBuffer = 100;
    int widthGrowthBuffer = 0;
    float xCorrect = 1f;
    float yCorrect = 10f;

    private void OnEnable()
    {
        ClearValues();
    }

    private void OnGUI()
    {
        //if (!Selection.activeObject == this) { return; }

        if (ColorUpdateComplete)
        {
            if (NewPixels != null)
            {
                //Debug.Log(CurrentImage.GetPixels().Length + " | " + NewPixels.Length);
                Texture2D t = new Texture2D(CurrentImage.width, CurrentImage.height);
                t.SetPixels(NewPixels);
                //EditorGUI.DrawPreviewTexture(ImageArea, t);
                //CurrentImage.SetPixels(NewPixels);
                CurrentImage = t;
                CurrentImage.Apply();
                //Repaint();
                //Debug.Log("Pixels Set" + DateTime.Now);
                ColorUpdateComplete = false;
                NewPixels = null;
            }
        }

        heightGrowthBuffer = 0;

        if (GUILayout.Button("Clear"))
        {
            ClearValues();
        }

        ImageSize = EditorGUILayout.IntField("Image Size: ", ImageSize % 2 == 0 ? ImageSize : 0);
        BrushSize = EditorGUILayout.IntField("Brush Size: ", BrushSize);

        MousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);        

        CurrentColor = EditorGUILayout.ColorField("Color: ", CurrentColor, null);

        ShowPalette = EditorGUILayout.Foldout(ShowPalette, "Color Palette (" + ColorPalette.Count + ")", true);
        if (ShowPalette)
        {
            for (int i = 0; i < ColorPalette.Count; i++)
            {
                //ColorPalette[i] = EditorGUILayout.ColorField("Color: ", ColorPalette[i]);
                Color c = EditorGUILayout.ColorField("Color: ", ColorPalette[i]);
                if (!c.SameColor(ColorPalette[i])) { ColorPalette[i] = c; }
                //EditorGUILayout.ColorField("Color: ", ColorPalette[i]);
            }
         }

        EditorGUI.DrawPreviewTexture(ImageArea, CurrentImage);

        Vector3 adj = new Vector3( Mathf.Clamp(MousePos.x - this.position.x - ImageArea.position.x, 0, ImageArea.width),
                                   Mathf.Clamp(MousePos.y - this.position.y - ImageArea.position.y, 0, ImageArea.height) );

        //Used to calculate the square to draw on the image
        Vector2 previewCoord = FindCoord(adj);
        //Used to calculate the square to draw on the screen as a preview
        Vector2 previewMouseCoord = FindCoord(MousePos, true);

        BrushRect = new Rect(previewMouseCoord.x, previewMouseCoord.y, ZoomedBrushSize, ZoomedBrushSize);

        //EditorGUILayout.LabelField("Window Pos: " + this.position + " ImagePos: " + ImageArea.position);
        //EditorGUILayout.LabelField("Mouse Pos: " + MousePos + " Relative Mouse Pos: " + adj);
        //EditorGUILayout.LabelField("Grid Pos: " + previewCoord + " Preview Mouse: " + previewMouseCoord);

        UpdateBrush();
        //Draw the preview brush
        if (!Cursor.visible)
        {
            EditorGUI.DrawRect(BrushRect, CurrentColor);
        }

        //var pixels = CurrentImage.GetPixels32();
        //PreviousFrame = new Texture2D(adjustedSize, adjustedSize);
        //PreviousFrame.SetPixels32(pixels);

        //DrawPixels(previewCoord);

        if (Event.current.type == EventType.mouseDown || Event.current.type == EventType.MouseDrag)//detect mousedown event
        {
            //TODO: This section currently draws one square of the correct size and color but it is not limited to a grid in any way.
            //      The next steps are to take the width of the image, make each brush size = 1 part of the total width so 128 size 
            //      means the brush size of 1 = 1 / 128 and so on.
            
            //Testing the cursor position
            DrawPixels(previewCoord);
            //Debug.Log("Preview Coord: " + previewCoord);

            CurrentImage.Apply();

            if (!ColorPalette.Contains(CurrentColor))
            {
                ShowPalette = false;
                ColorPalette.Add(Clone(CurrentColor));
                PaletteVerifier.Add(Clone(_currentColor));
            }
        }

        if (PaletteColorChanged)//Change this to a separate thread 
        {
            try
            {
                //UpdatePalette(NewPaletteColor);
                if (!PaletteThread.IsBusy && !UpdatingColor)
                {
                    //Debug.Log("Fire Worker");
                    UpdatingColor = true;
                    Color[] pixels = CurrentImage.GetPixels();
                    PaletteThread.RunWorkerAsync(pixels);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
                
        Repaint();
    }

    public void ClearValues()
    {
        ZoomSize = 4;
        ColorPalette.Clear();
        PaletteVerifier.Clear();
        CurrentColor = new Color(0, 0, 0, 1);
        //ColorPalette.Add(Clone(CurrentColor));
        ImageSize = 64;
        BrushSize = 4;
        UpdateImageSize(new Texture2D(ImageSize * ZoomSize, ImageSize * ZoomSize));
        BrushRect = new Rect(0, 0, 1, 1);
        InitPaletteThread();
        UpdatingColor = false;
        PaletteColorChanged = false;
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

    private Vector2 FindCoord(Vector2 coords, bool global = false)
    {
        float w = ImageArea.width;
        float h = ImageArea.height;

        float newX = 0;
        float newY = 0;

        if (global)
        {
            var x = coords.x - position.x;
            var y = coords.y - position.y;

            var blockRemainderX = (x % BrushSize);
            var blockRemainderY = (y % BrushSize);

            newX = blockRemainderX <= BrushSize ? x - blockRemainderX : ((x - blockRemainderX) + BrushSize);
            newY = blockRemainderY <= BrushSize ? y - blockRemainderY : ((y - blockRemainderY) + BrushSize);
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

    private Vector2 FindCoord(Vector3 coords, bool global = false)
    {
        return FindCoord(new Vector2(coords.x, coords.y), global);
    }

    private void Update()
    {
        //UpdateBrush();
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

    private void PalleteChanged(int num)
    {
        ShowPalette = false;
        //Debug.Log("PalleteChanged");
        //Find the color that changed
        ColorChange changed = new ColorChange();

        for (int i = 0; i < PaletteVerifier.Count; i++)
        {
            Color c = PaletteVerifier.ElementAt(i);

            //Debug.Log("PaletteVerifier[" + i + "] = " + c.ToString() + " ColorPalette[" + i + "] = " + ColorPalette.ElementAt(i).ToString() + " Same = " + c.SameColor(ColorPalette.ElementAt(i)));
            if (!ColorPalette.Contains(c) && !PaletteColorChanged)
            {
                //Debug.Log(PaletteColorChanged);
                changed.OriginalColor = Clone(c);
                changed.NewColor = Clone(ColorPalette.ElementAt(i));
                NewPaletteColor = changed;
                //Debug.Log(changed.OriginalColor + " : " + changed.NewColor + " | " + PaletteColorChanged);
                //return;
            }            
        }

        PaletteColorChanged = true;
    }

    private Color[] UpdatePalette(ColorChange changed, Color[] pixels = null)
    {
        if (changed != null && changed.NewColor != null && changed.OriginalColor != null && pixels != null)
        {
            Color[] newPixels = new Color[pixels.Length];
            Array.Copy(pixels, newPixels, pixels.Length);

            var oldPixels = pixels;
            for (int i = 0; i < oldPixels.Length; i++)
            {
                Color pixel = oldPixels[i];
                if (pixel.SameColor(changed.OriginalColor))
                {
                    newPixels.SetValue(Clone(changed.NewColor), i);
                }
            }

            PaletteVerifier.Clear();
            foreach (var c in ColorPalette)
            {
                PaletteVerifier.Add(Clone(c));
            }
            //var color = PaletteVerifier.Where(x => x == changed.OriginalColor).FirstOrDefault();
            //if (color != null)
            //{

            //    color = changed.NewColor;
            //}
            return newPixels;
        }
        else
        {
            Debug.Log("Changed or pixels where null.");
        }
        return null;
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
    private void UpdateImageSize(bool keepExistingImage = false)
    {
        if (keepExistingImage)
        {
            CurrentImage.Resize(adjustedSize, adjustedSize);
        }
        else
        {
            CurrentImage = new Texture2D(adjustedSize, adjustedSize);
        }
        ImageArea = new Rect(positionBuffer + widthGrowthBuffer, positionBuffer + heightGrowthBuffer, CurrentImage.height, CurrentImage.width);
    }

    private void UpdateImageSize(Texture2D texture)
    {
        CurrentImage = texture;
        ImageArea = new Rect(positionBuffer + widthGrowthBuffer, positionBuffer + heightGrowthBuffer, CurrentImage.height, CurrentImage.width);
    }
    #endregion UpdateImageSize

    // ========================================================================================
    // Helper Methods
    // ========================================================================================    
    public Color Clone(Color color)
    {
        return new Color(color.r, color.g, color.b, color.a);
    }

    // ========================================================================================
    // Helper Threads
    // ========================================================================================   
    BackgroundWorker PaletteThread = null;

    public void InitPaletteThread()
    {
        PaletteThread = new BackgroundWorker();
        PaletteThread.DoWork += PaletteThread_DoWork;
        PaletteThread.RunWorkerCompleted += PaletteThread_RunWorkerCompleted;
    }

    private void PaletteThread_DoWork(object sender, DoWorkEventArgs e)
    {
        var pixels = e.Argument;
        var results = UpdatePalette(NewPaletteColor, (Color[])pixels);
        e.Result = results;
    }

    private void PaletteThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Result != null)
        {
            NewPixels = (Color[])e.Result;
            ColorUpdateComplete = true;
            //CurrentImage.SetPixels(NewPixels);
            //CurrentImage.Apply();

            Debug.Log("Worker Complete");
        }

        PaletteColorChanged = false;
        UpdatingColor = false;
    }
}

//http://answers.unity3d.com/questions/677956/getting-notified-when-modifying-a-collection-ala-c.html
//works like observable collection
[Serializable]
public class ObservedList<T> : List<T>
{
    public event Action<int> Changed = delegate { };
    public event Action Updated = delegate { };
    public new void Add(T item)
    {
        base.Add(item);
        Updated();
    }
    public new void Remove(T item)
    {
        base.Remove(item);
        Updated();
    }
    public new void AddRange(IEnumerable<T> collection)
    {
        base.AddRange(collection);
        Updated();
    }
    public new void RemoveRange(int index, int count)
    {
        base.RemoveRange(index, count);
        Updated();
    }
    public new void Clear()
    {
        base.Clear();
        Updated();
    }
    public new void Insert(int index, T item)
    {
        base.Insert(index, item);
        Updated();
    }
    public new void InsertRange(int index, IEnumerable<T> collection)
    {
        base.InsertRange(index, collection);
        Updated();
    }
    public new void RemoveAll(Predicate<T> match)
    {
        base.RemoveAll(match);
        Updated();
    }


    public new T this[int index]
    {
        get
        {
            return base[index];
        }
        set
        {
            base[index] = value;
            Changed(index);
        }
    }


}

public class ColorChange
{
    public Color OriginalColor;
    public Color NewColor;

    public ColorChange()
    {
    }

    public ColorChange(Color o, Color n)
    {
        OriginalColor = o;
        NewColor = n;
    }

    public override string ToString()
    {
        StringBuilder s = new StringBuilder();
        s.Append("OriginalColor: ").Append(OriginalColor != null ? OriginalColor.ToString() : "null").Append(NewColor != null ? NewColor.ToString() : "null");
        return s.ToString();
    }
}

public static class Extensions
{
    public static bool SameColor(this Color c1, Color c2)//Needs to correctly convert from float to int (0-255)
    {
        //Color32 color1 = new Color32(c1.r, c1.g, c1.b, c1.a);
        //Color32 color2 = new Color32(c2.r, c2.g, c2.b, c2.a);

        //Debug.Log(Mathf.Round(c1.r * 1000f) + " -> " + Mathf.Round(c1.r * 1000f) / 1000f);
        //Debug.Log(Mathf.RoundToInt(c1.r * 10f) + " --> " + Mathf.RoundToInt(c1.r * 10f) / 10f);
        //Debug.Log(c1.r / 255 + " ---> " + c2.r / 255);
        //if (Mathf.RoundToInt(c1.r * 1000f) == Mathf.RoundToInt(c2.r * 1000f) &&
        //    Mathf.RoundToInt(c1.g * 1000f) == Mathf.RoundToInt(c2.g * 1000f) &&
        //    Mathf.RoundToInt(c1.b * 1000f) == Mathf.RoundToInt(c2.b * 1000f) &&
        //    Mathf.RoundToInt(c1.a * 1000f) == Mathf.RoundToInt(c2.a * 1000f))
        //if (Mathf.Round(c1.r * 100f) / 100f == Mathf.Round(c2.r * 100f) / 100f &&
        //    Mathf.Round(c1.g * 100f) / 100f == Mathf.Round(c2.g * 100f) / 100f &&
        //    Mathf.Round(c1.b * 100f) / 100f == Mathf.Round(c2.b * 100f) / 100f &&
        //    Mathf.Round(c1.a * 100f) / 100f == Mathf.Round(c2.a * 100f) / 100f)
        //if (Mathf.Round(c1.r * 10f) / 10f == Mathf.Round(c2.r * 10f) / 10f &&
        //    Mathf.Round(c1.g * 10f) / 10f == Mathf.Round(c2.g * 10f) / 10f &&
        //    Mathf.Round(c1.b * 10f) / 10f == Mathf.Round(c2.b * 10f) / 10f &&
        //    Mathf.Round(c1.a * 10f) / 10f == Mathf.Round(c2.a * 10f) / 10f)
        if (Mathf.Round(c1.r) == Mathf.Round(c2.r) &&
            Mathf.Round(c1.g) == Mathf.Round(c2.g) &&
            Mathf.Round(c1.b) == Mathf.Round(c2.b) &&
            Mathf.Round(c1.a) == Mathf.Round(c2.a) )
        //if(c1.r/255 == c2.r/255 &&
        //    c1.g / 255 == c2.g / 255 &&
        //    c1.b / 255 == c2.b / 255 &&
        //    c1.a / 255 == c2.a / 255 )

        {
            return true;
        }
        return false;
    }
}