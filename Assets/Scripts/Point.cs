using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct Point
{
    public double real;
    public double imag;
    public Color col;

    public Point(double real, double imag)
    {
        this.real = real;
        this.imag = imag;
        col = Color.black;
    }
}