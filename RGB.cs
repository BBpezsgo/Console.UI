using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace CLI;

public readonly struct RGB :
    IEquatable<RGB>,
    IEqualityOperators<RGB, RGB, bool>,

    IAdditionOperators<RGB, RGB, RGB>,
    ISubtractionOperators<RGB, RGB, RGB>,

    IMultiplyOperators<RGB, RGB, RGB>,
    IMultiplyOperators<RGB, int, RGB>,
    IMultiplyOperators<RGB, float, RGB>,

    IDivisionOperators<RGB, int, RGB>,
    IDivisionOperators<RGB, float, RGB>,
    IDivisionOperators<RGB, RGB, RGB>,

    IFormattable,
    ISpanParsable<RGB>,
    IParsable<RGB>
{
    readonly uint v;

    public byte R => (byte)((v >> 16) & byte.MaxValue);
    public byte G => (byte)((v >> 8) & byte.MaxValue);
    public byte B => (byte)((v >> 0) & byte.MaxValue);

    public readonly byte MaxChannel => Math.Max(R, Math.Max(G, B));
    public readonly byte MinChannel => Math.Min(R, Math.Min(G, B));

    const float Lum_R = 0.2126f / (float)byte.MaxValue;
    const float Lum_G = 0.7152f / (float)byte.MaxValue;
    const float Lum_B = 0.0722f / (float)byte.MaxValue;

    public readonly float Luminance => (Lum_R * R) + (Lum_G * G) + (Lum_B * B);
    public readonly byte Intensity => (byte)((int)(R + G + B) / 3);

    public readonly float Brightness => (float)(MaxChannel + MinChannel) / (float)(byte.MaxValue * 2);

    /// <summary>
    /// Source: .NET source code
    /// </summary>
    public readonly float Hue
    {
        get
        {
            if (R == G && G == B) return 0f;

            byte max = MaxChannel;
            byte min = MinChannel;

            int delta = max - min;
            float hue;

            if (R == max)
            { hue = (float)(G - B) / (float)delta; }
            else if (G == max)
            { hue = ((float)(B - R) / (float)delta) + 2f; }
            else
            { hue = ((float)(R - G) / (float)delta) + 4f; }

            hue *= 60f;
            if (hue < 0f)
            { hue += 360f; }

            return hue;
        }
    }

    /// <summary>
    /// Source: .NET source code
    /// </summary>
    public readonly float Saturation
    {
        get
        {
            if (R == G && G == B) return 0f;

            byte max = MaxChannel;
            byte min = MinChannel;

            int div = max + min;
            if (div > byte.MaxValue)
            { div = ((int)byte.MaxValue * 2) - max - min; }

            return (float)(max - min) / (float)div;
        }
    }

    public static readonly RGB White = new(255, 255, 255);
    public static readonly RGB Red = new(255, 0, 0);
    public static readonly RGB Green = new(0, 255, 0);
    public static readonly RGB Blue = new(0, 0, 255);
    public static readonly RGB Yellow = new(255, 255, 0);
    public static readonly RGB Cyan = new(0, 255, 255);
    public static readonly RGB Magenta = new(255, 0, 255);
    public static readonly RGB Black = new(0, 0, 0);
    public static readonly RGB Gray = new(85, 85, 85);
    public static readonly RGB Silver = new(170, 170, 170);

    public RGB(uint v) => this.v = v;
    public RGB(int v) => this.v = unchecked((uint)v);
    public RGB(byte r, byte g, byte b)
    {
        v = unchecked((uint)(b | (g << 8) | (r << 16)));
    }

    public RGB(int r, int g, int b) : this((byte)r, (byte)g, (byte)b) { }
    public RGB(float r, float g, float b) : this((byte)MathF.Round(Math.Clamp(r, 0f, 1f) * (float)byte.MaxValue), (byte)MathF.Round(Math.Clamp(g, 0f, 1f) * (float)byte.MaxValue), (byte)MathF.Round(Math.Clamp(b, 0f, 1f) * (float)byte.MaxValue)) { }

    public static implicit operator System.Drawing.Color(RGB v) => System.Drawing.Color.FromArgb(v.R, v.G, v.B);
    public static implicit operator int(RGB v) => unchecked((int)v.v);

    public static implicit operator RGB(ValueTuple<byte, byte, byte> v) => new(v.Item1, v.Item2, v.Item3);
    public static implicit operator RGB(System.Drawing.Color v) => new(v.R, v.G, v.B);

    public static explicit operator RGB(Vector3 v) => new(v.X, v.Y, v.Z);

    public static bool operator ==(RGB left, RGB right) => left.Equals(right);
    public static bool operator !=(RGB left, RGB right) => !left.Equals(right);

    public static RGB operator +(RGB a, RGB b) => new(a.R + b.R, a.G + b.G, a.B + b.B);
    public static RGB operator -(RGB a, RGB b) => new(a.R - b.R, a.G - b.G, a.B - b.B);

    public static RGB operator *(RGB a, RGB b) => new(a.R * b.R, a.G * b.G, a.B * b.B);
    public static RGB operator *(RGB a, int b) => new(a.R * b, a.G * b, a.B * b);
    public static RGB operator *(RGB a, float b) => new((byte)(a.R * b), (byte)(a.G * b), (byte)(a.B * b));

    public static RGB operator /(RGB a, int b) => new(a.R / b, a.G / b, a.B / b);
    public static RGB operator /(RGB a, float b) => new((byte)(a.R / b), (byte)(a.G / b), (byte)(a.B / b));
    public static RGB operator /(RGB left, RGB right) => new(left.R / right.R, left.G / right.G, left.B / right.B);

    public override int GetHashCode() => unchecked((int)v);
    public override bool Equals(object? obj) => obj is RGB color && Equals(color);
    public bool Equals(RGB other) => v == other.v;

    public static RGB Lerp(RGB a, RGB b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        float _r = (a.R * (1f - t)) + (b.R * t);
        float _g = (a.G * (1f - t)) + (b.G * t);
        float _b = (a.B * (1f - t)) + (b.B * t);
        return new RGB(_r, _g, _b);
    }

    public readonly void Deconstruct(out byte r, out byte g, out byte b)
    {
        r = R;
        g = G;
        b = B;
    }

    /// <summary>
    /// return the (squared) Euclidean distance between two colors
    /// </summary>
    public static int Distance(RGB colorA, RGB colorB)
    {
        int r = colorA.R - colorB.R;
        int g = colorA.G - colorB.G;
        int b = colorA.B - colorB.B;
        return (r * r) + (g * g) + (b * b);
    }

    public static RGB ParseHex(string s) => new(int.Parse(s.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture));

    public static RGB Parse(string s, IFormatProvider? provider)
    {
        s = s.Trim();
        string[] parts = s.Split(' ');

        if (parts.Length != 3)
        { throw new FormatException(); }

        if (!byte.TryParse(parts[0], NumberStyles.Integer, provider, out byte r))
        { throw new FormatException(); }
        if (!byte.TryParse(parts[1], NumberStyles.Integer, provider, out byte g))
        { throw new FormatException(); }
        if (!byte.TryParse(parts[2], NumberStyles.Integer, provider, out byte b))
        { throw new FormatException(); }

        return new RGB(r, g, b);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out RGB result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s)) return false;

        s = s.Trim();
        string[] parts = s.Split(' ');

        if (parts.Length != 3)
        { return false; }

        if (!byte.TryParse(parts[0], NumberStyles.Integer, provider, out byte r))
        { return false; }
        if (!byte.TryParse(parts[1], NumberStyles.Integer, provider, out byte g))
        { return false; }
        if (!byte.TryParse(parts[2], NumberStyles.Integer, provider, out byte b))
        { return false; }

        result = new RGB(r, g, b);
        return true;
    }

    public static RGB Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        Span<Range> ranges = stackalloc Range[3];
        int partCount = s.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries);

        if (partCount != 3)
        { throw new FormatException(); }

        if (!byte.TryParse(s[ranges[0]], NumberStyles.Integer, provider, out byte r))
        { throw new FormatException(); }
        if (!byte.TryParse(s[ranges[1]], NumberStyles.Integer, provider, out byte g))
        { throw new FormatException(); }
        if (!byte.TryParse(s[ranges[2]], NumberStyles.Integer, provider, out byte b))
        { throw new FormatException(); }

        return new RGB(r, g, b);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out RGB result)
    {
        result = default;

        if (s.IsEmpty) return false;

        Span<Range> ranges = stackalloc Range[3];
        int partCount = s.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries);

        if (partCount != 3)
        { return false; }

        if (!byte.TryParse(s[ranges[0]], NumberStyles.Integer, provider, out byte r))
        { return false; }
        if (!byte.TryParse(s[ranges[1]], NumberStyles.Integer, provider, out byte g))
        { return false; }
        if (!byte.TryParse(s[ranges[2]], NumberStyles.Integer, provider, out byte b))
        { return false; }

        result = new RGB(r, g, b);
        return true;
    }

    public override string ToString() => $"({R} {G} {B})";
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        if (format is null) return ToString();
        return ToString((ReadOnlySpan<char>)format);
    }
    public string ToString(ReadOnlySpan<char> format)
    {
        if (format.IsEmpty) return ToString();

        switch (format)
        {
            case "X": return $"#{R:X2}{G:X2}{B:X2}";
            case "x": return $"#{R:X2}{G:X2}{B:X2}";

            case "X1": return $"#{R:X1}{G:X1}{B:X1}";
            case "x1": return $"#{R:X1}{G:X1}{B:X1}";

            case "X2": return $"#{R:X2}{G:X2}{B:X2}";
            case "x2": return $"#{R:X2}{G:X2}{B:X2}";

            default:
                {
                    StringBuilder result = new(format.Length);
                    for (int i = 0; i < format.Length; i++)
                    {
                        switch (format[i])
                        {
                            case 'R': result.Append(R); break;
                            case 'G': result.Append(G); break;
                            case 'B': result.Append(B); break;
                            default: result.Append(format[i]); break;
                        }
                    }
                    return result.ToString();
                }
        }
    }
}
