using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal static class MandelbrotHelper
{
    public static decimal[] Linspace(decimal min, decimal max, int length)
    {
        decimal[] space = new decimal[length];
        decimal step = (max - min) / length;
        decimal val = min;

        for (int i = 0; i < length; i++)
        {
            space[i] = val;
            val += step;
        }
        return space;
    }

    public static double[] Linspace(double min, double max, int length)
    {
        double[] space = new double[length];
        double step = (max - min) / length;
        double val = min;

        for (int i = 0; i < length; i++)
        {
            space[i] = val;
            val += step;
        }
        return space;
    }

    public static Color[] GetColoursFromPoints(Point[] points)
    {
        Color[] cols = new Color[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            cols[i] = points[i].col;
        }
        return cols;
    }

    public static Color[] FlatImg(int width, int height, double[,] image)
    {
        Color[] flattened = new Color[width * height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float v = (float)image[i, j];
                Color c = new Color(v, v, v, 1.0f);
                flattened[i + width * j] = c;
            }
        }
        return flattened;
    }
}