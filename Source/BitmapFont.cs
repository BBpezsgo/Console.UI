using System;

namespace CLI;

public readonly ref struct BitmapFont<TPixel>
{
    public readonly ReadOnlySpan<TPixel> Buffer;

    public readonly int Width;
    public readonly int Height;

    public readonly int CharWidth;
    public readonly int CharHeight;

    public BitmapFont(ReadOnlySpan<TPixel> buffer, int width, int height, int charWidth, int charHeight)
    {
        Buffer = buffer;

        Width = width;
        Height = height;

        CharWidth = charWidth;
        CharHeight = charHeight;
    }
}
