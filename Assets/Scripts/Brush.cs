using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Brush : MonoBehaviour {

    public Vector2 BrushSize = Vector2.zero;
    public Color CurrentColor = Color.white;
    public SpriteRenderer CurrentSpriteRenderer = new SpriteRenderer();

    public Vector3 MousePos;

    public void OnDrawGizmosSelected()
    {
        //DrawBrushOutline();
    }

    public void OnDrawGizmos()
    {
        //DrawBrushOutline();
    } 
    //private void OnSceneGUI()
    //{
    //    //mosPos = Input.mousePosition;
    //    MousePos = Event.current.mousePosition;
    //}

    public void DrawBrushOutline()
    {
        //Gizmos.color = CurrentColor;//Should be the color of the selected object
        //Gizmos.DrawWireCube(MousePos, BrushSize);
        //Debug.Log("Drawing Brush Cube: " + MousePos);
    }

    public void UpdateBrush(Sprite sprite)
    {
        CurrentSpriteRenderer.sprite = sprite;
    }
}
