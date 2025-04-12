using System;
using System.Runtime.Versioning;

namespace CLI;

public class ConsoleRenderer : BufferedRenderer<ConsoleChar>
{
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public ConsoleRenderer() : this(Console.WindowWidth, Console.WindowHeight)
    { }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public ConsoleRenderer(int bufferWidth, int bufferHeight) : base(bufferWidth, bufferHeight)
    {
        Console.CursorVisible = false;
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public override void Render()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                ref readonly var c = ref this[x ,y];
                Console.ForegroundColor = c.Foreground;
                Console.BackgroundColor = c.Background;
                Console.Write(c.Char);
            }
        }

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
        { _buffer = new ConsoleChar[_width * _height]; }
    }
}
