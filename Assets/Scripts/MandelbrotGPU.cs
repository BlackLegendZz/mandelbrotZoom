using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MandelbrotGPU : MonoBehaviour
{
    public ComputeShader computeShader;
    [Range(100, 3000)]
    public int width = 1000;
    [Range(10, 5000)]
    public int iterations = 50;
    [Range(-10,10)]
    public float zoomFactor = 0.1f;
    public string realDest = "0";
    public string imagDest = "0";
    public bool record = false;
    //Just for display
    public Gradient selectedGradient;
    public Gradients.gradients gradient = Gradients.gradients.normal;


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
    int _width;
    Texture2D textureForImg;
    Rect rect;
    uint frameCounter = 0;

    void Calculate()
    {
        Gradient gr = Gradients.gradientList[(int)gradient];
        gradientColours = new Vector3[iterations];
        for (int i = 0; i < iterations; i++)
        {
            Color c = gr.Evaluate(i / (float)iterations);
            Vector3 v = new Vector3(c.r, c.g, c.b);
            gradientColours[i] = v;
        }

        double[] reals = MandelbrotHelper.Linspace((double)bounds.xMin, (double)bounds.xMax, _width);
        double[] imags = MandelbrotHelper.Linspace((double)bounds.yMin, (double)bounds.yMax, height);

        computeBufferReal = new ComputeBuffer(_width, d_size);
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
        computeShader.Dispatch(0, _width / 8, height / 8, 1);

        computeBufferReal.Dispose();
        computeBufferImag.Dispose();
        computeBufferColours.Dispose();

        frameCounter++;
    }

    void InitImageValues()
    {
        //the bounding box starts as a square which might not be the right format
        //so scale the y axis accordingly to avoid stretching
        bounds = new ImageBounds(-2, 2, -2, 2);
        decimal scale = (decimal)(y / x);
        bounds.yMin *= scale;
        bounds.yMax *= scale;
        height = (int)(_width * scale);
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
    
    // Start is called before the first frame update
    void Start()
    {
        d_size = sizeof(double);
        v3_size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3));

        _width = width;
        destination.real = decimal.Parse(realDest);
        destination.imag = decimal.Parse(imagDest);
        Debug.Log($"{destination.real}, {destination.imag}");
        x = 16;// transform.localScale.x;
        y = 9;// transform.localScale.y;
        
        InitImageValues();

        displayTexture = new RenderTexture(_width, height, 0);
        displayTexture.autoGenerateMips = false;
        displayTexture.wrapMode = TextureWrapMode.Clamp;
        displayTexture.enableRandomWrite = true;
        displayTexture.filterMode = FilterMode.Point;
        displayTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat;
        displayTexture.Create();


        textureForImg = new Texture2D(_width, height, TextureFormat.RGBA32, false);
        rect = new Rect(0, 0, _width, height);

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
        
        if (!record)
        {
            return;
        }
        //save rendertexture as image
        RenderTexture.active = displayTexture;
        textureForImg.ReadPixels(rect, 0, 0);
        textureForImg.Apply();
        byte[] bytes;
        bytes = textureForImg.EncodeToPNG();

        string path = @$"C:\Users\Jan\Desktop\sc\{frameCounter}.png";
        System.IO.File.WriteAllBytes(path, bytes);
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
