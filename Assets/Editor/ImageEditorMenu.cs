using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//More info here: https://docs.unity3d.com/ScriptReference/MenuItem.html
public class ImageEditorMenu {

    [MenuItem("Image/ImageEditor _%&i")]
    public static void CreateImageEditor()
    {
        GameObject go = new GameObject("Image Editor");
        go.AddComponent<ImageEditor>();
    }
}
