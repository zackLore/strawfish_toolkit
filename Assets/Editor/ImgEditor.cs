using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ImageEditor))]
public class ImgEditor : Editor {

    public ImageEditor ie;

    public override void OnInspectorGUI()
    {
        //Size Selection   
        int temp = EditorGUILayout.IntField("Image Size", ie.ImageSize);
        ie.ImageSize = temp % 4 == 0 ? temp : 0;
        if (ie.ImageSize == 0)
        {
            EditorGUILayout.HelpBox("Must be divisible by 4.",MessageType.Info);
        }

        //Brush Section
        ie.BrushSize = EditorGUILayout.IntSlider("Brush Size", ie.BrushSize, 1, ie.ImageSize, null);

        //Color Section
        EditorGUILayout.LabelField("Color Selection");       

        EditorGUILayout.BeginVertical();

        ie.CurrentColor = EditorGUILayout.ColorField(ie.CurrentColor, null);

        //ie.CurrentTexture = EditorGUILayout.ObjectField("Texture: ", ie.CurrentTexture, typeof(Texture2D), false) as Texture2D;
        //if (ie.CurrentTexture == null)
        //{
        //    EditorGUILayout.HelpBox("Please select a texture.", MessageType.Warning);
        //}

        EditorGUILayout.EndVertical();
    }

    public void OnEnable()
    {
        ie = target as ImageEditor;
        Tools.current = Tool.View;//Re-enables the tool view

        //Camera.main.ScreenToWorldPoint(ie.transform.position);
        ie.NewBrush();
    }

    //public void OnDisable()
    //{
    //    ie.DestroyBrush();
    //}
}
