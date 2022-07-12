using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MandelbrotCPU : MonoBehaviour
{
    public int iterations = 50;
    [Range(0,3000)]
    public int width = 500;
    public float zoomFactor = 0.1f;
    public Vector2 destination = Vector2.zero;
    ImageBounds bounds;
    Texture2D texture;
    Material material;
    double[,] image;
    int height;
    Vector2 _dest;
    bool run = false;
    float x, y;

    void Calculate()
    {
        double[] reals = MandelbrotHelper.Linspace(bounds.xMin, bounds.xMax, width);
        double[] imags = MandelbrotHelper.Linspace(bounds.yMin, bounds.yMax, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float iter = IterCalculation(reals[i], imags[j]);
                float scaledIteration = iter / iterations;
                float clampedIteration = (float)Math.Max(0.0, Math.Min(scaledIteration, 1.0));
                image[i, j] = clampedIteration;
            }
        }
    }
    
    float IterCalculation(double real, double imag)
    {
        float iter = 0;
        double z_real = 0.0;
        double z_imag = 0.0;
        while (iter < iterations)
        {
            double realSqr = z_real * z_real;
            double imagSqr = z_imag * z_imag;
            if (realSqr + imagSqr > 4)
            {
                float magnitude = (float)Math.Sqrt(realSqr + imagSqr);
                return (float)(iter + 1 - Math.Log(Math.Log(magnitude)) / Math.Log(2));
            }

            //squaring z
            z_imag = 2 * z_real * z_imag;
            z_real = realSqr - imagSqr;

            //adding c
            z_real += real;
            z_imag += imag;

            iter++;
        }
        return iter;
    }

    void InitImageValues()
    {
        //the bounding box starts as a square which might not be the right format
        //so scale the y axis accordingly to avoid stretching
        bounds = new ImageBounds(-2, 2, -2, 2);
        float scale = y / x;
        bounds.yMin *= scale;
        bounds.yMax *= scale;
        height = (int)(width * scale);
        image = new double[width, height];

        _dest = destination;

    }
    // Start is called before the first frame update
    void Start()
    {
        x = transform.localScale.x;
        y = transform.localScale.y;
        
        InitImageValues();

        material = GetComponent<MeshRenderer>().material;
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        material.mainTexture = texture;

        Calculate();

        texture.SetPixels(MandelbrotHelper.FlatImg(width, height, image));
        texture.Apply();
    }

    void UpdateBoundaries()
    {
        double xMinDist = Math.Abs(bounds.xMin - _dest.x);
        double xMaxDist = Math.Abs(_dest.x - bounds.xMax);
        double yMinDist = Math.Abs(bounds.yMin - _dest.y);
        double yMaxDist = Math.Abs(_dest.y - bounds.yMax);

        double xMinDistSc = zoomFactor * xMinDist;
        double xMaxDistSc = zoomFactor * xMaxDist;
        double yMinDistSc = zoomFactor * yMinDist;
        double yMaxDistSc = zoomFactor * yMaxDist;

        bounds.xMin += xMinDistSc;
        bounds.xMax -= xMaxDistSc;
        bounds.yMin += yMinDistSc;
        bounds.yMax -= yMaxDistSc;
    }

    // Update is called once per frame
    void Update()
    {
        if (!run)
        {
            return;
        }

        UpdateBoundaries();
        Calculate();

        texture.SetPixels(MandelbrotHelper.FlatImg(width, height, image));
        texture.Apply();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
        Gizmos.DrawSphere(new Vector3(destination.x, destination.y, 0), 0.01f);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "Run"))
        {
            run = true;
        }
        if (GUI.Button(new Rect(100, 0, 100, 50), "Stop"))
        {
            run = false;
        }
        if (GUI.Button(new Rect(400, 0, 100, 50), "Reset"))
        {
            run = false;
            InitImageValues();
            Calculate();
            texture.SetPixels(MandelbrotHelper.FlatImg(width, height, image));
            texture.Apply();
        }
    }
}
