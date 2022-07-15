using UnityEngine;

public struct ImageBounds {
    public decimal xMin;
    public decimal xMax;
    public decimal yMin;
    public decimal yMax;

    public ImageBounds(decimal xMin, decimal xMax, decimal yMin, decimal yMax)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.yMin = yMin;
        this.yMax = yMax;
    }
}
