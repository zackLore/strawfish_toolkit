using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ColorSelectionWindow : EditorWindow {
    public static ColorChange ColorSet;
    public static ColorPalette Palette;
    
    public List<Texture2D> ButtonTextures;

    public static void OpenColorSelectionWindow(ColorPalette palette, ColorChange colorSet)
    {
        var window = EditorWindow.GetWindow(typeof(ColorSelectionWindow));
        var title = new GUIContent("Color Selection");
        window.titleContent = title;

        ColorSet = colorSet;
        Palette = palette;

        int size = 0;
        foreach (var p in Palette.DefaultColors)
        {
            size += 40;
        }
        window.maxSize = new Vector2(size + 20, 5);
    }

    private void OnEnable()
    {
        ButtonTextures = new List<Texture2D>();
    }

    void OnGUI()
    {
        AddColorButtons(Palette.DefaultColors);
        if (ColorSet.Changed)
        {
            Close();
        }
    }

    void OnDestroy()
    {
    }

    public void AddColorButtons(List<Color> colors)
    {
        if (ButtonTextures.Count <= 0)
        {
            InitButtonTextures(Palette.DefaultColors);
        }
        
        GUIStyle hStyle = new GUIStyle();
        hStyle.margin = new RectOffset(5, 5, 25, 10);
        GUILayout.BeginHorizontal(hStyle);

        for (int i = 0; i < colors.Count; i++)
        {
            Texture2D ButtonTexture = ButtonTextures[i];

            var pad = GUI.skin.button;
            GUIStyle style = new GUIStyle(pad);
            style.padding = new RectOffset(0, 0, 5, 5);

            if (GUILayout.Button(ButtonTexture, style, GUILayout.Width(25)))
            {
                if (Event.current.button == 0)
                {
                    ColorSet.NewColor = colors[i];
                    ColorSet.Changed = true;
                }
            }
        }

        GUILayout.EndHorizontal();
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
}
