using System;
using System.Numerics;
using Maths;

namespace CLI;

public static class AnsiRendererExtendedExtensions
{
    const string Barille = "⠀⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊⠋⠌⠍⠎⠏⠐⠑⠒⠓⠔⠕⠖⠗⠘⠙⠚⠛⠜⠝⠞⠟⠠⠡⠢⠣⠤⠥⠦⠧⠨⠩⠪⠫⠬⠭⠮⠯⠰⠱⠲⠳⠴⠵⠶⠷⠸⠹⠺⠻⠼⠽⠾⠿⡀⡁⡂⡃⡄⡅⡆⡇⡈⡉⡊⡋⡌⡍⡎⡏⡐⡑⡒⡓⡔⡕⡖⡗⡘⡙⡚⡛⡜⡝⡞⡟⡠⡡⡢⡣⡤⡥⡦⡧⡨⡩⡪⡫⡬⡭⡮⡯⡰⡱⡲⡳⡴⡵⡶⡷⡸⡹⡺⡻⡼⡽⡾⡿⢀⢁⢂⢃⢄⢅⢆⢇⢈⢉⢊⢋⢌⢍⢎⢏⢐⢑⢒⢓⢔⢕⢖⢗⢘⢙⢚⢛⢜⢝⢞⢟⢠⢡⢢⢣⢤⢥⢦⢧⢨⢩⢪⢫⢬⢭⢮⢯⢰⢱⢲⢳⢴⢵⢶⢷⢸⢹⢺⢻⢼⢽⢾⢿⣀⣁⣂⣃⣄⣅⣆⣇⣈⣉⣊⣋⣌⣍⣎⣏⣐⣑⣒⣓⣔⣕⣖⣗⣘⣙⣚⣛⣜⣝⣞⣟⣠⣡⣢⣣⣤⣥⣦⣧⣨⣩⣪⣫⣬⣭⣮⣯⣰⣱⣲⣳⣴⣵⣶⣷⣸⣹⣺⣻⣼⣽⣾⣿";

    #region Line

    public static void Line(this AnsiRendererExtended renderer, Vector2 a, Vector2 b, AnsiColor color)
    {
        a.Y *= 2;
        b.Y *= 2;

        float dx = b.X - a.X;
        float dy = b.Y - a.Y;

        int sx = Math.Sign(dx);
        int sy = Math.Sign(dy);

        dx = Math.Abs(dx);
        dy = Math.Abs(dy);
        float d = Math.Max(dx, dy);

        float r = d / 2f;

        float x = a.X;
        float y = a.Y;

        if (dx > dy)
        {
            for (int i = 0; i < d; i++)
            {
                if (x < 0 || x >= renderer.Width || y < 0 || y / 2 >= renderer.Height) continue;

                char c;
                if (((int)y & 1) == 0)
                {
                    c = '▀';
                }
                else
                {
                    c = '▄';
                }

                renderer[(int)x, (int)y / 2] = new AnsiChar(c, color);

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
            float _y = -1;
            float _x = -1;
            for (int i = 0; i < d; i++)
            {
                if (x < 0 || x >= renderer.Width || y < 0 || y / 2 >= renderer.Height) continue;

                char c;
                if ((int)_y == (int)y && (int)_x == (int)x)
                {
                    c = '█';
                }
                else if (((int)y & 1) == 0)
                {
                    c = '▀';
                    if (sy > 0) _y = y + sy;
                    else _y = -1;
                }
                else
                {
                    c = '▄';
                    if (sy < 0) _y = y + sy;
                    else _y = -1;
                }

                renderer[(int)x, (int)y / 2] = new AnsiChar(c, color);
                _x = x;

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

    public static void LineBarille(this AnsiRendererExtended renderer, Vector3 a, Vector3 b, AnsiColor color)
    {
        LineBarille(renderer, new Vector2(a.X, a.Y), new Vector2(a.Y, a.Y), color);
    }

    public static void LineBarille(this AnsiRendererExtended renderer, Vector2 a, Vector2 b, AnsiColor color)
    {
        a *= new Vector2(2, 4);
        b *= new Vector2(2, 4);

        float dx = b.X - a.X;
        float dy = b.Y - a.Y;

        int sx = Math.Sign(dx);
        int sy = Math.Sign(dy);

        dx = Math.Abs(dx);
        dy = Math.Abs(dy);
        float d = Math.Max(dx, dy);

        float r = d / 2f;

        float x = a.X;
        float y = a.Y;

        void Set(Vector2Int p)
        {
            char chr = (p.X % 2, p.Y % 4) switch
            {
                (0, 0) => '⠁',
                (0, 1) => '⠂',
                (0, 2) => '⠄',
                (0, 3) => '⡀',
                (1, 0) => '⠈',
                (1, 1) => '⠐',
                (1, 2) => '⠠',
                (1, 3) => '⢀',
                _ => '⠀',
            };
            p.X /= 2;
            p.Y /= 4;
            if (renderer[p].Char >= Barille[0] &&
                renderer[p].Char <= Barille[^1])
            {
                renderer[p] = new AnsiChar((char)(renderer[p].Char | chr), color);
            }
            else
            {
                renderer[p] = new AnsiChar(chr, color);
            }
        }

        if (dx > dy)
        {
            for (int i = 0; i < d; i++)
            {
                if (x < 0 || x / 2 >= renderer.Width || y < 0 || y / 4 >= renderer.Height) continue;

                Set(new Vector2Int((int)x, (int)y));

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
                if (x < 0 || x / 2 >= renderer.Width || y < 0 || y / 4 >= renderer.Height) continue;

                Set(new Vector2Int((int)x, (int)y));

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

    #region Circle

    public static void FillCircle(this AnsiRendererExtended renderer, Vector2 c, float r, AnsiColor color)
    {
        float hr = r / 2f + 1f;

        Vector2 min = new(Math.Max(0, c.X - hr), Math.Max(0, c.Y - hr));
        Vector2 max = new(Math.Min(renderer.Width - 1, c.X + hr), Math.Min(renderer.Height - 1, c.Y + hr));

        min *= 2;
        max *= 4;

        c.X *= 2;
        c.Y *= 4;

        for (float y = min.Y; y <= max.Y; y++)
        {
            for (float x = min.X; x <= max.X; x++)
            {
                float dx = x - c.X;
                float dy = y - c.Y;
                if (dx * dx + dy * dy < hr * hr)
                {
                    Vector2Int p = new((int)x, (int)y);
                    char chr = (p.X % 2, p.Y % 4) switch
                    {
                        (0, 0) => '⠁',
                        (0, 1) => '⠂',
                        (0, 2) => '⠄',
                        (0, 3) => '⡀',
                        (1, 0) => '⠈',
                        (1, 1) => '⠐',
                        (1, 2) => '⠠',
                        (1, 3) => '⢀',
                        _ => '⠀',
                    };
                    p.X /= 2;
                    p.Y /= 4;
                    if (renderer[p].Char >= Barille[0] &&
                        renderer[p].Char <= Barille[^1])
                    {
                        renderer[p].Char |= chr;
                    }
                    else
                    {
                        renderer[p] = new AnsiChar(chr, color);
                    }
                }
            }
        }
    }

    #endregion

    #region Text

    public static void Text(this AnsiRendererExtended renderer, Vector2Int p, string text, AnsiColor color)
    {
        for (int i = 0; i < text.Length; i++)
        {
            int x = p.X + i;
            if (x < 0 || x >= renderer.Width) continue;
            renderer[x, p.Y] = new AnsiChar(text[i], color);
        }
    }

    #endregion
}
