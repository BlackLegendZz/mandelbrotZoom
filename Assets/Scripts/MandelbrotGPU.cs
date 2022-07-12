using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MandelbrotGPU : MonoBehaviour
{
    public ComputeShader computeShader;
    public int iterations = 50;
    [Range(0,3000)]
    public int width = 500;
    [Range(4, 10000)]
    public int calculationLimit = 4;
    public float zoomFactor = 0.1f;
    public double realDest = 0;
    public double imagDest = 0;
    public Gradient gradient;
    
    ComputeBuffer computeBufferReal;
    ComputeBuffer computeBufferImag;
    ComputeBuffer computeBufferColours;
    Point destination;
    ImageBounds bounds;
    int height;
    bool run = false;
    float x, y;
    RenderTexture displayTexture;
    Vector3[] gradientColours;
    int d_size;
    int v3_size;

    void Calculate()
    {
        gradientColours = new Vector3[iterations];
        for (int i = 0; i < iterations; i++)
        {
            Color c = gradient.Evaluate(i / (float)iterations);
            Vector3 v = new Vector3(c.r, c.g, c.b);
            gradientColours[i] = v;
        }

        double[] reals = MandelbrotHelper.Linspace(bounds.xMin, bounds.xMax, width);
        double[] imags = MandelbrotHelper.Linspace(bounds.yMin, bounds.yMax, height);

        computeBufferReal = new ComputeBuffer(width, d_size);
        computeBufferImag = new ComputeBuffer(height, d_size);
        computeBufferColours = new ComputeBuffer(iterations, v3_size);
        computeBufferReal.SetData(reals);
        computeBufferImag.SetData(imags);
        computeBufferColours.SetData(gradientColours);

        computeShader.SetBuffer(0, "reals", computeBufferReal);
        computeShader.SetBuffer(0, "imags", computeBufferImag);
        computeShader.SetBuffer(0, "colours", computeBufferColours);
        computeShader.SetTexture(0, "displayTexture", displayTexture);
        computeShader.SetFloat("iterations", iterations);
        computeShader.SetInt("calculationLimit", calculationLimit);
        computeShader.Dispatch(0, width / 8, height / 8, 1);

        computeBufferReal.Dispose();
        computeBufferImag.Dispose();
        computeBufferColours.Dispose();
    }

    void InitImageValues()
    {
        //the bounding box starts as a square which might not be the right format
        //so scale the y axis accordingly to avoid stretching
        bounds = new ImageBounds(-2+destination.real, 2+destination.real, -2+destination.imag, 2+destination.imag);
        float scale = y / x;
        bounds.yMin *= scale;
        bounds.yMax *= scale;
        height = (int)(width * scale);
    }

    void UpdateBoundaries()
    {
        float t = zoomFactor * Time.deltaTime;
        double xMinDist = Math.Abs(bounds.xMin - destination.real);
        double xMaxDist = Math.Abs(destination.real - bounds.xMax);
        double yMinDist = Math.Abs(bounds.yMin - destination.imag);
        double yMaxDist = Math.Abs(destination.imag - bounds.yMax);

        double xMinDistSc = t * xMinDist;
        double xMaxDistSc = t * xMaxDist;
        double yMinDistSc = t * yMinDist;
        double yMaxDistSc = t * yMaxDist;

        bounds.xMin += xMinDistSc;
        bounds.xMax -= xMaxDistSc;
        bounds.yMin += yMinDistSc;
        bounds.yMax -= yMaxDistSc;
    }

    // Start is called before the first frame update
    void Start()
    {
        d_size = sizeof(double);
        v3_size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3));

        destination.real = realDest;
        destination.imag = imagDest;
        x = 16;// transform.localScale.x;
        y = 9;// transform.localScale.y;
        
        InitImageValues();

        displayTexture = new RenderTexture(width, height, 0);
        displayTexture.autoGenerateMips = false;
        displayTexture.wrapMode = TextureWrapMode.Clamp;
        displayTexture.enableRandomWrite = true;
        displayTexture.filterMode = FilterMode.Point;
        displayTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat;
        displayTexture.Create();

        Calculate();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(displayTexture, destination);
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
    }

    void OnGUI()
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
        }
        if (GUI.Button(new Rect(500, 0, 100, 50), "Screenshot"))
        {
            ScreenCapture.CaptureScreenshot(@"C:\Users\Jan\Desktop\sc\sc.png");
        }
    }

    private void OnMouseDown()
    {
        Vector3 mPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mPos);

    }
}
