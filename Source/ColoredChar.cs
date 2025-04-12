using System;

namespace CLI;

public struct ColoredChar :
    IEquatable<ColoredChar>,
    IEquatable<char>
{
    public static ColoredChar Empty => new(' ', default, default);

    public char Char;
    public RGB Foreground;
    public RGB Background;

    public ColoredChar(char @char)
    {
        Char = @char;
        Foreground = RGB.Silver;
        Background = RGB.Black;
    }

    public ColoredChar(char @char, RGB foreground, RGB background = default)
    {
        Char = @char;
        Foreground = foreground;
        Background = background;
    }

    public override readonly bool Equals(object? obj) => obj is ColoredChar charInfo && Equals(charInfo);
    public readonly bool Equals(ColoredChar other) => Foreground == other.Foreground && Background == other.Background && Char == other.Char;
    public readonly bool Equals(char other) => Char == other;

    public override readonly int GetHashCode() => HashCode.Combine(Char, Foreground, Background);

    public static bool operator ==(ColoredChar a, ColoredChar b) => a.Equals(b);
    public static bool operator !=(ColoredChar a, ColoredChar b) => !a.Equals(b);

    public static bool operator ==(ColoredChar a, char b) => a.Char == b;
    public static bool operator !=(ColoredChar a, char b) => a.Char != b;

    public static bool operator ==(char a, ColoredChar b) => a == b.Char;
    public static bool operator !=(char a, ColoredChar b) => a != b.Char;

    public static explicit operator ColoredChar(char c) => new(c);
    public static implicit operator char(ColoredChar c) => c.Char;

    public static implicit operator ColoredChar(AnsiChar c) => new(c.Char, c.Foreground.ToRGB(), c.Background.ToRGB());
    public static implicit operator ColoredChar(ConsoleChar c) => new(c.Char, c.Foreground.ToRGB(), c.Background.ToRGB());

    public override readonly string ToString() => Char.ToString();
}
