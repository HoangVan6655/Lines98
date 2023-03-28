using UnityEngine;

public static class Utilities
{
    public static Color32 ToColor(int hexVal)
    {
        byte R = (byte)((hexVal >> 16) & 0xFF);
        byte G = (byte)((hexVal >> 8) & 0xFF);
        byte B = (byte)((hexVal) & 0xFF);
        return new Color32(R, G, B, 255);
    }

    public static bool IsContainXY(Bounds current, Vector3 target)
    {
        return target.x >= current.min.x && target.x <= current.max.x && target.y >= current.min.y && target.y <= current.max.y;
    }

    public static Vector3 QuadraticBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2) => ((1 - t) * (1 - t) * p0) + (2 * (1 - t) * t * p1) + (t * t * p2);
}