using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MandelbrotGPU))]
public class CustomEditorExtension : Editor
{
    MandelbrotGPU mandelbrotGPU;

    void OnEnable()
    {
        mandelbrotGPU = (MandelbrotGPU)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        mandelbrotGPU.selectedGradient = Gradients.gradientList[(int)mandelbrotGPU.gradient];
    }
}
