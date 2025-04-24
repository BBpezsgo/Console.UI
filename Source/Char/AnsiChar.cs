using System;

namespace CLI;

public struct AnsiChar :
    IChar<AnsiColor>,
    IEquatable<AnsiChar>,
    IEquatable<char>
{
    public static readonly AnsiChar Empty = new(' ', AnsiColor.Black, AnsiColor.Black);

    readonly AnsiColor IChar<AnsiColor>.Foreground => Foreground;
    readonly AnsiColor IChar<AnsiColor>.Background => Background;
    readonly char IChar.Char => Char;

    public char Char;
    public AnsiColor Foreground;
    public AnsiColor Background;

    public AnsiChar(char @char)
    {
        Char = @char;
        Foreground = AnsiColor.White;
        Background = AnsiColor.Black;
    }

    public AnsiChar(char @char, AnsiColor foreground, AnsiColor background = AnsiColor.Black)
    {
        Char = @char;
        Foreground = foreground;
        Background = background;
    }

    public override readonly bool Equals(object? obj) => obj is AnsiChar charInfo && Equals(charInfo);
    public readonly bool Equals(AnsiChar other) => Foreground == other.Foreground && Background == other.Background && Char == other.Char;
    public readonly bool Equals(char other) => Char == other;

    public override readonly int GetHashCode() => HashCode.Combine(Char, Foreground, Background);

    public static bool operator ==(AnsiChar a, AnsiChar b) => a.Equals(b);
    public static bool operator !=(AnsiChar a, AnsiChar b) => !a.Equals(b);

    public static bool operator ==(AnsiChar a, char b) => a.Char == b;
    public static bool operator !=(AnsiChar a, char b) => a.Char != b;

    public static bool operator ==(char a, AnsiChar b) => a == b.Char;
    public static bool operator !=(char a, AnsiChar b) => a != b.Char;

    public static explicit operator AnsiChar(char c) => new(c);
    public static implicit operator char(AnsiChar c) => c.Char;

    public static explicit operator AnsiChar(ColoredChar c) => new(c.Char, c.Foreground.ToAnsi(), c.Background.ToAnsi());
    public static implicit operator AnsiChar(ConsoleChar c) => new(c.Char, c.Foreground.ToAnsi(), c.Background.ToAnsi());

    public override readonly string ToString() => Char.ToString();
}
