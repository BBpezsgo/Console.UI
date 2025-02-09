using System;

namespace CLI;

public static class SideCharacters
{
    public static readonly SideCharacters<char> BoxSides = new('┌', '┐', '┘', '└', '─', '│');
    public static readonly SideCharacters<char> PanelSides = new('╒', '═', '╕', '│', '┘', '─', '└', '│');
    public static readonly SideCharacters<char> BoxSidesDouble = new('╔', '╗', '╝', '╚', '═', '║');
    public static readonly SideCharacters<char> BoxSidesShadow = new('┌', '─', '╖', '║', '╝', '═', '╘', '│');
}

public readonly struct SideCharacters<T>
{
    public readonly T TopLeft;
    public readonly T Top;
    public readonly T TopRight;
    public readonly T Right;
    public readonly T BottomRight;
    public readonly T Bottom;
    public readonly T BottomLeft;
    public readonly T Left;

    public SideCharacters(T topLeft, T top, T topRight, T right, T bottomRight, T bottom, T bottomLeft, T left)
    {
        TopLeft = topLeft;
        Top = top;
        TopRight = topRight;
        Right = right;
        BottomRight = bottomRight;
        Bottom = bottom;
        BottomLeft = bottomLeft;
        Left = left;
    }

    public SideCharacters(T topLeft, T topRight, T bottomRight, T bottomLeft, T horizontal, T vertical)
    {
        TopLeft = topLeft;
        Top = horizontal;
        TopRight = topRight;
        Right = vertical;
        BottomRight = bottomRight;
        Bottom = horizontal;
        BottomLeft = bottomLeft;
        Left = vertical;
    }

    public SideCharacters(ReadOnlySpan<T> values)
    {
        if (values.Length != 8)
        { throw new ArgumentException("Length of values must be 8", nameof(values)); }

        TopLeft = values[0];
        Top = values[1];
        TopRight = values[2];
        Right = values[3];
        BottomRight = values[4];
        Bottom = values[5];
        BottomLeft = values[6];
        Left = values[7];
    }
}
