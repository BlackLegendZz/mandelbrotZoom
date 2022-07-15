using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Gradients
{
    static GradientAlphaKey[] alphaKeys = {
        new GradientAlphaKey(1, 0),
        new GradientAlphaKey(1, 1)
    };

    static Gradient normal = new Gradient
    {
        alphaKeys = alphaKeys,
        colorKeys = new[]
        {
            new GradientColorKey(new Color(0.000f, 0.007f, 0.392f), 0.000f),
            new GradientColorKey(new Color(0.125f, 0.419f, 0.796f), 0.160f),
            new GradientColorKey(new Color(0.929f, 1.000f, 1.000f), 0.420f),
            new GradientColorKey(new Color(1.000f, 0.666f, 0.035f), 0.646f),
            new GradientColorKey(new Color(0.000f, 0.007f, 0.000f), 0.857f),
            new GradientColorKey(new Color(0.000f, 0.007f, 0.392f), 1.000f)
        }
    };
    static Gradient grey = new Gradient
    {
        alphaKeys = alphaKeys,
        colorKeys = new[]
        {
            new GradientColorKey(new Color(0, 0, 0), 0),
            new GradientColorKey(new Color(1, 1, 1), 1)
        }
    };
    static Gradient magma = new Gradient
    {
        alphaKeys = alphaKeys,
        colorKeys = new[]
        {
            new GradientColorKey(new Color(0.000f, 0.000f, 0.015f), 0.000f),
            new GradientColorKey(new Color(0.117f, 0.062f, 0.282f), 0.142f),
            new GradientColorKey(new Color(0.392f, 0.098f, 0.494f), 0.285f),
            new GradientColorKey(new Color(0.592f, 0.172f, 0.470f), 0.428f),
            new GradientColorKey(new Color(0.823f, 0.262f, 0.411f), 0.571f),
            new GradientColorKey(new Color(0.956f, 0.474f, 0.356f), 0.714f),
            new GradientColorKey(new Color(0.949f, 0.709f, 0.486f), 0.857f),
            new GradientColorKey(new Color(0.968f, 0.972f, 0.717f), 1.000f)
        }
    };
    static Gradient twilight = new Gradient
    {
        alphaKeys = alphaKeys,
        colorKeys = new[]
        {
            new GradientColorKey(new Color(0.886f, 0.85f, 0.888f), 0.0f),
            new GradientColorKey(new Color(0.538f, 0.678f, 0.771f), 0.143f),
            new GradientColorKey(new Color(0.374f, 0.386f, 0.708f), 0.286f),
            new GradientColorKey(new Color(0.293f, 0.086f, 0.407f), 0.429f),
            new GradientColorKey(new Color(0.301f, 0.077f, 0.262f), 0.571f),
            new GradientColorKey(new Color(0.632f, 0.245f, 0.312f), 0.714f),
            new GradientColorKey(new Color(0.785f, 0.574f, 0.461f), 0.857f),
            new GradientColorKey(new Color(0.884f, 0.848f, 0.873f), 1.0f)
        }
    };
    public enum gradients { normal, grey, magma, twilight };
    public static Gradient[] gradientList = { normal, grey, magma, twilight };
}
