using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.ComponentModel;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using VNKit;

// =====================
// To Do
// =====================
// 1. Zoom -
//      Zoom is off by a small amount causing the image to distort.  
//      Need to create a color coordinate system to track positions
//      of actual pixels regardless of size.  Then upon resize the
//      image will be redrawn with the current brush size and zoom
//      using the coordinate rather than the mouse position. Possibly
//      use a large array to keep track of color positions so that if 
//      the image scales up there does not need to be another array
//      created and the image data will persist between zooms.
// 2. Preview Windows
// 3. Import Image
// =====================

public class ImageEditorWindow : EditorWindow {
    [MenuItem("Window/ Image Editor")]
    public static void OpenTestWindo()
    {
        var window = EditorWindow.GetWindow(typeof(ImageEditorWindow));
        var title = new GUIContent("Image Editor");
        window.titleContent = title;
    }

    public Color[][] ImageData;

    #region Icons
    Texture2D EraserIcon;
    Texture2D PaintBucketIcon;
    #endregion Icons

    Color Eraser;
    List<Texture2D> ButtonTextures;
    List<Texture2D> PaletteButtonTextures;

    Rect BrushRect;
    Rect ButtonRect;
    bool ColorUpdateComplete;
    ColorChange NewPaletteColor;
    bool PaletteColorChanged;
    bool UpdatingColor;
    List<Color> PaletteVerifier = new List<Color>();
    Color[] NewPixels = null;
    Rect ImageArea;
    Vector3 MousePos = new Vector3(0, 0);
    Texture2D PreviousFrame;
    bool SnapToGrid;

    Vector2 Scale = new Vector2(1, 1);

    public ColorPalette Palette;
    public int CurrentPaletteIndex = 0;
    public PaletteTracker PaletteList = new PaletteTracker();

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
            //if (value)
            //{
            //    if(PaletteList.Entries.Count > 0)
            //    {
            //        heightGrowthBuffer += 20;
            //    }
            //}
            //else
            //{
            //    heightGrowthBuffer = 0;
            //}
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
                //Action<int> a = new Action<int>(PalleteChanged);//when this is displayed in the GUI it fires several times because it is reassigned.  Needs to only fire on the real change.
                //_colorPalette.Changed += a;
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
                if (PaletteList.Contains(_currentColor))
                {
                    CurrentPaletteIndex = PaletteList.GetIndex(_currentColor);
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
            _zoomSize = _zoomSize < 1 ? 1 : _zoomSize;
            return _zoomSize;
        }
        set
        {
            if (value != _zoomSize)
            {
                Texture2D newImage = new Texture2D(CurrentImage.width, CurrentImage.height);
                newImage.SetPixels(CurrentImage.GetPixels());
                TextureScale.Point(newImage, (CurrentImage.width / ZoomSize) * value, (CurrentImage.height / ZoomSize) * value);
                CurrentImage = newImage;
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
    int positionBuffer = 170;
    int widthGrowthBuffer = 0;
    float xCorrect = 1f;
    float yCorrect = 10f;

    private int imaageDataSize;

    private void OnEnable()
    {
        //initialize array
        imaageDataSize = 1000;
        ImageData = new Color[imaageDataSize][];
        for (int i = 0; i < ImageData.Length; i++)
        {
            ImageData[i] = new Color[imaageDataSize];
        }

        LoadIcons();
        Eraser = new Color(.85f, .85f, .85f, 0);
        positionBuffer = 185;
        SnapToGrid = true;
        ButtonRect = new Rect(position.width / 2 - 100, position.height - 50, 200, 200);
        ClearValues();
    }

    private void LoadIcons()
    {
        try
        {
            EraserIcon = Resources.Load("Images/eraser2") as Texture2D;
            PaintBucketIcon = Resources.Load("Images/paintBucket") as Texture2D;
        }
        catch (Exception ex)
        {
            EraserIcon = new Texture2D(25, 25);
            PaintBucketIcon = new Texture2D(25, 25);
            Debug.Log(ex.ToString());
        }
    }

    private void OnGUI()
    {
        heightGrowthBuffer = 0;

        //if (GUILayout.Button("Zoom"))
        //{
        //    Scale += new Vector2(20f, -1f);
        //    //GUIUtility.ScaleAroundPivot(Scale, new Vector2(window.position.width, window.position.height));
        //    //GUIUtility.ScaleAroundPivot(new Vector2(Screen.width / 1440.0f, Screen.height / 900.0f), new Vector2(0.0f, 0.0f));
        //    Debug.Log(Scale);
        //}

        ZoomSize = EditorGUILayout.IntSlider(ZoomSize, 1, 10);
        ImageSize = EditorGUILayout.IntField("Image Size: ", ImageSize % 2 == 0 ? ImageSize : 0);
        BrushSize = EditorGUILayout.IntField("Brush Size: ", BrushSize);

        MousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);        

        CurrentColor = EditorGUILayout.ColorField("Color: ", CurrentColor, null);
        
        AddColorButtons(Palette.DefaultColors);

        ShowPalette = EditorGUILayout.Foldout(ShowPalette, "Color Palette (" + PaletteList.Entries.Count + ")", true);
        if (ShowPalette)
        {
            AddPaletteTrackerButtons(PaletteList.GetColors());
        }

        EditorGUILayout.BeginHorizontal();
        SnapToGrid = EditorGUILayout.Toggle("Snap To Grid", SnapToGrid);

        var iconPadding = GUI.skin.button;
        GUIStyle iconStyle = new GUIStyle(iconPadding);
        iconStyle.padding = new RectOffset(0, 0, 5, 5);

        if (GUILayout.Button(PaintBucketIcon, iconStyle, GUILayout.Width(32), GUILayout.Height(32)))
        {
            PaintPixels(CurrentImage, CurrentColor);
        }

        if (GUILayout.Button(EraserIcon, iconStyle, GUILayout.Width(32), GUILayout.Height(32)))
        {
            CurrentColor = Eraser;
        }

        if (GUILayout.Button("Clear", GUILayout.Height(32)))
        {
            ClearValues();
        }

        if (GUILayout.Button("Save Image", GUILayout.Height(32)))
        {
            //handle save
            SaveImage();
        }    
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.DrawPreviewTexture(ImageArea, CurrentImage);
        
        Vector3 adj = new Vector2(MousePos.x - this.position.x - ImageArea.position.x,
                                  MousePos.y - this.position.y - ImageArea.position.y);

        //Used to calculate the square to draw on the image
        Vector2 previewCoord = FindCoord(adj);
        //Used to calculate the square to draw on the screen as a preview
        Vector2 previewMouseCoord = FindCoord(MousePos, true);

        BrushRect = new Rect(previewMouseCoord.x, previewMouseCoord.y, ZoomedBrushSize, ZoomedBrushSize);

        UpdateBrush();
        //Draw the preview brush
        if (ImageArea.Overlaps(BrushRect))
        {
            EditorGUI.DrawRect(BrushRect, Color.black);
            EditorGUI.DrawRect(new Rect(BrushRect.x + 1, BrushRect.y + 1, BrushRect.width - 2, BrushRect.height - 2), Color.white);
            EditorGUI.DrawRect(new Rect(BrushRect.x + 2, BrushRect.y + 2, BrushRect.width - 4, BrushRect.height - 4), CurrentColor);
        }

        // =====================================
        // Mouse Down Event
        // =====================================
        if ((Event.current.type == EventType.mouseDown || Event.current.type == EventType.MouseDrag) && ImageArea.Overlaps(BrushRect))
        {            
            if (!PaletteList.Contains(CurrentColor))
            {
                //Debug.Log("Adding Color: " + CurrentColor + " pos: " + MousePos);
                if (!CurrentColor.SameColor(Eraser))
                {
                    PaletteList.Entries.Add(new PaletteEntry(Clone(CurrentColor)));
                    CurrentPaletteIndex = PaletteList.GetIndex(CurrentColor);//Set to last item in the list
                    AddPaletteButtonTexture(CurrentPaletteIndex);
                }
            }

            try
            {
                PaletteEntry entry = PaletteList.Entries[CurrentPaletteIndex];
                if (entry != null)
                {
                    //entry.Strokes.Add(new Vector2(MousePos.x, MousePos.y));
                    entry.AddEntry(new Vector2(MousePos.x, MousePos.y), BrushSize);
                }
                else
                {
                    Debug.Log("Null entry: " + CurrentPaletteIndex);
                    Debug.Log(PaletteList.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }

            DrawPixels(previewCoord);
            AddCoord(previewCoord, CurrentColor);
            CurrentImage.Apply();
        }

        if (NewPaletteColor.Changed)
        {
            try
            {
                UpdatePalette(CurrentPaletteIndex);
                CurrentImage.Apply();
                Repaint();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }
        }
        
        Repaint();
    }

    public void AddColorButtons(List<Color> colors)
    {
        if (ButtonTextures.Count() <= 0)
        {
            InitButtonTextures(Palette.DefaultColors);
        }
        GUILayout.BeginHorizontal();

        for (int i=0; i<colors.Count; i++)
        {
            Texture2D ButtonTexture = ButtonTextures.ElementAt(i);

            var pad = GUI.skin.button;
            GUIStyle style = new GUIStyle(pad);
            style.padding = new RectOffset(0, 0, 5, 5);
            
            if (GUILayout.Button(ButtonTexture, style, GUILayout.Width(25)))
            {
                if (Event.current.button == 0)
                {
                    CurrentColor = colors[i];
                }
            }
        }

        GUILayout.EndHorizontal();
    }

    public void AddPaletteButtonTexture(int index)
    {
        Color c = PaletteList.Entries.ElementAt(index).CurrentColor;
        Texture2D ButtonTexture = new Texture2D(15, 15);
        var colors_ = ButtonTexture.GetPixels();
        for (int j = 0; j < colors_.Length; j++)
        {
            colors_[j] = c;
        }
        ButtonTexture.SetPixels(colors_);
        ButtonTexture.Apply();

        PaletteButtonTextures.Add(ButtonTexture);
        //Debug.Log("PaletteButtonTexture Added: " + index + "->" + c);
    }

    public void AddPaletteTrackerButtons(List<Color> colors)
    {
        if (PaletteButtonTextures.Count() <= 0)
        {
            //Debug.Log("No Palette Button Textures");
            return;
        }

        GUILayout.BeginHorizontal();

        for (int i = 0; i < colors.Count; i++)
        {
            Texture2D ButtonTexture = PaletteButtonTextures.ElementAt(i);

            var pad = GUI.skin.button;
            GUIStyle style = new GUIStyle(pad);
            style.padding = new RectOffset(0, 0, 5, 5);
            
            if (GUILayout.Button(ButtonTexture, style, GUILayout.Width(25)))
            {
                if (Event.current.button == 0)
                {
                    CurrentColor = colors[i];
                }
                else
                {
                    //Change Color
                    NewPaletteColor = new ColorChange(colors[i], colors[i]);
                    ColorSelectionWindow.OpenColorSelectionWindow(Palette, NewPaletteColor);
                    CurrentPaletteIndex = i;              
                }
            }
        }

        GUILayout.EndHorizontal();
    }

    public void ClearValues()
    {
        Debug.Log(PaletteList.ToString());
        //Debug.Log(NewPaletteColor);
        NewPaletteColor = new ColorChange(CurrentColor, CurrentColor);
        ZoomSize = 4;
        //ColorPalette.Clear();
        //PaletteVerifier.Clear();
        CurrentColor = new Color(0, 0, 0, 1);
        //ColorPalette.Add(Clone(CurrentColor));
        ImageSize = 64;
        BrushSize = 4;
        UpdateImageSize(new Texture2D(ImageSize * ZoomSize, ImageSize * ZoomSize));
        //PaintPixels(0, 0, CurrentImage.height, CurrentImage.width, CurrentImage, Color.red);
        try
        {
            //Debug.Log("Painting...");
            //PaintPixels(CurrentImage, new Color(1,1,1,0));
            PaintPixels(CurrentImage, Eraser);
            Repaint();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        BrushRect = new Rect(0, 0, 1, 1);
        InitPaletteThread();
        UpdatingColor = false;
        PaletteColorChanged = false;

        //Debug.Log(PaletteList.ToString());

        Palette = new ColorPalette();
        PaletteList = new PaletteTracker();
        //PaletteList.Entries.Add(new PaletteEntry(CurrentColor));
        CurrentPaletteIndex = 0;

        PaletteButtonTextures = new List<Texture2D>();
        //AddPaletteButtonTexture(CurrentPaletteIndex);

        InitButtonTextures(Palette.DefaultColors);
        Scale = Vector2.one;
    }

    #region DrawPixels
    private void DrawPixels(Vector3 pos)
    {
        int x = 0;
        int y = (int)Mathf.Floor(CurrentImage.height - pos.y - 1);
        for (int i = 0; i < ZoomedBrushSize; i++)
        {
            x = (int)Mathf.Floor(pos.x + i);
            y = (int)Mathf.Floor(CurrentImage.height - pos.y - 1);

            if (x < CurrentImage.width && x >= 0 &&
                    y < CurrentImage.height && y >= 0)
            {
                CurrentImage.SetPixel(x, y, CurrentColor);
            }
            for (int j = 0; j < ZoomedBrushSize; j++)
            {
                y = (int)Mathf.Floor(CurrentImage.height - 1 - pos.y - j);

                if (x < CurrentImage.width && x >= 0 &&
                    y < CurrentImage.height && y >= 0)
                {
                    CurrentImage.SetPixel(x, y, CurrentColor);
                }
            }
        }
    }

    private void DrawPixels(Vector3 pos, Color color, int size)
    {
        int x = 0;
        int y = (int)Mathf.Floor(CurrentImage.height - pos.y - 1);
        for (int i = 0; i < size; i++)
        {
            x = (int)Mathf.Floor(pos.x + i);
            y = (int)Mathf.Floor(CurrentImage.height - pos.y - 1);

            if (x < CurrentImage.width && x >= 0 &&
                    y < CurrentImage.height && y >= 0)
            {
                CurrentImage.SetPixel(x, y, color);
            }
            for (int j = 0; j < size; j++)
            {
                y = (int)Mathf.Floor(CurrentImage.height - 1 - pos.y - j);

                if (x < CurrentImage.width && x >= 0 &&
                    y < CurrentImage.height && y >= 0)
                {
                    CurrentImage.SetPixel(x, y, color);
                }
            }
        }
    }

    private void DrawPixels(Vector3 pos, Texture2D target)
    {
        int x = 0;
        int y = 0;
        for (int i = 0; i <= ZoomedBrushSize; i++)
        {
            //x = (int)pos.x + i;
            //y = (int)target.height - (int)pos.y;
            x = (int)Mathf.Floor(pos.x + i);
            y = (int)Mathf.Floor(target.height - pos.y);

            if (x < target.width && x >= 0 &&
                    y < target.height && y >= 0)
            {
                target.SetPixel(x, y, CurrentColor);
            }
            //x = (int)Mathf.Ceil(pos.x + i);
            for (int j = 0; j <= ZoomedBrushSize; j++)
            {
                y = (int)target.height - (int)pos.y - j;
                y = (int)Mathf.Floor(target.height - pos.y - j);

                if (x < target.width && x >= 0 &&
                    y < target.height && y >= 0)
                {
                    target.SetPixel(x, y, CurrentColor);
                }
                
                //y = (int)Mathf.Ceil(ImageArea.height - pos.y - j);
            }
        }
    }
    #endregion DrawPixels

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

            if (!SnapToGrid)
            {
                var blockRemainderX = (x % BrushSize);
                var blockRemainderY = (y % BrushSize);

                newX = blockRemainderX <= BrushSize ? x - blockRemainderX : ((x - blockRemainderX) + BrushSize);
                newY = blockRemainderY <= BrushSize ? y - blockRemainderY : ((y - blockRemainderY) + BrushSize);
            }
            else
            {
                var xx =  x - ImageArea.position.x;
                var yy = y - ImageArea.position.y;

                var blockRemainderX = (xx % ZoomedBrushSize);
                var blockRemainderY = (yy % ZoomedBrushSize);

                newX = blockRemainderX <= ZoomedBrushSize ? x - blockRemainderX : ((x - blockRemainderX) + ZoomedBrushSize);
                newY = blockRemainderY <= ZoomedBrushSize ? y - blockRemainderY : ((y - blockRemainderY) + ZoomedBrushSize);
            }
        }
        else
        {
            if (!SnapToGrid)
            {
                //coords.y = coords.y - heightGrowthBuffer;

                var blockRemainderX = (coords.x % BrushSize);
                var blockRemainderY = (coords.y % BrushSize);

                newX = blockRemainderX <= BrushSize ? Mathf.Clamp(coords.x - blockRemainderX, 0, w) : Mathf.Clamp((coords.x - blockRemainderX) + BrushSize, 0, w);
                newY = blockRemainderY <= BrushSize ? Mathf.Clamp(coords.y - blockRemainderY, 0, h) : Mathf.Clamp((coords.y - blockRemainderY) + BrushSize, 0, h);
            }
            else
            {
                var blockRemainderX = (coords.x % ZoomedBrushSize);
                var blockRemainderY = (coords.y % ZoomedBrushSize);

                newX = blockRemainderX <= ZoomedBrushSize ? Mathf.Clamp(coords.x - blockRemainderX, 0, w) : Mathf.Clamp((coords.x - blockRemainderX) + ZoomedBrushSize, 0, w);
                newY = blockRemainderY <= ZoomedBrushSize ? Mathf.Clamp(coords.y - blockRemainderY, 0, h) : Mathf.Clamp((coords.y - blockRemainderY) + ZoomedBrushSize, 0, h);
            }
        }

        return new Vector2(newX, newY);
    }

    private Vector2 FindCoord(Vector3 coords, bool global = false)
    {
        return FindCoord(new Vector2(coords.x, coords.y), global);
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

    private void InitButtonTextures(List<Color> colors)
    {
        ButtonTextures = new List<Texture2D>();
        foreach (var c in colors)
        {
            Texture2D ButtonTexture = new Texture2D(15, 15);
            var colors_ = ButtonTexture.GetPixels();
            for (int j = 0; j < colors_.Length; j++)
            {
                colors_[j] = c;
            }
            ButtonTexture.SetPixels(colors_);
            ButtonTexture.Apply();

            ButtonTextures.Add(ButtonTexture);
        }
    }

    private void PaintPixels(Texture2D image, Color color)
    {
        //int x = 0;
        //int y = 0;
        for (int i = 0; i < image.width; i++)
        {
            //x = (int)Mathf.Ceil(pos.x + i);
            //y = (int)Mathf.Ceil(image.height - pos.y);

            //if (x < image.height && x >= 0 &&
            //        y < image.width && y >= 0)
            //{
            //    image.SetPixel(x, y, color);
            //}

            for (int j = 0; j < image.height; j++)
            {
                //y = (int)Mathf.Ceil(image.height - pos.y - j);

                if (i < image.height && i >= 0 &&
                    j < image.width && j >= 0)
                {
                    image.SetPixel(i, j, color);
                }
            }
        }
        image.Apply();
    }

    //Not in use
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

    //http://wiki.unity3d.com/index.php?title=TextureScale
    private void SaveImage()
    {
        //EditorUtility.DisplayDialogComplex("Select a Folder", "Choose the save location for your image...", "OK", "Cancel", "Alt");
        var imagePath = EditorUtility.SaveFilePanel("Select a File", "C", "testImage", "png");
        Debug.Log(imagePath);

        //shrink to real image size
        Color[] oldPixels = CurrentImage.GetPixels();
        Color[] newPixels = new Color[oldPixels.Length / ZoomSize];

        //int j = 0;
        //for (int i = 0; i < oldPixels.Length; i += ZoomSize)
        //{
        //    newPixels[j++] = oldPixels[i];
        //}

        //Texture2D newImage = new Texture2D(CurrentImage.width / ZoomSize, CurrentImage.height / ZoomSize);
        //newImage.SetPixels(newPixels);

        Texture2D newImage = new Texture2D(CurrentImage.width, CurrentImage.height);
        newImage.SetPixels(oldPixels);
        //TextureScale.Bilinear(newImage, CurrentImage.width / ZoomSize, CurrentImage.height / ZoomSize);
        TextureScale.Point(newImage, CurrentImage.width / ZoomSize, CurrentImage.height / ZoomSize);

        //Texture2D newImage = CurrentImage;
        //newImage.Resize(newImage.width / ZoomSize, newImage.height / ZoomSize);

        var png = newImage.EncodeToPNG();

        File.WriteAllBytes(imagePath, png);
        System.Diagnostics.Process.Start(imagePath);        
    }

    private void UpdatePalette(int index)
    {
        //PaletteEntry p = PaletteList.Entries.ElementAt(index);
        //Debug.Log(p);
        //Debug.Log("Stroke Count: " + p.Strokes.Count);
        //foreach (var pos in p.Strokes)
        for (int j=0; j<PaletteList.Entries.Count; j++)
        {
            PaletteEntry p = PaletteList.Entries.ElementAt(j);
            for (int i = 0; i < p.Strokes.Count; i++)
            {
                var pos = p.Strokes[i];
                Vector3 adj = new Vector2(pos.x - this.position.x - ImageArea.position.x,
                                 pos.y - this.position.y - ImageArea.position.y);

                //Used to calculate the square to draw on the image
                Vector2 previewCoord = FindCoord(adj);
                //Debug.Log(previewCoord + " | " + pos);
                DrawPixels(previewCoord, j==index ? NewPaletteColor.NewColor : p.CurrentColor, p.BrushSizes[i] * ZoomSize);
            }
        }

        //Puts color on top of the image
        //for(int i=0; i < p.Strokes.Count; i++)
        //{
        //    var pos = p.Strokes[i];
        //    Vector3 adj = new Vector2(pos.x - this.position.x - ImageArea.position.x,
        //                     pos.y - this.position.y - ImageArea.position.y);

        //    //Used to calculate the square to draw on the image
        //    Vector2 previewCoord = FindCoord(adj);
        //    //Debug.Log(previewCoord + " | " + pos);
        //    DrawPixels(previewCoord, NewPaletteColor.NewColor, p.BrushSizes[i] * ZoomSize);            
        //}

        PaletteList.Entries.ElementAt(index).CurrentColor = Clone(NewPaletteColor.NewColor);
        UpdatePaletteButtonTexture(index);
        NewPaletteColor = new ColorChange(CurrentColor, CurrentColor);
    }

    //not in use
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

    public void UpdatePaletteButtonTexture(int index)
    {
        Color c = PaletteList.Entries.ElementAt(index).CurrentColor;
        Texture2D ButtonTexture = PaletteButtonTextures.ElementAt(index);
        var colors_ = ButtonTexture.GetPixels();
        for (int j = 0; j < colors_.Length; j++)
        {
            colors_[j] = c;
        }
        ButtonTexture.SetPixels(colors_);
        ButtonTexture.Apply();
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
            //var pixels = CurrentImage.GetPixels();
            //for (int i = 0; i < pixels.Length; i++)
            //{
            //    pixels[i] = Color.clear;
            //}
            //CurrentImage.SetPixels(pixels);
            //CurrentImage.Apply();            
        }
        ImageArea = new Rect(50, positionBuffer + heightGrowthBuffer, CurrentImage.height, CurrentImage.width);
        //PaintPixels(CurrentImage, new Color(1, 1, 1, 0));
        PaintPixels(CurrentImage, Eraser);
    }

    private void UpdateImageSize(Texture2D texture)
    {
        CurrentImage = texture;
        ImageArea = new Rect(50, positionBuffer + heightGrowthBuffer, CurrentImage.height, CurrentImage.width);
    }
    #endregion UpdateImageSize

    // ========================================================================================
    // Helper Methods
    // ========================================================================================    
    public Color Clone(Color color)
    {
        return new Color(color.r, color.g, color.b, color.a);
    }

    public void AddCoord(Vector2 pos, Color color)
    {
        try
        {
            ImageData[(int)pos.x][(int)pos.y] = Clone(color);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
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
    public bool Changed = false;

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

public class ColorPalette
{
    public List<Color> DefaultColors;
    public List<Color> CustomPalette;

    public ColorPalette()
    {
        DefaultColors = new List<Color>();
        CustomPalette = new List<Color>();

        //Defaults
        DefaultColors.Add(Color.white);
        DefaultColors.Add(Color.black);
        DefaultColors.Add(Color.gray);
        DefaultColors.Add(Color.blue);
        DefaultColors.Add(new Color(135/255f, 206 / 255f, 235/255f, 1));//135,206,235 sky blue
        DefaultColors.Add(Color.cyan);        
        DefaultColors.Add(Color.green);
        DefaultColors.Add(new Color(0, 128 / 255f, 0, 1));//green
        DefaultColors.Add(new Color(0, 100 / 255f, 0, 1));//dark green
        DefaultColors.Add(new Color(139 / 255f, 69 / 255f, 19 / 255f, 1));//brown
        DefaultColors.Add(Color.magenta);
        DefaultColors.Add(Color.red);
        DefaultColors.Add(new Color(255/255f, 165/255f, 0, 1));//orange
        DefaultColors.Add(Color.yellow);
    }

    public ColorPalette(List<Color> defaultColors)
    {
        DefaultColors = defaultColors;
        CustomPalette = new List<Color>();
    }

    public ColorPalette(List<Color> defaultColors, List<Color> customPalette)
    {
        DefaultColors = defaultColors;
        CustomPalette = customPalette;
    }
}

public class PaletteEntry
{
    public Color CurrentColor;
    public List<Vector2> Strokes;
    public List<int> BrushSizes;

    #region PaletteEntry
    public PaletteEntry()
    {
        CurrentColor = Color.black;
        Strokes = new List<Vector2>();
        BrushSizes = new List<int>();
    }

    public PaletteEntry(Color color)
    {
        CurrentColor = color;
        Strokes = new List<Vector2>();
        BrushSizes = new List<int>();
    }

    public PaletteEntry(Color color, List<Vector2> positions)
    {
        CurrentColor = color;
        Strokes = positions;
        BrushSizes = new List<int>();
        foreach (var s in Strokes)
        {
            BrushSizes.Add(1);
        }
    }

    public PaletteEntry(List<Vector2> positions)
    {
        CurrentColor = Color.black;
        Strokes = positions;
        BrushSizes = new List<int>();
        foreach (var s in Strokes)
        {
            BrushSizes.Add(1);
        }
    }
    #endregion PaletteEntry

    public void AddEntry(Vector2 stroke, int size)
    {
        Strokes.Add(stroke);
        BrushSizes.Add(size);
    }

    public override string ToString()
    {
        string temp = "";

        //temp += "index: " + PaletteIndex;
        temp += "color: " + CurrentColor.ToString();
        temp += " -> " + Environment.NewLine;

        foreach (var s in Strokes)
        {
            temp += "  " + s.ToString();
        }

        temp += Environment.NewLine;

        return temp;
    }
}

public class PaletteTracker : Collection<PaletteEntry>
{
    public List<PaletteEntry> Entries;

    public PaletteTracker()
    {
        Entries = new List<PaletteEntry>();
    }

    public bool Contains(Color color)
    {
        foreach (PaletteEntry p in Entries)
        {
            if (p.CurrentColor.SameColor(color))
            {
                return true;
            }
        }
        return false;
    }

    public List<Color> GetColors()
    {
        List<Color> colors = new List<Color>();
        foreach (var e in Entries)
        {
            colors.Add(e.CurrentColor);
        }
        return colors;
    }

    public int GetIndex(Color color)
    {
        int index = -1;
        if (Entries.Count > 0)
        {
            for (int i=0; i < Entries.Count; i++)
            {
                Color c = Entries[i].CurrentColor;
                if (color.SameColor(c))
                {
                    index = i;
                    break;
                }
            }
        }

        return index;
    }

    public override string ToString()
    {
        string temp = "";

        foreach (var e in Entries)
        {
            temp += "Entry (" + Entries.IndexOf(e) + ")-> ";
            temp += e.ToString();
        }

        return temp;
    }
}

// ================================================================
// Extensions
// ================================================================
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
        //if (Mathf.Round(c1.r) == Mathf.Round(c2.r) &&
        //    Mathf.Round(c1.g) == Mathf.Round(c2.g) &&
        //    Mathf.Round(c1.b) == Mathf.Round(c2.b) &&
        //    Mathf.Round(c1.a) == Mathf.Round(c2.a) )
        if (c1.r / 255f == c2.r / 255f &&
            c1.g / 255f == c2.g / 255f &&
            c1.b / 255f == c2.b / 255f &&
            c1.a / 255f == c2.a / 255f)

        //if (c1.r * 255 == c2.r * 255 &&
        //    c1.g * 255 == c2.g * 255 &&
        //    c1.b * 255 == c2.b * 255 &&
        //    c1.a * 255 == c2.a * 255 )
        {
            return true;
        }
        return false;
    }
}