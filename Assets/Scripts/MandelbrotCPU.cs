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
    public decimal realDest = 0;
    public decimal imagDest = 0;

    Point destination;
    ImageBounds bounds;
    Texture2D texture;
    Material material;
    double[,] image;
    int height;
    bool run = false;
    float x, y;

    void Calculate()
    {
        decimal[] reals = MandelbrotHelper.Linspace(bounds.xMin, bounds.xMax, width);
        decimal[] imags = MandelbrotHelper.Linspace(bounds.yMin, bounds.yMax, height);

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
    
    int IterCalculation(decimal real, decimal imag)
    {
        int iter = 0;
        decimal z_real = 0;
        decimal z_imag = 0;
        while (iter < iterations)
        {
            decimal realSqr = z_real * z_real;
            decimal imagSqr = z_imag * z_imag;
            if (realSqr + imagSqr > 4)
            {
                break;
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
        decimal scale = (decimal)(y / x);
        bounds.yMin *= scale;
        bounds.yMax *= scale;
        height = (int)(width * scale);
        image = new double[width, height];

        destination.real = realDest;
        destination.imag = imagDest;

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
        decimal t = (decimal)(zoomFactor * Time.deltaTime);
        decimal xMinDist = Math.Abs(bounds.xMin - destination.real);
        decimal xMaxDist = Math.Abs(destination.real - bounds.xMax);
        decimal yMinDist = Math.Abs(bounds.yMin - destination.imag);
        decimal yMaxDist = Math.Abs(destination.imag - bounds.yMax);

        decimal xMinDistSc = t * xMinDist;
        decimal xMaxDistSc = t * xMaxDist;
        decimal yMinDistSc = t * yMinDist;
        decimal yMaxDistSc = t * yMaxDist;

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
