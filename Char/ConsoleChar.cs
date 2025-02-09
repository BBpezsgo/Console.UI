using System;

namespace CLI;

public struct ConsoleChar :
    IEquatable<ConsoleChar>,
    IEquatable<char>
{
    public static ConsoleChar Empty => new(' ', ConsoleColor.Black, ConsoleColor.Black);

    public char Char;
    public ConsoleColor Foreground;
    public ConsoleColor Background;

    public ConsoleChar(char @char)
    {
        Char = @char;
        Foreground = ConsoleColor.White;
        Background = ConsoleColor.Black;
    }

    public ConsoleChar(char @char, ConsoleColor foreground, ConsoleColor background = ConsoleColor.Black)
    {
        Char = @char;
        Foreground = foreground;
        Background = background;
    }

    public override readonly bool Equals(object? obj) => obj is ConsoleChar charInfo && Equals(charInfo);
    public readonly bool Equals(ConsoleChar other) => Foreground == other.Foreground && Background == other.Background && Char == other.Char;
    public readonly bool Equals(char other) => Char == other;

    public override readonly int GetHashCode() => HashCode.Combine(Char, Foreground, Background);

    public static bool operator ==(ConsoleChar a, ConsoleChar b) => a.Equals(b);
    public static bool operator !=(ConsoleChar a, ConsoleChar b) => !a.Equals(b);

    public static bool operator ==(ConsoleChar a, char b) => a.Char == b;
    public static bool operator !=(ConsoleChar a, char b) => a.Char != b;

    public static bool operator ==(char a, ConsoleChar b) => a == b.Char;
    public static bool operator !=(char a, ConsoleChar b) => a != b.Char;

    public static explicit operator ConsoleChar(char c) => new(c);
    public static implicit operator char(ConsoleChar c) => c.Char;

    public static explicit operator ConsoleChar(ColoredChar c) => new(c.Char, c.Foreground.ToConsole(), c.Background.ToConsole());
    public static explicit operator ConsoleChar(AnsiChar c) => new(c.Char, c.Foreground.ToConsole(), c.Background.ToConsole());

    public override readonly string ToString() => Char.ToString();
}
