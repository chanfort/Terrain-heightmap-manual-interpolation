using System.Runtime.CompilerServices;

public static class GenericMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Interpolate(float x, float x0, float x1, float y0, float y1)
    {
        return (y0 + (y1 - y0) * (x - x0) / (x1 - x0));
    }
}
