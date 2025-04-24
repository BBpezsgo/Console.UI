using System;

namespace CLI;

public static class BufferedRendererExtensions
{
    #region Fill()

    public static void Fill<TPixel>(this BufferedRenderer<TPixel> renderer, TPixel value)
        => Array.Fill(renderer.Buffer, value);

    public static void Fill<TPixel>(this BufferedRenderer<TPixel> renderer, Rect rect, TPixel value)
        => BufferUtils.Fill(renderer.Buffer, renderer.Width, renderer.Height, rect, value);

    #endregion

    #region Put()

    public static void Put<TPixel>(this BufferedRenderer<TPixel> renderer, int x, int y, ReadOnlySpan<TPixel> data, int dataWidth, int dataHeight)
        => BufferUtils.Put(renderer.Buffer, renderer.Width, renderer.Height, x, y, data, dataWidth, dataHeight);

    #endregion

    #region Clear()

    public static void Clear<TPixel>(this BufferedRenderer<TPixel> renderer) => Array.Clear(renderer.Buffer);

    public static void Clear<TPixel>(this BufferedRenderer<TPixel> renderer, Rect rect)
    {
        for (int y = 0; y < rect.Height; y++)
        {
            int actualY = rect.Y + y;
            if (actualY >= renderer.Height) break;
            if (actualY < 0) continue;

            int startIndex = (actualY * renderer.Width) + Math.Max((short)0, rect.Left);
            int endIndex = (actualY * renderer.Width) + Math.Min(renderer.Width - 1, rect.Right);
            int length = Math.Max(0, endIndex - startIndex);

            renderer.Buffer.AsSpan().Slice(startIndex, length).Clear();
        }
    }

    #endregion
}
