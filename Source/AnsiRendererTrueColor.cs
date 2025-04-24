using System.Runtime.Versioning;
using System.Text;

namespace CLI;

public class AnsiRendererTrueColor : BufferedRenderer<ColoredChar>
{
    readonly StringBuilder Builder;

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public AnsiRendererTrueColor() : this(System.Console.WindowWidth, System.Console.WindowHeight)
    { }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public AnsiRendererTrueColor(int bufferWidth, int bufferHeight) : base(bufferWidth, bufferHeight)
    {
        System.Console.CursorVisible = false;

        Builder = new StringBuilder(_width * _height);
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public override void Render()
    {
        Builder.Clear();

        RGB bg = default;
        RGB fg = default;

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                ref ColoredChar c = ref this[x, y];

                if (bg != c.Background)
                {
                    Ansi.SetBackgroundColor(Builder, c.Background.R, c.Background.G, c.Background.B);
                    bg = c.Background;
                }

                if (fg != c.Foreground)
                {
                    Ansi.SetForegroundColor(Builder, c.Foreground.R, c.Foreground.G, c.Foreground.B);
                    fg = c.Foreground;
                }

                Builder.Append(c.Char is '\0' ? ' ' : c.Char);
            }
        }

        Builder.Append(Ansi.Reset);

        System.Console.CursorVisible = false;
        System.Console.SetCursorPosition(0, 0);
        System.Console.Out.Write(Builder);
        System.Console.SetCursorPosition(0, 0);
        System.Console.ResetColor();
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public override void RefreshBufferSize() => RefreshBufferSize(System.Console.WindowWidth, System.Console.WindowHeight);

    public void RefreshBufferSize(int width, int height)
    {
        _width = width;
        _height = height;

        if (_buffer.Length != _width * _height)
        { _buffer = new ColoredChar[_width * _height]; }
    }
}
