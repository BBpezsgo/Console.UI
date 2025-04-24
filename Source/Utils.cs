using System;
using System.Numerics;
using Maths;

namespace CLI;

public static class ColorUtils
{
    public static void Threshold(Span<ColorF> buffer, ColorF threshold)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] -= threshold;
            buffer[i].R = Math.Max(0, buffer[i].R);
            buffer[i].G = Math.Max(0, buffer[i].G);
            buffer[i].B = Math.Max(0, buffer[i].B);
        }
    }

    public static void Threshold(Span<RGB> buffer, RGB threshold)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] -= threshold;
            buffer[i] = new RGB(
                Math.Max((byte)0, buffer[i].R),
                Math.Max((byte)0, buffer[i].G),
                Math.Max((byte)0, buffer[i].B)
            );
        }
    }

    public static void Threshold<T>(Span<T> buffer, T threshold)
        where T : INumber<T>
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] -= threshold;
            buffer[i] = T.Max(T.Zero, buffer[i]);
        }
    }

    public static void Blur(Span<RGB> pix, int w, int h, int radius)
    {
        if (radius < 1) return;

        int wm = w - 1;
        int hm = h - 1;
        int wh = w * h;

        int div = radius + radius + 1;

        int[] r = new int[wh];
        int[] g = new int[wh];
        int[] b = new int[wh];

        int rSum;
        int gSum;
        int bSum;

        int x; int y;

        int i;

        int yp;
        int yi;
        int yw;

        RGB p;
        RGB p1;
        RGB p2;

        int[] vMin = new int[Math.Max(w, h)];
        int[] vMax = new int[Math.Max(w, h)];

        int[] dv = new int[256 * div];
        for (i = 0; i < 256 * div; i++)
        { dv[i] = i / div; }

        yw = yi = 0;

        for (y = 0; y < h; y++)
        {
            rSum = gSum = bSum = 0;
            for (i = -radius; i <= radius; i++)
            {
                p = pix[yi + Math.Min(wm, Math.Max(i, 0))];
                rSum += p.R;
                gSum += p.G;
                bSum += p.B;
            }
            for (x = 0; x < w; x++)
            {
                r[yi] = dv[rSum];
                g[yi] = dv[gSum];
                b[yi] = dv[bSum];

                if (y == 0)
                {
                    vMin[x] = Math.Min(x + radius + 1, wm);
                    vMax[x] = Math.Max(x - radius, 0);
                }
                p1 = pix[yw + vMin[x]];
                p2 = pix[yw + vMax[x]];

                rSum += p1.R - p2.R;
                gSum += p1.G - p2.G;
                bSum += p1.B - p2.B;
                yi++;
            }
            yw += w;
        }

        for (x = 0; x < w; x++)
        {
            rSum = gSum = bSum = 0;
            yp = -radius * w;
            for (i = -radius; i <= radius; i++)
            {
                yi = Math.Max(0, yp) + x;
                rSum += r[yi];
                gSum += g[yi];
                bSum += b[yi];
                yp += w;
            }
            yi = x;
            for (y = 0; y < h; y++)
            {
                pix[yi] = new RGB(dv[rSum], dv[gSum], dv[bSum]);
                if (x == 0)
                {
                    vMin[y] = Math.Min(y + radius + 1, hm) * w;
                    vMax[y] = Math.Max(y - radius, 0) * w;
                }
                p1 = new RGB(x + vMin[y]);
                p2 = new RGB(x + vMax[y]);

                rSum += r[p1] - r[p2];
                gSum += g[p1] - g[p2];
                bSum += b[p1] - b[p2];

                yi += w;
            }
        }
    }

    public static void Blur<TColor>(Span<TColor> pix, int w, int h, int radius, Func<TColor, RGB> convTo, Func<RGB, TColor> convFrom)
    {
        if (radius < 1) return;

        int wm = w - 1;
        int hm = h - 1;
        int wh = w * h;
        int div = radius + radius + 1;
        int[] r = new int[wh];
        int[] g = new int[wh];
        int[] b = new int[wh];
        int rSum, gSum, bSum, x, y, i, yp, yi, yw;
        RGB p, p1, p2;
        int[] vMin = new int[Math.Max(w, h)];
        int[] vMax = new int[Math.Max(w, h)];

        int[] dv = new int[256 * div];
        for (i = 0; i < 256 * div; i++)
        {
            dv[i] = i / div;
        }

        yw = yi = 0;

        for (y = 0; y < h; y++)
        {
            rSum = gSum = bSum = 0;
            for (i = -radius; i <= radius; i++)
            {
                p = convTo.Invoke(pix[yi + Math.Min(wm, Math.Max(i, 0))]);
                rSum += p.R;
                gSum += p.G;
                bSum += p.B;
            }
            for (x = 0; x < w; x++)
            {
                r[yi] = dv[rSum];
                g[yi] = dv[gSum];
                b[yi] = dv[bSum];

                if (y == 0)
                {
                    vMin[x] = Math.Min(x + radius + 1, wm);
                    vMax[x] = Math.Max(x - radius, 0);
                }
                p1 = convTo.Invoke(pix[yw + vMin[x]]);
                p2 = convTo.Invoke(pix[yw + vMax[x]]);

                rSum += p1.R - p2.R;
                gSum += p1.G - p2.G;
                bSum += p1.B - p2.B;
                yi++;
            }
            yw += w;
        }

        for (x = 0; x < w; x++)
        {
            rSum = gSum = bSum = 0;
            yp = -radius * w;
            for (i = -radius; i <= radius; i++)
            {
                yi = Math.Max(0, yp) + x;
                rSum += r[yi];
                gSum += g[yi];
                bSum += b[yi];
                yp += w;
            }
            yi = x;
            for (y = 0; y < h; y++)
            {
                pix[yi] = convFrom.Invoke(new RGB(dv[rSum], dv[gSum], dv[bSum]));
                if (x == 0)
                {
                    vMin[y] = Math.Min(y + radius + 1, hm) * w;
                    vMax[y] = Math.Max(y - radius, 0) * w;
                }
                p1 = new RGB(x + vMin[y]);
                p2 = new RGB(x + vMax[y]);

                rSum += r[p1] - r[p2];
                gSum += g[p1] - g[p2];
                bSum += b[p1] - b[p2];

                yi += w;
            }
        }
    }

    public static void Add<TSelf, TOther>(this Span<TSelf> to, ReadOnlySpan<TOther> what)
        where TSelf : IAdditionOperators<TSelf, TOther, TSelf>
    {
        for (int i = 0; i < what.Length; i++)
        { to[i] += what[i]; }
    }

    public static void Bloom(Span<ColorF> buffer, int w, int h, int radius)
    {
        if (radius < 1) return;
        Span<ColorF> bloomBuffer = new ColorF[buffer.Length];
        CalculateBloom(buffer, bloomBuffer, w, h, radius);
        Add(buffer, (ReadOnlySpan<ColorF>)bloomBuffer);
    }

    public static void CalculateBloom(Span<ColorF> buffer, Span<ColorF> bloomBuffer, int w, int h, int radius)
    {
        buffer.CopyTo(bloomBuffer);
        ColorUtils.Threshold(bloomBuffer, ColorF.White);
        ColorUtils.Blur(bloomBuffer, w, h, radius, v => new RGB(v.R, v.G, v.B), v => new ColorF(v.R / 255f, v.G / 255f, v.B / 255f));
    }
}
