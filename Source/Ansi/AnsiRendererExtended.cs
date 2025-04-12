using System;
using System.Runtime.Versioning;
using System.Text;

namespace CLI;

public partial class AnsiRendererExtended : BufferedRenderer<AnsiChar>
{
    readonly StringBuilder Builder;

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public AnsiRendererExtended() : this(Console.WindowWidth, Console.WindowHeight)
    { }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public AnsiRendererExtended(int bufferWidth, int bufferHeight) : base(bufferWidth, bufferHeight)
    {
        Console.CursorVisible = false;

        Builder = new StringBuilder(_width * _height);
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public override void Render()
    {
        Builder.Clear();

        AnsiColor prevForegroundColor = default;
        AnsiColor prevBackgroundColor = default;

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Ansi.FromConsoleChar(
                    Builder,
                    this[x, y],
                    ref prevForegroundColor,
                    ref prevBackgroundColor,
                    x == 0 && y == 0);
            }
        }

        Builder.Append(Ansi.Reset);

        Console.CursorVisible = false;
        Console.SetCursorPosition(0, 0);
        Console.Out.Write(Builder);
        Console.SetCursorPosition(0, 0);
        Console.ResetColor();
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public override void RefreshBufferSize() => RefreshBufferSize(Console.WindowWidth, Console.WindowHeight);

    public void RefreshBufferSize(int width, int height)
    {
        _width = width;
        _height = height;

        if (_buffer.Length != _width * _height)
        { _buffer = new AnsiChar[_width * _height]; }
    }

    public static void RenderExtended(ReadOnlySpan<RGB> buffer, int width, int height)
    {
        StringBuilder builder = new(width * height);
        byte prevColor = default;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = (y * width) + x;
                RGB color = buffer[i];
                byte bruh = Ansi.ToAnsi256(color.R, color.G, color.B);

                if ((x == 0 && y == 0) || prevColor != bruh)
                {
                    Ansi.SetBackgroundColor(builder, (AnsiColor)bruh);
                    prevColor = bruh;
                }

                builder.Append(' ');
            }
        }
        Console.Out.Write(builder);
        Console.SetCursorPosition(0, 0);
    }

    public static void RenderTrueColor(ReadOnlySpan<RGB> buffer, int width, int height)
    {
        StringBuilder builder = new(width * height);
        RGB prevColor = default;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = (y * width) + x;
                RGB color = buffer[i];

                if ((x == 0 && y == 0) || prevColor != color)
                {
                    Ansi.SetBackgroundColor(builder, color.R, color.G, color.B);
                    prevColor = color;
                }

                builder.Append(' ');
            }
        }
        Console.Out.Write(builder);
        Console.SetCursorPosition(0, 0);
    }
}
