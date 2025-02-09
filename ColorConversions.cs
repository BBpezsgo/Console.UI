using System;
using System.Diagnostics;

namespace CLI;

public static class ColorConversions
{
    public static AnsiColor ToAnsi(this RGB color)
    {
        return (AnsiColor)(16 + 36 * (color.R * 5 / 255) + 6 * (color.G * 5 / 255) + (color.B * 5 / 255));
    }

    public static AnsiColor ToAnsi(this ConsoleColor color) => color switch
    {
        ConsoleColor.Black => AnsiColor.Black,
        ConsoleColor.DarkBlue => AnsiColor.Blue,
        ConsoleColor.DarkGreen => AnsiColor.Green,
        ConsoleColor.DarkCyan => AnsiColor.Cyan,
        ConsoleColor.DarkRed => AnsiColor.Red,
        ConsoleColor.DarkMagenta => AnsiColor.Magenta,
        ConsoleColor.DarkYellow => AnsiColor.Yellow,
        ConsoleColor.Gray => AnsiColor.White,
        ConsoleColor.DarkGray => AnsiColor.BrightBlack,
        ConsoleColor.Blue => AnsiColor.BrightBlue,
        ConsoleColor.Green => AnsiColor.BrightGreen,
        ConsoleColor.Cyan => AnsiColor.BrightCyan,
        ConsoleColor.Red => AnsiColor.BrightRed,
        ConsoleColor.Magenta => AnsiColor.BrightMagenta,
        ConsoleColor.Yellow => AnsiColor.BrightYellow,
        ConsoleColor.White => AnsiColor.BrightWhite,
        _ => throw new UnreachableException(),
    };

    public static RGB ToRGB(this AnsiColor color)
    {
        switch (color)
        {
            case AnsiColor.Black: return (0, 0, 0);
            case AnsiColor.Red: return (170, 0, 0);
            case AnsiColor.Green: return (0, 170, 0);
            case AnsiColor.Yellow: return (170, 85, 0);
            case AnsiColor.Blue: return (0, 0, 170);
            case AnsiColor.Magenta: return (170, 0, 170);
            case AnsiColor.Cyan: return (0, 170, 170);
            case AnsiColor.White: return (170, 170, 170);
            case AnsiColor.BrightBlack: return (85, 85, 85);
            case AnsiColor.BrightRed: return (255, 85, 85);
            case AnsiColor.BrightGreen: return (85, 255, 85);
            case AnsiColor.BrightYellow: return (255, 255, 85);
            case AnsiColor.BrightBlue: return (85, 85, 255);
            case AnsiColor.BrightMagenta: return (255, 85, 255);
            case AnsiColor.BrightCyan: return (85, 255, 255);
            case AnsiColor.BrightWhite: return (255, 255, 255);
            default:
                if ((int)color >= 16 && (int)color <= 231)
                {
                    color -= 16;
                    int index_R = (int)color / 36;
                    int r = (index_R > 0) ? (55 + (index_R * 40)) : 0;
                    int index_G = (int)color % 36 / 6;
                    int g = (index_G > 0) ? (55 + (index_G * 40)) : 0;
                    int index_B = (int)color % 6;
                    int b = (index_B > 0) ? (55 + (index_B * 40)) : 0;
                    return ((byte)r, (byte)g, (byte)b);
                }

                if ((int)color >= 232 && (int)color <= 255)
                {
                    byte v = (byte)(((int)color - 232) * 10 + 8);
                    return (v, v, v);
                }

                throw new UnreachableException();
        }
        ;
    }

    public static RGB ToRGB(this ConsoleColor color) => color switch
    {
        ConsoleColor.Black => (RGB)(0, 0, 0),
        ConsoleColor.DarkRed => (RGB)(170, 0, 0),
        ConsoleColor.DarkGreen => (RGB)(0, 170, 0),
        ConsoleColor.DarkYellow => (RGB)(170, 85, 0),
        ConsoleColor.DarkBlue => (RGB)(0, 0, 170),
        ConsoleColor.DarkMagenta => (RGB)(170, 0, 170),
        ConsoleColor.DarkCyan => (RGB)(0, 170, 170),
        ConsoleColor.Gray => (RGB)(170, 170, 170),
        ConsoleColor.DarkGray => (RGB)(85, 85, 85),
        ConsoleColor.Red => (RGB)(255, 85, 85),
        ConsoleColor.Green => (RGB)(85, 255, 85),
        ConsoleColor.Yellow => (RGB)(255, 255, 85),
        ConsoleColor.Blue => (RGB)(85, 85, 255),
        ConsoleColor.Magenta => (RGB)(255, 85, 255),
        ConsoleColor.Cyan => (RGB)(85, 255, 255),
        ConsoleColor.White => (RGB)(255, 255, 255),
        _ => throw new UnreachableException(),
    };

    public static ConsoleColor ToConsole(this AnsiColor color) => color switch
    {
        AnsiColor.Black => ConsoleColor.Black,
        AnsiColor.Blue => ConsoleColor.DarkBlue,
        AnsiColor.Green => ConsoleColor.DarkGreen,
        AnsiColor.Cyan => ConsoleColor.DarkCyan,
        AnsiColor.Red => ConsoleColor.DarkRed,
        AnsiColor.Magenta => ConsoleColor.DarkMagenta,
        AnsiColor.Yellow => ConsoleColor.DarkYellow,
        AnsiColor.White => ConsoleColor.Gray,
        AnsiColor.BrightBlack => ConsoleColor.DarkGray,
        AnsiColor.BrightBlue => ConsoleColor.Blue,
        AnsiColor.BrightGreen => ConsoleColor.Green,
        AnsiColor.BrightCyan => ConsoleColor.Cyan,
        AnsiColor.BrightRed => ConsoleColor.Red,
        AnsiColor.BrightMagenta => ConsoleColor.Magenta,
        AnsiColor.BrightYellow => ConsoleColor.Yellow,
        AnsiColor.BrightWhite => ConsoleColor.White,
        _ => color.ToRGB().ToConsole(),
    };

    /// <summary>
    /// Source: <see href="https://stackoverflow.com/questions/41644778/convert-24-bit-color-to-4-bit-rgbi"/>
    /// </summary>
    public static ConsoleColor ToConsole(this RGB color)
    {
        static RGB To24bitColor(byte r, byte g, byte b, byte i)
            => ((ConsoleColor)((i << 3) | (r << 2) | (g << 1) | b)).ToRGB();

        static (byte R, byte G, byte B) RgbxApprox(RGB color, byte x)
        {
            int threshold = (x + 1) * (byte.MaxValue / 3);
            byte r = color.R > threshold ? (byte)1 : (byte)0;
            byte g = color.G > threshold ? (byte)1 : (byte)0;
            byte b = color.B > threshold ? (byte)1 : (byte)0;
            return (r, g, b);
        }

        (byte r0, byte g0, byte b0) = RgbxApprox(color, 0);
        (byte r1, byte g1, byte b1) = RgbxApprox(color, 1);

        RGB color1 = To24bitColor(r0, g0, b0, 0);
        RGB color2 = To24bitColor(r1, g1, b1, 1);

        int d0 = RGB.Distance(color, color1);
        int d1 = RGB.Distance(color, color2);

        ConsoleColor result = 0b_0000;

        if (d0 <= d1)
        {
            result |= ConsoleColor.Black;
            if (r0 != 0) result |= ConsoleColor.DarkRed;
            if (g0 != 0) result |= ConsoleColor.DarkGreen;
            if (b0 != 0) result |= ConsoleColor.DarkBlue;
        }
        else
        {
            result |= ConsoleColor.DarkGray;
            if (r1 != 0) result |= ConsoleColor.DarkRed;
            if (g1 != 0) result |= ConsoleColor.DarkGreen;
            if (b1 != 0) result |= ConsoleColor.DarkBlue;
        }

        return result;
    }
}
