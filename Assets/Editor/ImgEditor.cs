using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ImageEditor))]
public class ImgEditor : Editor {

    public ImageEditor ie;
    public Brush brush;

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

    //https://forum.unity3d.com/threads/how-to-get-mouseposition-in-scene-view.208911/
    private void OnSceneGUI()
    {
        if (ie != null)
        {
            if (brush == null)
            {
                brush = ie.GetComponentInChildren<Brush>();
            }

            if (brush != null)
            {
                var pos = Event.current.mousePosition;
                pos.y = SceneView.currentDrawingSceneView.camera.pixelHeight - pos.y;
                pos = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(pos);
                pos.y = -pos.y;

                if (pos != null)
                {
                    brush.MousePos = pos;
                    //brush.OnDrawGizmos();
                }
            }
        }
    }

    public void OnEnable()
    {
        ie = target as ImageEditor;
        Tools.current = Tool.View;//Re-enables the tool view

        FocusCamera();

        ie.NewBrush();
    }

    public void FocusCamera()
    {
        Vector3 pos = ie.transform.position;
        pos.x = pos.x + (ie.ImageSize / 2);
        pos.y = pos.y + (ie.ImageSize / 2);
        
        SceneView.lastActiveSceneView.pivot = pos;
        SceneView.lastActiveSceneView.Repaint();        
    }

    //public void OnDisable()
    //{
    //    ie.DestroyBrush();
    //}
}
