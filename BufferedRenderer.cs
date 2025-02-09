using System;
using System.Numerics;

namespace CLI;

public abstract class BufferedRenderer<TPixel> : IRenderer<TPixel>
{
    protected int _width;
    protected int _height;
    protected TPixel[] _buffer;

    public int Width => _width;
    public int Height => _height;
    public TPixel[] Buffer => _buffer;

    public ref TPixel this[int i] => ref _buffer[i];
    public ref TPixel this[int x, int y] => ref this[(y * Width) + x];
    public ref TPixel this[float x, float y] => ref this[((int)MathF.Round(y) * Width) + (int)MathF.Round(x)];
    public ref TPixel this[(int X, int Y) p] => ref this[(p.Y * Width) + p.X];
    public ref TPixel this[Vector2 p] => ref this[((int)MathF.Round(p.Y) * Width) + (int)MathF.Round(p.X)];

    protected BufferedRenderer(int bufferWidth, int bufferHeight)
    {
        _width = bufferWidth;
        _height = bufferHeight;
        _buffer = new TPixel[Width * Height];
    }

    public abstract void Render();

    public abstract void RefreshBufferSize();

    public void Set(int i, TPixel pixel)
    {
        if (i >= 0 && i < _buffer.Length) _buffer[i] = pixel;
    }

    public void Clear() => Array.Clear(_buffer);
}
