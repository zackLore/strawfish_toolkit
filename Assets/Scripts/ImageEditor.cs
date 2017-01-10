using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageEditor : MonoBehaviour {

    private Color _currentColor;
    public Color CurrentColor
    {
        get
        {
            return _currentColor;
        }
        set
        {
            if (_currentColor != value)
            {
                _currentColor = value;
                if (brush != null)
                {
                    brush.CurrentColor = CurrentColor;
                }
            }
        }
    }

    private int _imageSize;
    public int ImageSize
    {
        get
        {
            return _imageSize;
        }
        set
        {
            if (_imageSize != value)
            {
                _imageSize = value;
                if (_imageSize > 0 && _imageSize % 4 == 0)
                {
                    Debug.Log("New Array Created");
                    InitializeBitmapArray();
                }
            }
        }
    }

    private int _brushSize;
    public int BrushSize
    {
        get
        {
            return _brushSize;
        }
        set
        {
            if (_brushSize != value)
            {
                _brushSize = value;
                if (brush != null)
                {
                    brush.BrushSize = new Vector2(BrushSize, BrushSize);
                }
            }
        }
    }

    public Texture2D[][] BitmapArray;
    public Texture2D CurrentPixel;

    public Brush brush { get; set; }

	// Use this for initialization
	void Start () {        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnDrawGizmosSelected()
    {
        //if (ImageSize == 0) { return; }
        //var pos = transform.position;

        ////Draw Grid
        //Gizmos.color = Color.gray;
        ////int max = ImageSize * ImageSize;

        //for (int i = 0; i < ImageSize; i++)
        //{
        //    for (int j = 0; j < ImageSize; j++)
        //    {
        //        Gizmos.DrawWireCube(new Vector3(i + .5f, j + .5f), new Vector3(1, 1));
        //    }
        //}

        ////Draw Border
        //Gizmos.color = Color.white;
        //var centerX = pos.x + (ImageSize / 2);
        //var centerY = pos.y + (ImageSize / 2);

        //Gizmos.DrawWireCube(new Vector2(centerX, centerY), new Vector3(ImageSize, ImageSize));
    }

    public void CreateBrush()
    {
        GameObject go = new GameObject("Brush");
        go.transform.SetParent(this.transform);

        brush = go.AddComponent<Brush>();
        brush.BrushSize = new Vector2(BrushSize, BrushSize);
        brush.CurrentColor = CurrentColor;

        
    }

    public void DestroyBrush()
    {
        if (brush != null)
        {
            DestroyImmediate(brush.gameObject);
        }
    }

    public void NewBrush()
    {
        if (brush == null)
        {
            CreateBrush();
        }
    }
    
    public void InitializeBitmapArray()
    {
        BitmapArray = new Texture2D[ImageSize][];
        for (int i = 0; i < BitmapArray.Length; i++)
        {
            BitmapArray[i] = new Texture2D[ImageSize];
        }
    }
}
