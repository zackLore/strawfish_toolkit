using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class PreviewWindow : EditorWindow {
    public static PaletteTracker PreviewTracker_;
    public static Stroke[,] ImageData_;
    public static bool RefreshNeeded;

    public Texture2D CurrentPreviewImage;
    public Rect ImageArea;
    public Stroke[,] ImageData;
    public PaletteTracker PreviewTracker;
    public List<Texture2D> PaletteButtonTextures;

    public int PreviewZoom = 1;
    public bool ShowPalette = false;

    private bool initialized = false;

    [MenuItem("Window/ Open Preview _%&#p")]
    public static void OpenPreviewWindow()
    {
        var window = EditorWindow.GetWindow(typeof(PreviewWindow));
        var title = new GUIContent("Preview");
        window.titleContent = title;

        try
        {
            //var imageEditor = GetWindow(typeof(ImageEditorWindow)) as ImageEditorWindow;
            //if (imageEditor != null)
            //{
            //    PreviewTracker_ = imageEditor.PaletteList;
            //    ImageData_ = imageEditor.ImageData;
            //}
            GetData();
        }
        catch (Exception ex)
        {
            Debug.Log("Open Failed: " + ex.ToString());
        }
    }

    public void InitPreview()
    {
        try
        {
            if (ImageData_ == null || PreviewTracker_ == null || RefreshNeeded)
            {
                GetData();
            }

            PreviewZoom = 4;
            ImageData = ImageData_;
            PaletteTracker newTracker = new PaletteTracker();
            newTracker.Entries = PreviewTracker_.Entries;
            PreviewTracker = newTracker;

            if (PreviewTracker.Entries.Count > 0)
            {
                for (int i = 0; i < PreviewTracker.Entries.Count; i++)
                {
                    AddPaletteButtonTexture(i);
                }
            }

            CurrentPreviewImage = Zoom(PreviewZoom);
            ImageArea = new Rect(50, 50, ImageData.GetLength(0) * PreviewZoom, ImageData.GetLength(0) * PreviewZoom);
            if (ImageData != null && PreviewTracker != null)
            {
                initialized = true;
            }
            RefreshNeeded = false;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
    }

    private void OnEnable()
    {        
        InitPreview();
    }

    private void OnGUI()
    {
        try
        {
            if (!initialized || RefreshNeeded) { Debug.Log("initialized: " + initialized + " RefreshNeede: " + RefreshNeeded + " -> " + DateTime.Now.TimeOfDay); InitPreview(); return; }
            EditorGUILayout.BeginHorizontal();

            //Colors
            EditorGUILayout.BeginVertical();

            //ShowPalette = EditorGUILayout.Foldout(ShowPalette, "Palette Tracker (" + PreviewTracker.Entries.Count + ")", true);
            ShowPalette = EditorGUILayout.Foldout(ShowPalette, "Palette Tracker", true);
            if (ShowPalette)
            {
                List<Color> colors = PreviewTracker.GetColors();
                if (colors.Count > 0 && PaletteButtonTextures.Count > 0)
                {
                    for (int i = 0; i < colors.Count; i++)
                    {
                        Texture2D ButtonTexture = PaletteButtonTextures[i];

                        var pad = GUI.skin.button;
                        GUIStyle style = new GUIStyle(pad);
                        style.padding = new RectOffset(0, 0, 5, 5);

                        if (GUILayout.Button(ButtonTexture, style, GUILayout.Width(25)))
                        {
                            //if (Event.current.button == 0)
                            //{
                            //    CurrentColor = colors[i];
                            //}
                            //else
                            //{
                            //    //Change Color
                            //    NewPaletteColor = new ColorChange(colors[i], colors[i]);
                            //    ColorSelectionWindow.OpenColorSelectionWindow(Palette, NewPaletteColor);
                            //    CurrentPaletteIndex = i;
                            //}
                        }
                    }
                }
            }

            EditorGUILayout.BeginVertical();

            //Image
            EditorGUI.DrawPreviewTexture(ImageArea, CurrentPreviewImage);

            EditorGUILayout.EndHorizontal();
            Repaint();
        }
        catch (UnityException uex)
        {
            Debug.Log(uex.ToString());
        }
        catch (Exception ex)
        {
            Debug.Log("OnGUI failed: " + ex.ToString());
        }
    }

    public void AddPaletteButtonTexture(int index)
    {
        Color c = PreviewTracker.Entries[index].CurrentColor;
        Texture2D ButtonTexture = new Texture2D(15, 15);
        var colors_ = ButtonTexture.GetPixels();
        for (int j = 0; j < colors_.Length; j++)
        {
            colors_[j] = c;
        }
        ButtonTexture.SetPixels(colors_);
        ButtonTexture.Apply();

        PaletteButtonTextures.Add(ButtonTexture);
    }

    private Texture2D Zoom(int zoomSize)//The y axis is off
    {
        //Get unzoomedSize
        int size = ImageData.GetLength(0);

        //Update size with zoom
        size = zoomSize * size;

        //Create new Image at correct size
        Texture2D zoomedImage = new Texture2D(size, size);
        //Fill new image with correct colors
        int tempX = 0;
        int tempY = 0;
        for (int x = 0; x < size; x += zoomSize)
        {
            for (int y = 0; y < size; y += zoomSize)
            {
                Stroke s = ImageData[x / zoomSize, y / zoomSize];
                for (int i = 0; i < zoomSize; i++)
                {
                    tempX = x + i;
                    for (int j = 0; j < zoomSize; j++)
                    {
                        tempY = y + j;
                        tempY = (zoomedImage.height - 1) - tempY;
                        zoomedImage.SetPixel(tempX, tempY, s.CurrentColor);
                    }
                }
            }
        }
        zoomedImage.Apply();
        return zoomedImage;
    }
    
    public static void GetData()
    {
        try
        {
            var imageEditor = GetWindow(typeof(ImageEditorWindow)) as ImageEditorWindow;
            if (imageEditor != null)
            {
                PreviewTracker_ = imageEditor.PaletteList;
                ImageData_ = imageEditor.ImageData;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("GetData failed: " + ex.ToString());
        }
    }
}
