using System;
using System.Numerics;
using Maths;

namespace CLI;

public static partial class RendererExtensions
{
    public static void ApplyBloom(
        this BufferedRenderer<ColorF> renderer,
        int radius) => ColorUtils.Bloom(renderer.Buffer, renderer.Width, renderer.Height, radius);

    #region  Fill Triangle

    public static void FillTriangle<TPixel>(
        this IRenderer<TPixel> renderer,
        Array2D<float>? depth,
        Vector2Int a, Vector3 texA,
        Vector2Int b, Vector3 texB,
        Vector2Int c, Vector3 texC,
        Image image, Func<ColorF, TPixel> converter)
        => renderer.FillTriangle<TPixel>(
            depth,
            a.X, a.Y, texA.X, texA.Y, texA.Z,
            b.X, b.Y, texB.X, texB.Y, texB.Z,
            c.X, c.Y, texC.X, texC.Y, texC.Z,
            image, converter);

    public static void FillTriangle<TPixel>(
        this IRenderer<TPixel> renderer,
        Array2D<float>? depth,
        int x1, int y1, float u1, float v1, float w1,
        int x2, int y2, float u2, float v2, float w2,
        int x3, int y3, float u3, float v3, float w3,
        Image image, Func<ColorF, TPixel> converter)
    {
        // sort the points vertically
        if (y2 < y1)
        {
            Swap(ref x1, ref x2);
            Swap(ref y1, ref y2);
            Swap(ref u1, ref u2);
            Swap(ref v1, ref v2);
            Swap(ref w1, ref w2);
        }

        if (y3 < y1)
        {
            Swap(ref x1, ref x3);
            Swap(ref y1, ref y3);
            Swap(ref u1, ref u3);
            Swap(ref v1, ref v3);
            Swap(ref w1, ref w3);
        }

        if (y2 > y3)
        {
            Swap(ref x2, ref x3);
            Swap(ref y2, ref y3);
            Swap(ref u2, ref u3);
            Swap(ref v2, ref v3);
            Swap(ref w2, ref w3);
        }

        int dy1 = y2 - y1;
        int dx1 = x2 - x1;
        float dv1 = v2 - v1;
        float du1 = u2 - u1;
        float dw1 = w2 - w1;

        int dy2 = y3 - y1;
        int dx2 = x3 - x1;
        float dv2 = v3 - v1;
        float du2 = u3 - u1;
        float dw2 = w3 - w1;

        float texU, texV, texW;

        float daxStep = 0f;
        float dbxStep = 0f;
        float du1Step = 0f;
        float dv1Step = 0f;
        float du2Step = 0f;
        float dv2Step = 0f;
        float dw1Step = 0f;
        float dw2Step = 0f;

        if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
        if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

        if (dy1 != 0) du1Step = du1 / MathF.Abs(dy1);
        if (dy1 != 0) dv1Step = dv1 / MathF.Abs(dy1);
        if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

        if (dy2 != 0) du2Step = du2 / MathF.Abs(dy2);
        if (dy2 != 0) dv2Step = dv2 / MathF.Abs(dy2);
        if (dy2 != 0) dw2Step = dw2 / MathF.Abs(dy2);

        if (dy1 != 0)
        {
            for (int i = y1; i <= y2; i++)
            {
                int ax = (int)(x1 + ((i - y1) * daxStep));
                int bx = (int)(x1 + ((i - y1) * dbxStep));

                float texSu = u1 + ((i - y1) * du1Step);
                float texSv = v1 + ((i - y1) * dv1Step);
                float texSw = w1 + ((i - y1) * dw1Step);

                float texEu = u1 + ((i - y1) * du2Step);
                float texEv = v1 + ((i - y1) * dv2Step);
                float texEw = w1 + ((i - y1) * dw2Step);

                if (ax > bx)
                {
                    Swap(ref ax, ref bx);
                    Swap(ref texSu, ref texEu);
                    Swap(ref texSv, ref texEv);
                    Swap(ref texSw, ref texEw);
                }

                float tStep = 1f / (float)(bx - ax);
                float t = 0f;

                for (int j = ax; j < bx; j++)
                {
                    texU = ((1f - t) * texSu) + (t * texEu);
                    texV = ((1f - t) * texSv) + (t * texEv);
                    texW = ((1f - t) * texSw) + (t * texEw);

                    if (renderer.IsVisible(j, i) && (!depth.HasValue || texW > depth.Value[j, i]))
                    {
                        ColorF c = image.NormalizedSample(texU / texW, texV / texW);
                        renderer[j, i] = converter.Invoke(c);
                        // BloomBlur[j, i] = c;
                        if (depth.HasValue) depth.Value[j, i] = texW;
                    }

                    t += tStep;
                }
            }
        }

        dy1 = y3 - y2;
        dx1 = x3 - x2;
        dv1 = v3 - v2;
        du1 = u3 - u2;
        dw1 = w3 - w2;

        if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
        if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

        du1Step = 0f;
        dv1Step = 0f;
        if (dy1 != 0) du1Step = du1 / MathF.Abs(dy1);
        if (dy1 != 0) dv1Step = dv1 / MathF.Abs(dy1);
        if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

        if (dy1 != 0)
        {
            for (int i = y2; i <= y3; i++)
            {
                int ax = (int)(x2 + ((i - y2) * daxStep));
                int bx = (int)(x1 + ((i - y1) * dbxStep));

                float texSu = u2 + ((i - y2) * du1Step);
                float texSv = v2 + ((i - y2) * dv1Step);
                float texSw = w2 + ((i - y2) * dw1Step);

                float texEu = u1 + ((i - y1) * du2Step);
                float texEv = v1 + ((i - y1) * dv2Step);
                float texEw = w1 + ((i - y1) * dw2Step);

                if (ax > bx)
                {
                    Swap(ref ax, ref bx);
                    Swap(ref texSu, ref texEu);
                    Swap(ref texSv, ref texEv);
                    Swap(ref texSw, ref texEw);
                }

                float tStep = 1f / (float)(bx - ax);
                float t = 0f;

                for (int j = ax; j < bx; j++)
                {
                    texU = ((1f - t) * texSu) + (t * texEu);
                    texV = ((1f - t) * texSv) + (t * texEv);
                    texW = ((1f - t) * texSw) + (t * texEw);

                    if (renderer.IsVisible(j, i) && (!depth.HasValue || texW > depth.Value[j, i]))
                    {
                        ColorF c = image.NormalizedSample(texU / texW, texV / texW);
                        renderer[j, i] = converter.Invoke(c);
                        // BloomBlur[j, i] = c;
                        if (depth.HasValue) depth.Value[j, i] = texW;
                    }

                    t += tStep;
                }
            }
        }
    }
    
    #endregion

    #region Text (pixel renderer)

    static Point CharacterCoord(char @char, int sheetWidth, int sheetHeight, int charWidth, int charHeight)
    {
        int cols = sheetWidth / charWidth;
        int rows = sheetHeight / charHeight;

        int characterX = @char % cols;
        int characterY = @char / rows;

        return new Point(characterX * charWidth, characterY * charHeight);
    }

    static Point CharacterCoord(byte @char, int sheetWidth, int sheetHeight, int charWidth, int charHeight)
    {
        int cols = sheetWidth / charWidth;
        int rows = sheetHeight / charHeight;

        int characterX = @char % cols;
        int characterY = @char / rows;

        return new Point(characterX * charWidth, characterY * charHeight);
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Char<TPixel>(this IOnlySetterRenderer<TPixel> self, int x, int y, char @char, ReadOnlySpan<TPixel> fontSheetBuffer, int fontSheetWidth, int fontSheetHeight, int charWidth, int charHeight)
    {
        Point charCoord = CharacterCoord(@char, fontSheetWidth, fontSheetHeight, charWidth, charHeight);

        for (int offsetY = 0; offsetY < charHeight; offsetY++)
        {
            if (y + offsetY < 0) continue;
            if (y + offsetY >= self.Height) break;

            int i = charCoord.X + ((charCoord.Y + offsetY) * fontSheetHeight);
            self.Put(x, y + offsetY, fontSheetBuffer.Slice(i, charWidth), charWidth, 1);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Char<TPixel>(this IOnlySetterRenderer<TPixel> self, int x, int y, byte @char, ReadOnlySpan<TPixel> fontSheetBuffer, int fontSheetWidth, int fontSheetHeight, int charWidth, int charHeight)
    {
        Point charCoord = CharacterCoord(@char, fontSheetWidth, fontSheetHeight, charWidth, charHeight);

        for (int offsetY = 0; offsetY < charHeight; offsetY++)
        {
            if (y + offsetY < 0) continue;
            if (y + offsetY >= self.Height) break;

            int i = charCoord.X + ((charCoord.Y + offsetY) * fontSheetHeight);
            self.Put(x, y + offsetY, fontSheetBuffer.Slice(i, charWidth), charWidth, 1);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Text<TPixel>(this IOnlySetterRenderer<TPixel> self, int x, int y, ReadOnlySpan<char> text, ReadOnlySpan<TPixel> fontSheetBuffer, int fontSheetWidth, int fontSheetHeight, int fontWidth, int fontHeight)
    {
        if (text.IsEmpty) return;
        if (y < 0 || y >= self.Height) return;

        for (int i = 0; i < text.Length; i++)
        {
            self.Char(x + (i * fontWidth), y, text[i], fontSheetBuffer, fontSheetWidth, fontSheetHeight, fontWidth, fontHeight);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Text<TPixel>(this IOnlySetterRenderer<TPixel> self, int x, int y, ReadOnlySpan<byte> text, ReadOnlySpan<TPixel> fontSheetBuffer, int fontSheetWidth, int fontSheetHeight, int fontWidth, int fontHeight)
    {
        if (text.IsEmpty) return;
        if (y < 0 || y >= self.Height) return;

        for (int i = 0; i < text.Length; i++)
        {
            self.Char(x + (i * fontWidth), y, text[i], fontSheetBuffer, fontSheetWidth, fontSheetHeight, fontWidth, fontHeight);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Char<TPixel>(this IOnlySetterRenderer<TPixel> self, int x, int y, char @char, BitmapFont<TPixel> font)
    {
        Point charCoord = CharacterCoord(@char, font.Width, font.Height, font.CharWidth, font.CharHeight);

        for (int offsetY = 0; offsetY < font.CharHeight; offsetY++)
        {
            if (y + offsetY < 0) continue;
            if (y + offsetY >= self.Height) break;

            int i = charCoord.X + ((charCoord.Y + offsetY) * font.Height);
            self.Put(x, y + offsetY, font.Buffer.Slice(i, font.CharWidth), font.CharWidth, 1);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Char<TPixel>(this IOnlySetterRenderer<TPixel> self, int x, int y, byte @char, BitmapFont<TPixel> font)
    {
        Point charCoord = CharacterCoord(@char, font.Width, font.Height, font.CharWidth, font.CharHeight);

        for (int offsetY = 0; offsetY < font.CharHeight; offsetY++)
        {
            if (y + offsetY < 0) continue;
            if (y + offsetY >= self.Height) break;

            int i = charCoord.X + ((charCoord.Y + offsetY) * font.Height);
            self.Put(x, y + offsetY, font.Buffer.Slice(i, font.CharWidth), font.CharWidth, 1);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Text<TPixel>(this IOnlySetterRenderer<TPixel> self, int x, int y, ReadOnlySpan<char> text, BitmapFont<TPixel> font)
    {
        if (text.IsEmpty) return;
        if (y < 0 || y >= self.Height) return;

        for (int i = 0; i < text.Length; i++)
        {
            self.Char(x + (i * font.CharWidth), y, text[i], font);
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Text<TPixel>(this IOnlySetterRenderer<TPixel> self, int x, int y, ReadOnlySpan<byte> text, BitmapFont<TPixel> font)
    {
        if (text.IsEmpty) return;
        if (y < 0 || y >= self.Height) return;

        for (int i = 0; i < text.Length; i++)
        {
            self.Char(x + (i * font.CharWidth), y, text[i], font);
        }
    }

    #endregion

    #region InputField()

    const string ShiftedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ§'\"+!%/=()?:_-+ ";
    const string Chars = "abcdefghijklmnopqrstuvwxyz0123456789,.-+ ";
    const string Keys = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789,.-+ ";

    #endregion

    #region CorneredLine()

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void CorneredLine<TPixel>(this IOnlySetterRenderer<TPixel> self, Point a, Point b, TPixel v)
    {
        Point min = Point.Min(a, b);
        Point max = Point.Max(a, b);

        if (min.X >= 0 && min.X < self.Width)
        {
            for (int y = Math.Max((short)0, min.Y); y <= max.Y; y++)
            {
                if (y >= self.Height) break;
                self.Set(min.X, y, v);
            }
        }

        if (max.Y >= 0 && max.Y < self.Height)
        {
            for (int x = Math.Max((short)0, min.X); x <= max.X; x++)
            {
                if (x >= self.Width) break;
                self.Set(x, max.Y, v);
            }
        }
    }

    #endregion

    #region Bitfield

    public static void Bitfield(this IOnlySetterRenderer<char> renderer, Point position, ReadOnlySpan<int> bitfield)
        => renderer.Bitfield(position, bitfield, '1', '0');

    public static void Bitfield(this IOnlySetterRenderer<char> renderer, Point position, int bitfield)
        => renderer.Bitfield(position, bitfield, '1', '0');

    public static void Bitfield<TPixel>(this IOnlySetterRenderer<TPixel> renderer, Point position, ReadOnlySpan<int> bitfield, TPixel one, TPixel zero)
    {
        for (int i = 0; i < bitfield.Length; i++)
        {
            Bitfield(renderer, new Point(position.X + (i * 32), position.Y), bitfield[i], one, zero);
        }
    }

    public static void Bitfield<TPixel>(this IOnlySetterRenderer<TPixel> renderer, Point position, int bitfield, TPixel one, TPixel zero)
    {
        for (int i = 0; i < 32; i++)
        {
            renderer.Set((position.Y * renderer.Width) + position.X + i, (bitfield & (1 << i)) != 0 ? one : zero);
        }
    }

    public static void Bitfield(this IOnlySetterRenderer<char> renderer, Point position, ReadOnlySpan<uint> bitfield)
        => renderer.Bitfield(position, bitfield, '1', '0');

    public static void Bitfield(this IOnlySetterRenderer<char> renderer, Point position, uint bitfield)
        => renderer.Bitfield(position, bitfield, '1', '0');

    public static void Bitfield<TPixel>(this IOnlySetterRenderer<TPixel> renderer, Point position, ReadOnlySpan<uint> bitfield, TPixel one, TPixel zero)
    {
        for (int i = 0; i < bitfield.Length; i++)
        {
            Bitfield(renderer, new Point(position.X + (i * 32), position.Y), bitfield[i], one, zero);
        }
    }

    public static void Bitfield<TPixel>(this IOnlySetterRenderer<TPixel> renderer, Point position, uint bitfield, TPixel one, TPixel zero)
    {
        for (int i = 0; i < 32; i++)
        {
            renderer.Set((position.Y * renderer.Width) + position.X + i, (bitfield & (1 << i)) != 0 ? one : zero);
        }
    }

    #endregion

    #region Triangle()

    public static void Triangle<TPixel>(
        this IOnlySetterRenderer<TPixel> renderer,
        Point a,
        Point b,
        Point c,
        TPixel color)
        => renderer.Triangle(
            a.X, a.Y,
            b.X, b.Y,
            c.X, c.Y,
            color);

    public static void Triangle<TPixel>(
        this IOnlySetterRenderer<TPixel> renderer,
        Span<float> depth,
        Point a, float depthA,
        Point b, float depthB,
        Point c, float depthC,
        TPixel color)
        => renderer.Triangle<TPixel>(
            depth,
            a.X, a.Y, depthA,
            b.X, b.Y, depthB,
            c.X, c.Y, depthC,
            color);

    public static void Triangle<TPixel>(
        this IOnlySetterRenderer<TPixel> renderer,
        int x0, int y0,
        int x1, int y1,
        int x2, int y2,
        TPixel color)
    {
        int width = renderer.Width;
        int height = renderer.Height;

        // sort the points vertically
        if (y1 > y2)
        {
            Swap(ref x1, ref x2);
            Swap(ref y1, ref y2);
        }
        if (y0 > y1)
        {
            Swap(ref x0, ref x1);
            Swap(ref y0, ref y1);
        }
        if (y1 > y2)
        {
            Swap(ref x1, ref x2);
            Swap(ref y1, ref y2);
        }

        float dx_far = (float)(x2 - x0) / (y2 - y0 + 1);
        float dx_upper = (float)(x1 - x0) / (y1 - y0 + 1);
        float dx_low = (float)(x2 - x1) / (y2 - y1 + 1);
        float xf = x0;
        float xt = x0 + dx_upper; // if y0 == y1, special case
        for (int y = y0; y <= (y2 > height - 1 ? height - 1 : y2); y++)
        {
            if (y >= 0)
            {
                for (int x = xf > 0 ? Convert.ToInt32(xf) : 0;
                     x <= (xt < width ? xt : width - 1); x++)
                {
                    if (!renderer.IsVisible(x, y)) continue;
                    renderer.Set(x + (y * width), color);
                }
                for (int x = xf < width ? Convert.ToInt32(xf) : width - 1;
                     x >= (xt > 0 ? xt : 0); x--)
                {
                    if (!renderer.IsVisible(x, y)) continue;
                    renderer.Set(x + (y * width), color);
                }
            }
            xf += dx_far;
            if (y < y1)
            { xt += dx_upper; }
            else
            { xt += dx_low; }
        }
    }

    public static void Triangle<TPixel>(
        this IOnlySetterRenderer<TPixel> renderer,
        Span<float> depth,
        int x1, int y1, float w1,
        int x2, int y2, float w2,
        int x3, int y3, float w3,
        TPixel color)
    {
        // sort the points vertically
        if (y2 < y1)
        {
            Swap(ref x1, ref x2);
            Swap(ref y1, ref y2);
            Swap(ref w1, ref w2);
        }

        if (y3 < y1)
        {
            Swap(ref x1, ref x3);
            Swap(ref y1, ref y3);
            Swap(ref w1, ref w3);
        }

        if (y2 > y3)
        {
            Swap(ref x2, ref x3);
            Swap(ref y2, ref y3);
            Swap(ref w2, ref w3);
        }

        int dy1 = y2 - y1;
        int dx1 = x2 - x1;
        float dw1 = w2 - w1;

        int dy2 = y3 - y1;
        int dx2 = x3 - x1;
        float dw2 = w3 - w1;

        float w;

        float daxStep = 0f;
        float dbxStep = 0f;
        float dw1Step = 0f;
        float dw2Step = 0f;

        if (dy1 != 0) daxStep = (float)dx1 / (float)Math.Abs(dy1);
        if (dy2 != 0) dbxStep = (float)dx2 / (float)Math.Abs(dy2);

        if (dy1 != 0) dw1Step = (float)dw1 / (float)Math.Abs(dy1);

        if (dy2 != 0) dw2Step = (float)dw2 / (float)Math.Abs(dy2);

        if (dy1 != 0)
        {
            for (int i = y1; i <= y2; i++)
            {
                int ax = x1 + (int)((i - y1) * daxStep);
                int bx = x1 + (int)((i - y1) * dbxStep);

                float sw = w1 + ((i - y1) * dw1Step);

                float ew = w1 + ((i - y1) * dw2Step);

                if (ax > bx)
                {
                    Swap(ref ax, ref bx);
                    Swap(ref sw, ref ew);
                }

                float tStep = 1f / (float)(bx - ax);
                float t = 0f;

                for (int j = ax; j < bx; j++)
                {
                    w = ((1f - t) * sw) + (t * ew);

                    if (renderer.IsVisible(j, i) &&
                        (depth.IsEmpty || w > depth[j + (i * renderer.Width)]))
                    {
                        renderer.Set(j, i, color);
                        if (!depth.IsEmpty) depth[j + (i * renderer.Width)] = w;
                    }

                    t += tStep;
                }
            }
        }

        dy1 = y3 - y2;
        dx1 = x3 - x2;
        dw1 = w3 - w2;

        if (dy1 != 0) daxStep = (float)dx1 / (float)Math.Abs(dy1);
        if (dy2 != 0) dbxStep = (float)dx2 / (float)Math.Abs(dy2);

        if (dy1 != 0) dw1Step = (float)dw1 / (float)Math.Abs(dy1);

        if (dy1 != 0)
        {
            for (int i = y2; i <= y3; i++)
            {
                int ax = x2 + (int)((i - y2) * daxStep);
                int bx = x1 + (int)((i - y1) * dbxStep);

                float sw = w2 + ((i - y2) * dw1Step);

                float ew = w1 + ((i - y1) * dw2Step);

                if (ax > bx)
                {
                    Swap(ref ax, ref bx);
                    Swap(ref sw, ref ew);
                }

                float tStep = 1f / (float)(bx - ax);
                float t = 0f;

                for (int j = ax; j < bx; j++)
                {
                    w = ((1f - t) * sw) + (t * ew);

                    if (renderer.IsVisible(j, i)
                        && (depth.IsEmpty || w > depth[j + (i * renderer.Width)]))
                    {
                        renderer.Set(j, i, color);
                        if (!depth.IsEmpty) depth[j + (i * renderer.Width)] = w;
                    }

                    t += tStep;
                }
            }
        }
    }

    public static void FillTriangle<TPixel>(
        this IOnlySetterRenderer<TPixel> renderer,
        Span<float> depth,
        Point a, Vector3 texA,
        Point b, Vector3 texB,
        Point c, Vector3 texC,
        ReadOnlySpan2D<TPixel> image)
        => renderer.FillTriangle<TPixel>(
            depth,
            a.X, a.Y, texA.X, texA.Y, texA.Z,
            b.X, b.Y, texB.X, texB.Y, texB.Z,
            c.X, c.Y, texC.X, texC.Y, texC.Z,
            image);

    public static void FillTriangle<TPixel>(
        this IOnlySetterRenderer<TPixel> renderer,
        Span<float> depth,
        int x1, int y1, float u1, float v1, float w1,
        int x2, int y2, float u2, float v2, float w2,
        int x3, int y3, float u3, float v3, float w3,
        ReadOnlySpan2D<TPixel> image)
    {
        // sort the points vertically
        if (y2 < y1)
        {
            Swap(ref x1, ref x2);
            Swap(ref y1, ref y2);
            Swap(ref u1, ref u2);
            Swap(ref v1, ref v2);
            Swap(ref w1, ref w2);
        }

        if (y3 < y1)
        {
            Swap(ref x1, ref x3);
            Swap(ref y1, ref y3);
            Swap(ref u1, ref u3);
            Swap(ref v1, ref v3);
            Swap(ref w1, ref w3);
        }

        if (y2 > y3)
        {
            Swap(ref x2, ref x3);
            Swap(ref y2, ref y3);
            Swap(ref u2, ref u3);
            Swap(ref v2, ref v3);
            Swap(ref w2, ref w3);
        }

        int dy1 = y2 - y1;
        int dx1 = x2 - x1;
        float dv1 = v2 - v1;
        float du1 = u2 - u1;
        float dw1 = w2 - w1;

        int dy2 = y3 - y1;
        int dx2 = x3 - x1;
        float dv2 = v3 - v1;
        float du2 = u3 - u1;
        float dw2 = w3 - w1;

        float texU, texV, texW;

        float daxStep = 0f;
        float dbxStep = 0f;
        float du1Step = 0f;
        float dv1Step = 0f;
        float du2Step = 0f;
        float dv2Step = 0f;
        float dw1Step = 0f;
        float dw2Step = 0f;

        if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
        if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

        if (dy1 != 0) du1Step = du1 / MathF.Abs(dy1);
        if (dy1 != 0) dv1Step = dv1 / MathF.Abs(dy1);
        if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

        if (dy2 != 0) du2Step = du2 / MathF.Abs(dy2);
        if (dy2 != 0) dv2Step = dv2 / MathF.Abs(dy2);
        if (dy2 != 0) dw2Step = dw2 / MathF.Abs(dy2);

        if (dy1 != 0)
        {
            for (int i = y1; i <= y2; i++)
            {
                int ax = x1 + (int)((i - y1) * daxStep);
                int bx = x1 + (int)((i - y1) * dbxStep);

                float texSu = u1 + ((i - y1) * du1Step);
                float texSv = v1 + ((i - y1) * dv1Step);
                float texSw = w1 + ((i - y1) * dw1Step);

                float texEu = u1 + ((i - y1) * du2Step);
                float texEv = v1 + ((i - y1) * dv2Step);
                float texEw = w1 + ((i - y1) * dw2Step);

                if (ax > bx)
                {
                    Swap(ref ax, ref bx);
                    Swap(ref texSu, ref texEu);
                    Swap(ref texSv, ref texEv);
                    Swap(ref texSw, ref texEw);
                }

                // texU = texSu;
                // texV = texSv;
                // texW = texSw;

                float tStep = 1f / (float)(bx - ax);
                float t = 0f;

                for (int j = ax; j < bx; j++)
                {
                    texU = ((1f - t) * texSu) + (t * texEu);
                    texV = ((1f - t) * texSv) + (t * texEv);
                    texW = ((1f - t) * texSw) + (t * texEw);

                    if (renderer.IsVisible(j, i)
                        && (depth.IsEmpty || texW > depth[j + (i * renderer.Width)]))
                    {
                        renderer.Set(j, i, NormalizedSample(image, texU / texW, texV / texW));
                        if (!depth.IsEmpty) depth[j + (i * renderer.Width)] = texW;
                    }

                    t += tStep;
                }
            }
        }

        dy1 = y3 - y2;
        dx1 = x3 - x2;
        dv1 = v3 - v2;
        du1 = u3 - u2;
        dw1 = w3 - w2;

        if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
        if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

        du1Step = 0f;
        dv1Step = 0f;
        if (dy1 != 0) du1Step = du1 / MathF.Abs(dy1);
        if (dy1 != 0) dv1Step = dv1 / MathF.Abs(dy1);
        if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

        if (dy1 != 0)
        {
            for (int i = y2; i <= y3; i++)
            {
                int ax = x2 + (int)((i - y2) * daxStep);
                int bx = x1 + (int)((i - y1) * dbxStep);

                float texSu = u2 + ((i - y2) * du1Step);
                float texSv = v2 + ((i - y2) * dv1Step);
                float texSw = w2 + ((i - y2) * dw1Step);

                float texEu = u1 + ((i - y1) * du2Step);
                float texEv = v1 + ((i - y1) * dv2Step);
                float texEw = w1 + ((i - y1) * dw2Step);

                if (ax > bx)
                {
                    Swap(ref ax, ref bx);
                    Swap(ref texSu, ref texEu);
                    Swap(ref texSv, ref texEv);
                    Swap(ref texSw, ref texEw);
                }

                // texU = texSu;
                // texV = texSv;
                // texW = texSw;

                float tStep = 1f / (float)(bx - ax);
                float t = 0f;

                for (int j = ax; j < bx; j++)
                {
                    texU = ((1f - t) * texSu) + (t * texEu);
                    texV = ((1f - t) * texSv) + (t * texEv);
                    texW = ((1f - t) * texSw) + (t * texEw);

                    if (renderer.IsVisible(j, i) &&
                        (depth.IsEmpty || texW > depth[j + (i * renderer.Width)]))
                    {
                        renderer.Set(j, i, NormalizedSample(image, texU / texW, texV / texW));
                        if (!depth.IsEmpty) depth[j + (i * renderer.Width)] = texW;
                    }

                    t += tStep;
                }
            }
        }
    }

    public static void FillTriangle<TPixel, TSourcePixel>(
        this IOnlySetterRenderer<TPixel> renderer,
        Span<float> depth,
        Point a, Vector3 texA,
        Point b, Vector3 texB,
        Point c, Vector3 texC,
        ReadOnlySpan2D<TSourcePixel> image, Func<TSourcePixel, TPixel> converter)
        => renderer.FillTriangle<TPixel, TSourcePixel>(
            depth,
            a.X, a.Y, texA.X, texA.Y, texA.Z,
            b.X, b.Y, texB.X, texB.Y, texB.Z,
            c.X, c.Y, texC.X, texC.Y, texC.Z,
            image, converter);

    public static void FillTriangle<TPixel, TSourcePixel>(
        this IOnlySetterRenderer<TPixel> renderer,
        Span<float> depth,
        int x1, int y1, float u1, float v1, float w1,
        int x2, int y2, float u2, float v2, float w2,
        int x3, int y3, float u3, float v3, float w3,
        ReadOnlySpan2D<TSourcePixel> image, Func<TSourcePixel, TPixel> converter)
    {
        // sort the points vertically
        if (y2 < y1)
        {
            Swap(ref x1, ref x2);
            Swap(ref y1, ref y2);
            Swap(ref u1, ref u2);
            Swap(ref v1, ref v2);
            Swap(ref w1, ref w2);
        }

        if (y3 < y1)
        {
            Swap(ref x1, ref x3);
            Swap(ref y1, ref y3);
            Swap(ref u1, ref u3);
            Swap(ref v1, ref v3);
            Swap(ref w1, ref w3);
        }

        if (y2 > y3)
        {
            Swap(ref x2, ref x3);
            Swap(ref y2, ref y3);
            Swap(ref u2, ref u3);
            Swap(ref v2, ref v3);
            Swap(ref w2, ref w3);
        }

        int dy1 = y2 - y1;
        int dx1 = x2 - x1;
        float dv1 = v2 - v1;
        float du1 = u2 - u1;
        float dw1 = w2 - w1;

        int dy2 = y3 - y1;
        int dx2 = x3 - x1;
        float dv2 = v3 - v1;
        float du2 = u3 - u1;
        float dw2 = w3 - w1;

        float texU, texV, texW;

        float daxStep = 0f;
        float dbxStep = 0f;
        float du1Step = 0f;
        float dv1Step = 0f;
        float du2Step = 0f;
        float dv2Step = 0f;
        float dw1Step = 0f;
        float dw2Step = 0f;

        if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
        if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

        if (dy1 != 0) du1Step = du1 / MathF.Abs(dy1);
        if (dy1 != 0) dv1Step = dv1 / MathF.Abs(dy1);
        if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

        if (dy2 != 0) du2Step = du2 / MathF.Abs(dy2);
        if (dy2 != 0) dv2Step = dv2 / MathF.Abs(dy2);
        if (dy2 != 0) dw2Step = dw2 / MathF.Abs(dy2);

        if (dy1 != 0)
        {
            for (int i = y1; i <= y2; i++)
            {
                int ax = x1 + (int)((i - y1) * daxStep);
                int bx = x1 + (int)((i - y1) * dbxStep);

                float texSu = u1 + ((i - y1) * du1Step);
                float texSv = v1 + ((i - y1) * dv1Step);
                float texSw = w1 + ((i - y1) * dw1Step);

                float texEu = u1 + ((i - y1) * du2Step);
                float texEv = v1 + ((i - y1) * dv2Step);
                float texEw = w1 + ((i - y1) * dw2Step);

                if (ax > bx)
                {
                    Swap(ref ax, ref bx);
                    Swap(ref texSu, ref texEu);
                    Swap(ref texSv, ref texEv);
                    Swap(ref texSw, ref texEw);
                }

                // texU = texSu;
                // texV = texSv;
                // texW = texSw;

                float tStep = 1f / (float)(bx - ax);
                float t = 0f;

                for (int j = ax; j < bx; j++)
                {
                    texU = ((1f - t) * texSu) + (t * texEu);
                    texV = ((1f - t) * texSv) + (t * texEv);
                    texW = ((1f - t) * texSw) + (t * texEw);

                    if (renderer.IsVisible(j, i)
                        && (depth.IsEmpty || texW > depth[j + (i * renderer.Width)]))
                    {
                        renderer.Set(j, i, converter.Invoke(NormalizedSample(image, texU / texW, texV / texW)));
                        if (!depth.IsEmpty) depth[j + (i * renderer.Width)] = texW;
                    }

                    t += tStep;
                }
            }
        }

        dy1 = y3 - y2;
        dx1 = x3 - x2;
        dv1 = v3 - v2;
        du1 = u3 - u2;
        dw1 = w3 - w2;

        if (dy1 != 0) daxStep = dx1 / MathF.Abs(dy1);
        if (dy2 != 0) dbxStep = dx2 / MathF.Abs(dy2);

        du1Step = 0f;
        dv1Step = 0f;
        if (dy1 != 0) du1Step = du1 / MathF.Abs(dy1);
        if (dy1 != 0) dv1Step = dv1 / MathF.Abs(dy1);
        if (dy1 != 0) dw1Step = dw1 / MathF.Abs(dy1);

        if (dy1 != 0)
        {
            for (int i = y2; i <= y3; i++)
            {
                int ax = x2 + (int)((i - y2) * daxStep);
                int bx = x1 + (int)((i - y1) * dbxStep);

                float texSu = u2 + ((i - y2) * du1Step);
                float texSv = v2 + ((i - y2) * dv1Step);
                float texSw = w2 + ((i - y2) * dw1Step);

                float texEu = u1 + ((i - y1) * du2Step);
                float texEv = v1 + ((i - y1) * dv2Step);
                float texEw = w1 + ((i - y1) * dw2Step);

                if (ax > bx)
                {
                    Swap(ref ax, ref bx);
                    Swap(ref texSu, ref texEu);
                    Swap(ref texSv, ref texEv);
                    Swap(ref texSw, ref texEw);
                }

                // texU = texSu;
                // texV = texSv;
                // texW = texSw;

                float tStep = 1f / (float)(bx - ax);
                float t = 0f;

                for (int j = ax; j < bx; j++)
                {
                    texU = ((1f - t) * texSu) + (t * texEu);
                    texV = ((1f - t) * texSv) + (t * texEv);
                    texW = ((1f - t) * texSw) + (t * texEw);

                    if (renderer.IsVisible(j, i) &&
                        (depth.IsEmpty || texW > depth[j + (i * renderer.Width)]))
                    {
                        renderer.Set(j, i, converter.Invoke(NormalizedSample(image, texU / texW, texV / texW)));
                        if (!depth.IsEmpty) depth[j + (i * renderer.Width)] = texW;
                    }

                    t += tStep;
                }
            }
        }
    }

    static TPixel NormalizedSample<TPixel>(ReadOnlySpan2D<TPixel> image, float texU, float texV)
    {
        int x = (int)(texU * image.Width);
        int y = (int)(texV * image.Height);

        x = Math.Clamp(x, 0, image.Width - 1);
        y = Math.Clamp(y, 0, image.Height - 1);

        return image.Span[x + (y * image.Width)];
    }

    static void Swap<T>(ref T a, ref T b)
    {
        T temp = b;
        b = a;
        a = temp;
    }

    #endregion

    #region Line()

    public static void Lines<T>(
        this IOnlySetterRenderer<T> renderer,
        ReadOnlySpan<Point> points,
        T c,
        bool connectEnd = false)
    {
        for (int i = 1; i < points.Length; i++)
        { Line(renderer, points[i - 1], points[i], c); }

        if (connectEnd && points.Length > 2)
        { Line(renderer, points[0], points[^1], c); }
    }

    /*
    public void DrawLine(VectorInt a, VectorInt b, Color color)
        => DrawLine(a, b, (byte)color, ' ');
    public void DrawLine(VectorInt a, VectorInt b, ushort attributes, char c)
    {
        int dist = (int)MathF.Sqrt((a - b).SqrMagnitude);

        for (int i = 0; i < dist; i++)
        {
            float v = (float)i / (float)dist;
            Vector p = (a * v) + (b * (1f - v));
            VectorInt p2 = p.Round();
            if (!IsVisible(p2)) continue;
            this[p2].Attributes = attributes;
            this[p2].Char = c;
        }
    }
    */

    /// <summary>
    /// Source: <see href="https://stackoverflow.com/a/32252934">StackOverflow</see>
    /// </summary>
    public static void Line<TPixel>(
        this IOnlySetterRenderer<TPixel> renderer,
        Point a,
        Point b,
        TPixel color)
    {
        int dx = b.X - a.X;
        int dy = b.Y - a.Y;

        int sx = Math.Sign(dx);
        int sy = Math.Sign(dy);

        dx = Math.Abs(dx);
        dy = Math.Abs(dy);
        int d = Math.Max(dx, dy);

        float r = d / 2f;

        int x = a.X;
        int y = a.Y;

        if (dx > dy)
        {
            for (int i = 0; i < d; i++)
            {
                if (x < 0 || x >= renderer.Width || y < 0 || y >= renderer.Height) continue;
                renderer.Set(x, y, color);

                x += sx;
                r += dy;

                if (r >= dx)
                {
                    y += sy;
                    r -= dx;
                }
            }
        }
        else
        {
            for (int i = 0; i < d; i++)
            {
                if (x < 0 || x >= renderer.Width || y < 0 || y >= renderer.Height) continue;
                renderer.Set(x, y, color);

                y += sy;
                r += dx;

                if (r >= dy)
                {
                    x += sx;
                    r -= dy;
                }
            }
        }
    }

    #endregion

    /// <exception cref="ArgumentOutOfRangeException"/>
    public static void Set<TPixel>(this IOnlySetterRenderer<TPixel> renderer, int x, int y, TPixel pixel) => renderer.Set((y * renderer.Width) + x, pixel);

    /// <exception cref="ArgumentOutOfRangeException"/>
    public static void Set<TPixel>(this IOnlySetterRenderer<TPixel> renderer, float x, float y, TPixel pixel) => renderer.Set(((int)MathF.Round(y) * renderer.Width) + (int)MathF.Round(x), pixel);

    /// <exception cref="ArgumentOutOfRangeException"/>
    public static void Set<TPixel>(this IOnlySetterRenderer<TPixel> renderer, Point p, TPixel pixel) => renderer.Set((p.Y * renderer.Width) + p.X, pixel);

    /// <exception cref="ArgumentOutOfRangeException"/>
    public static void Set<TPixel>(this IOnlySetterRenderer<TPixel> renderer, Vector2 p, TPixel pixel) => renderer.Set(((int)MathF.Round(p.Y) * renderer.Width) + (int)MathF.Round(p.X), pixel);

    public static bool IsVisible(this IRenderer renderer, int x, int y) => x >= 0 && y >= 0 && x < renderer.Width && y < renderer.Height;
    public static bool IsVisible(this IRenderer renderer, float x, float y) => renderer.IsVisible((int)MathF.Round(x), (int)MathF.Round(y));
    public static bool IsVisible(this IRenderer renderer, Point position) => renderer.IsVisible(position.X, position.Y);
    public static bool IsVisible(this IRenderer renderer, Vector2 position) => renderer.IsVisible((int)MathF.Round(position.X), (int)MathF.Round(position.Y));

    public static void Fill<TPixel>(this IOnlySetterRenderer<TPixel> renderer, TPixel value)
    {
        for (int y = 0; y < renderer.Height; y++)
        {
            for (int x = 0; x < renderer.Width; x++)
            {
                renderer.Set(x, y, value);
            }
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Fill<TPixel>(this IOnlySetterRenderer<TPixel> renderer, Rect rect, TPixel value)
    {
        for (int offsetY = 0; offsetY < rect.Height; offsetY++)
        {
            int y = rect.Y + offsetY;
            if (y >= renderer.Height) break;
            if (y < 0) continue;

            for (int offsetX = 0; offsetX < rect.Width; offsetX++)
            {
                int x = rect.X + offsetX;
                if (x < 0) continue;
                if (x >= renderer.Width) break;

                renderer.Set(x, y, value);
            }
        }
    }

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Put<TPixel>(this IOnlySetterRenderer<TPixel> renderer, int x, int y, ReadOnlySpan2D<TPixel> data)
        => renderer.Put(x, y, data.Span, data.Width, data.Height);

    /// <remarks>
    /// <b>Note:</b> This checks if the coordinate is out of range
    /// </remarks>
    public static void Put<TPixel>(this IOnlySetterRenderer<TPixel> renderer, int x, int y, ReadOnlySpan<TPixel> data, int dataWidth, int dataHeight)
    {
        for (int offsetY = 0; offsetY < dataHeight; offsetY++)
        {
            if (y + offsetY < 0) continue;
            if (y + offsetY >= renderer.Height) break;

            if (y < 0) continue;
            if (y >= dataHeight) break;

            for (int offsetX = 0; offsetX < dataWidth; offsetX++)
            {
                if (x + offsetX < 0) continue;
                if (x + offsetX >= renderer.Width) break;

                if (x < 0) continue;
                if (x >= dataHeight) break;

                renderer.Set(x + offsetX, y + offsetY, data[x + (y * dataWidth)]);
            }
        }
    }

    public static void Fill<TPixel>(this IOnlySetterRenderer<TPixel> renderer, Func<int, int, TPixel> mapper)
    {
        for (int y = 0; y < renderer.Height; y++)
        {
            for (int x = 0; x < renderer.Width; x++)
            {
                renderer.Set(x, y, mapper.Invoke(x, y));
            }
        }
    }

    public static void FillNormalized<TPixel>(this IOnlySetterRenderer<TPixel> renderer, Func<float, float, TPixel> mapper)
    {
        for (int y = 0; y < renderer.Height; y++)
        {
            for (int x = 0; x < renderer.Width; x++)
            {
                renderer.Set(x, y, mapper.Invoke((float)x / (float)renderer.Width, (float)y / (float)renderer.Height));
            }
        }
    }
}
