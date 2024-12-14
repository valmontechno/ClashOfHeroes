using UnityEngine;

public static class MathUtils
{
    /// <summary>
    /// Maps a float value from one range to another
    /// </summary>
    public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }

    /// <summary>
    /// Maps each component of a vector from one range to another
    /// </summary>
    public static Vector2 Map(Vector2 value, Vector2 fromMin, Vector2 fromMax, Vector2 toMin, Vector2 toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }
}
