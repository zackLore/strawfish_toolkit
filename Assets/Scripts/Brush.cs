using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brush : MonoBehaviour {

    public Vector2 BrushSize = Vector2.zero;
    public Color CurrentColor = Color.white;
    public SpriteRenderer CurrentSpriteRenderer = new SpriteRenderer();

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = CurrentColor;//Should be the color of the selected object
        Gizmos.DrawWireCube(transform.position, BrushSize);
        Debug.Log("Drawing Brush Cube");
    }

    public void Start()
    {
        CurrentSpriteRenderer.sprite = new Sprite();
    }

    public void UpdateBrush(Sprite sprite)
    {
        CurrentSpriteRenderer.sprite = sprite;
    }
}
