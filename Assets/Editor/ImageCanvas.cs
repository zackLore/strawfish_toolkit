using UnityEngine;

public class ImageCanvas
{
    public int Height;
    public int Width;
    public Vector3 Position;
    public float XBuffer;
    public float YBuffer;

    private float b_;
    public float B
    {
        get
        {
            return YBuffer + Height;
        }
    }

    private float l_;
    public float L
    {
        get
        {
            return XBuffer;
        }
    }

    private float r_;
    public float R
    {
        get
        {
            return XBuffer + Width;
        }
    }

    private float t_;
    public float T
    {
        get
        {
            return YBuffer;
        }
    }

    public ImageCanvas()
    {
    }

    public ImageCanvas(int height, int width, Vector3 position, float xBuffer, float yBuffer)
    {
        Height = height;
        Width = width;
        Position = position;
        XBuffer = xBuffer;
        YBuffer = yBuffer;
    }
}
