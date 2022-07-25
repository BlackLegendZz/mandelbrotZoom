using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MandelbrotGPU : MonoBehaviour
{
    public ComputeShader computeShader;

    [Range(100, 3000)]
    public int width = 1000;

    [Range(1, 120)]
    public int keyframes = 1;
    public bool record = false;

    [Range(10, 5000), Header("Level of detail")]
    public int iterations = 50;

    [Range(-0.1f,0.1f), Header("Zoom Variables")]
    public float zoomFactor = 0.01f;

    [Range(1,10)]
    public float zoomMultiplier = 1;

    [Header("Destination")]
    public string realDest = "0";
    public string imagDest = "0";

    [Header("Visual Settings")]
    public Gradient selectedGradient; //Just for display
    public Gradients.gradients gradient = Gradients.gradients.normal;


    ComputeBuffer computeBufferReal;
    ComputeBuffer computeBufferImag;
    ComputeBuffer computeBufferColours;
    Point destination;
    ImageBounds bounds;
    int height;
    int _width;
    float x, y;
    bool run = false;
    RenderTexture displayTexture;
    RenderTexture currentTexture;
    RenderTexture nextTexture;
    RenderTexture splitTexture;
    Texture2D textureForImg;
    Vector3[] gradientColours;
    int d_size;
    int v3_size;
    Rect rect;
    int frameCounter = 0;
    int countToKeyframe = 1;

    void GetDiscreteGradient()
    {
        Gradient gr = Gradients.gradientList[(int)gradient];
        gradientColours = new Vector3[iterations];
        for (int i = 0; i < iterations; i++)
        {
            Color c = gr.Evaluate(i / (float)iterations);
            Vector3 v = new Vector3(c.r, c.g, c.b);
            gradientColours[i] = v;
        }
    }

    void SaveAsImage()
    {
        RenderTexture t = RenderTexture.active;
        //save rendertexture as image
        RenderTexture.active = displayTexture;
        textureForImg.ReadPixels(rect, 0, 0);
        textureForImg.Apply();
        byte[] bytes;
        bytes = textureForImg.EncodeToJPG(100);

        string path = @$"C:\Users\Jan\Desktop\sc\{frameCounter}.jpg";
        System.IO.File.WriteAllBytes(path, bytes);
        RenderTexture.active = t;
    }

    void Calculate()
    {

        GetDiscreteGradient();
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
        computeShader.SetTexture(0, "nextImg", nextTexture);
        computeShader.SetFloat("iterations", iterations);
        computeShader.SetInt("offset", 0);
        computeShader.Dispatch(0, _width / 8, height / 8, 1);

        computeBufferReal.Dispose();
        computeBufferImag.Dispose();
        computeBufferColours.Dispose();

        frameCounter++;
    }
    
    void CalculateSplit()
    {
        double[] reals = MandelbrotHelper.Linspace((double)bounds.xMin, (double)bounds.xMax, _width);
        double[] imags = MandelbrotHelper.Linspace((double)bounds.yMin, (double)bounds.yMax, height);

        int heightSplitStart = (int)(height * (countToKeyframe - 1) / (float)keyframes);
        int heightSplitEnd = (int)(height * countToKeyframe / (float)keyframes);

        if (heightSplitStart != 0) {
            heightSplitStart--;
        }
        double[] imagsSplit = new double[heightSplitEnd - heightSplitStart];
        for (int i = 0; i < imagsSplit.Length; i++)
        {
            imagsSplit[i] = imags[i + heightSplitStart];
        }

        computeBufferReal = new ComputeBuffer(_width, d_size);
        computeBufferImag = new ComputeBuffer(imagsSplit.Length, d_size);
        computeBufferColours = new ComputeBuffer(iterations, v3_size);
        computeBufferReal.SetData(reals);
        computeBufferImag.SetData(imagsSplit);
        computeBufferColours.SetData(gradientColours);

        computeShader.SetBuffer(2, "reals", computeBufferReal);
        computeShader.SetBuffer(2, "imags", computeBufferImag);
        computeShader.SetBuffer(2, "colours", computeBufferColours);
        computeShader.SetTexture(2, "splitImg", splitTexture);
        computeShader.SetFloat("iterations", iterations);
        computeShader.SetInt("offset", heightSplitStart);
        computeShader.Dispatch(2, _width / 8, imagsSplit.Length / 8, 1);

        computeBufferReal.Dispose();
        computeBufferImag.Dispose();
        computeBufferColours.Dispose();
    }

    void LerpRenderTextures()
    {
        float lerpVal = (float)countToKeyframe / keyframes;

        computeShader.SetTexture(1, "currentImg", currentTexture);
        computeShader.SetTexture(1, "nextImg", nextTexture);
        computeShader.SetTexture(1, "displayTexture", displayTexture);
        computeShader.SetFloat("lerpVal", lerpVal);
        computeShader.Dispatch(1, _width / 8, height / 8, 1);

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
        decimal t = (decimal)(zoomFactor * zoomMultiplier);
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

        displayTexture = MandelbrotHelper.CreateNewRenderTexture(_width, height);
        currentTexture = MandelbrotHelper.CreateNewRenderTexture(_width, height);
        nextTexture = MandelbrotHelper.CreateNewRenderTexture(_width, height);
        splitTexture = MandelbrotHelper.CreateNewRenderTexture(_width, height);


        textureForImg = new Texture2D(_width, height, TextureFormat.RGBA32, false);
        rect = new Rect(0, 0, _width, height);

        //Calculate the states of the first two keyframes
        Calculate();
        Graphics.Blit(nextTexture, currentTexture);
        Graphics.Blit(nextTexture, displayTexture);
        UpdateBoundaries();
        Calculate();
        UpdateBoundaries(); //Setup for split-creation of the 3rd frame
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

        if (countToKeyframe == keyframes)
        {
            CalculateSplit();
            Graphics.Blit(nextTexture, currentTexture);
            Graphics.Blit(nextTexture, displayTexture);
            Graphics.Blit(splitTexture, nextTexture);
            UpdateBoundaries(); //Update for split calculation of coming frame
            countToKeyframe = 1;
        }
        else
        {
            CalculateSplit();
            LerpRenderTextures();
            countToKeyframe++;
        }

        if (record)
        {
            SaveAsImage();
        }
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
            Graphics.Blit(nextTexture, displayTexture);
            countToKeyframe = 1;
        }
        if (GUI.Button(new Rect(500, 0, 100, 50), "Screenshot"))
        {
            ScreenCapture.CaptureScreenshot(@"C:\Users\Jan\Desktop\sc\sc.png");
        }
    } 
}
