using SkiaSharp;

public static class SkiaColorHelpers
{
    /// <summary>
    /// Returns a lighter version of the color. Factor > 1.0 lightens the color.
    /// </summary>
    public static SKColor Lighter(this SKColor color, float factor = 1.2f)
    {
        return AdjustBrightness(color, factor);
    }

    /// <summary>
    /// Returns a darker version of the color. Factor > 1.0 darkens the color.
    /// </summary>
    public static SKColor Darker(this SKColor color, float factor = 1.2f)
    {
        return AdjustBrightness(color, 1f / factor);
    }

    /// <summary>
    /// Checks if the color is valid (e.g., not transparent black).
    /// </summary>
    public static bool IsValid(this SKColor color)
    {
        return color.Alpha != 0 || (color.Red != 0 || color.Green != 0 || color.Blue != 0);
    }

    /// <summary>
    /// Returns a brighter or darker version of the color by modifying its RGB values.
    /// </summary>
    private static SKColor AdjustBrightness(SKColor color, float factor)
    {
        byte r = Clamp(color.Red * factor);
        byte g = Clamp(color.Green * factor);
        byte b = Clamp(color.Blue * factor);
        return new SKColor(r, g, b, color.Alpha);
    }

    private static byte Clamp(float value)
    {
        return (byte)Math.Clamp(value, 0, 255);
    }
}