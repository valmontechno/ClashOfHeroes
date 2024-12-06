using UnityEngine;

public static class MathUtils
{
    public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

    public static Vector2 Map(Vector2 value, Vector2 fromMin, Vector2 fromMax, Vector2 toMin, Vector2 toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }
}
