using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ImageEditorWindow : EditorWindow {

    [MenuItem("Window/ImageEditorWindow")]
    public static void OpenImageEditorWindow()
    {
        var window = EditorWindow.GetWindow(typeof(ImageEditorWindow));
        var title = new GUIContent("Image Editor");
        window.titleContent = title;
    }

    private void OnGUI()
    {
        if (Selection.activeGameObject == null) { return; }

        var selection = Selection.activeGameObject.GetComponent<ImageEditor>();
        if (selection != null)
        {

        }
    }
}
