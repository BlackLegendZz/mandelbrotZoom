using UnityEngine;

public struct ImageBounds {
    public double xMin;
    public double xMax;
    public double yMin;
    public double yMax;

    public ImageBounds(double xMin, double xMax, double yMin, double yMax)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.yMin = yMin;
        this.yMax = yMax;
    }
}
