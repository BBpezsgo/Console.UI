using System;
using System.Numerics;

namespace CLI;

public interface IRenderer
{
    public int Width { get; }
    public int Height { get; }

    public void Render();
    public void RefreshBufferSize();
}

public interface IRenderer<TPixel> : IRenderer, IOnlySetterRenderer<TPixel>
{
    public ref TPixel this[int i] { get; }
    public ref TPixel this[int x, int y] => ref this[(y * Width) + x];
    public ref TPixel this[float x, float y] => ref this[((int)MathF.Round(y) * Width) + (int)MathF.Round(x)];
    public ref TPixel this[Vector2 p] => ref this[((int)MathF.Round(p.Y) * Width) + (int)MathF.Round(p.X)];
}

public interface IOnlySetterRenderer<TPixel> : IRenderer
{
    public void Set(int i, TPixel pixel);
}
