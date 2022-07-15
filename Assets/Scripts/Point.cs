using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct Point
{
    public decimal real;
    public decimal imag;
    public Color col;

    public Point(decimal real, decimal imag)
    {
        this.real = real;
        this.imag = imag;
        col = Color.black;
    }
}