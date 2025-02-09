using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO;
using System.Text;

namespace CLI;

public readonly struct ConsoleImage
{
    public readonly int Width;
    public readonly int Height;

    readonly AnsiChar[] Data;

    public AnsiChar this[int x, int y] => Data[x + (y * Width)];

    public ConsoleImage(AnsiChar[] value, int width, int height)
    {
        Data = value;
        Width = width;
        Height = height;
    }

    public ConsoleImage(AnsiChar[] value, int width)
    {
        Data = value;
        Width = width;
        Height = value.Length / width;
    }

    ConsoleImage(BinaryReader reader)
    {
        Width = reader.ReadInt16();
        Height = reader.ReadInt16();
        int l = Width * Height;
        Data = new AnsiChar[l];
        for (int i = 0; i < l; i++)
        {
            Data[i] = new AnsiChar(reader.ReadChar(), (AnsiColor)reader.ReadByte(), (AnsiColor)reader.ReadByte());
        }
    }

    void Serialize(BinaryWriter writer)
    {
        writer.Write(Width);
        writer.Write(Height);
        int l = Width * Height;
        for (int i = 0; i < l; i++)
        {
            writer.Write(Data[i].Char);
            writer.Write((byte)Data[i].Foreground);
            writer.Write((byte)Data[i].Background);
        }
    }

    public static ConsoleImage FromBytes(BinaryReader reader) => new(reader);
    public static ConsoleImage FromBytes(byte[] data)
    {
        using MemoryStream memoryStream = new(data, false);
        using BinaryReader reader = new(memoryStream);
        return new ConsoleImage(reader);
    }
    public static ConsoleImage FromBytes(ReadOnlySpan<byte> data)
    {
        using MemoryStream memoryStream = new(data.ToArray(), false);
        using BinaryReader reader = new(memoryStream);
        return new ConsoleImage(reader);
    }
    public static ConsoleImage FromBase64(string text)
    {
        byte[] data = Convert.FromBase64String(text);
        return FromBytes(data);
    }
    public static ConsoleImage FromBase64(ReadOnlySpan<byte> utf8)
    {
        int length = Base64.GetMaxDecodedFromUtf8Length(utf8.Length);
        Span<byte> buffer = new byte[length];
        OperationStatus status = Base64.DecodeFromUtf8(utf8, buffer, out _, out int bytesWritten);
        return status switch
        {
            OperationStatus.Done => FromBytes(buffer[..bytesWritten]),
            OperationStatus.InvalidData => throw new FormatException("Input Base64 string is not formatted correctly"),
            _ => throw new NotImplementedException(),
        };
    }

    public void ToBytes(BinaryWriter writer) => Serialize(writer);
    public byte[] ToBytes()
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);
        Serialize(writer);
        return memoryStream.ToArray();
    }
    public string ToBase64()
    {
        byte[] data = ToBytes();
        return Convert.ToBase64String(data);
    }
    public ReadOnlySpan<byte> ToBase64Utf8()
    {
        byte[] data = ToBytes();
        int length = Base64.GetMaxEncodedToUtf8Length(data.Length);
        Span<byte> buffer = new byte[length];
        OperationStatus status = Base64.EncodeToUtf8(data, buffer, out _, out int bytesWritten);
        return status switch
        {
            OperationStatus.Done => buffer[..bytesWritten],
            OperationStatus.InvalidData => throw new FormatException("Input Base64 string is not formatted correctly"),
            _ => throw new NotImplementedException(),
        };
    }

    public string ToCSharp()
    {
        ReadOnlySpan<byte> data = ToBytes();
        StringBuilder builder = new(data.Length * 4);
        builder.Append($"public static readonly {nameof(ConsoleImage)} ImageData = {nameof(ConsoleImage)}.{nameof(FromBytes)}(\r\n    \"");
        int rowWidth = 0;
        for (int i = 0; i < data.Length; i++)
        {
            // if (rowWidth >= 32)
            // {
            //     builder.Append("\"u8");
            //     if (rowWidth < 38)
            //     { builder.Append(' ', 38 - rowWidth); }
            //     builder.Append("+\r\n    \"");
            //     rowWidth = 0;
            // }

            byte c = data[i];
            if (char.IsAsciiLetterOrDigit((char)c))
            {
                builder.Append((char)c);
                rowWidth++;
            }
            else
            {
                switch (c)
                {
                    case (byte)'\0':
                        builder.Append(@"\0");
                        rowWidth += 2;
                        break;
                    case (byte)'\n':
                        builder.Append(@"\n");
                        rowWidth += 2;
                        break;
                    case (byte)'\r':
                        builder.Append(@"\r");
                        rowWidth += 2;
                        break;
                    case (byte)'\v':
                        builder.Append(@"\v");
                        rowWidth += 2;
                        break;
                    case (byte)'\t':
                        builder.Append(@"\t");
                        rowWidth += 2;
                        break;
                    default:
                        builder.Append($"\\x0{Convert.ToString(c, 16).PadLeft(2, '0')}");
                        rowWidth += 5;
                        break;
                }
            }
        }
        builder.Append("\"u8);\r\n");

        /*
        builder.Append("public static readonly byte[] ImageData = new byte[] {\r\n");
        for (int i = 0; i < data.Length; i++)
        {
            if (i > 0 && i % 16 == 0)
            {
                builder.Append("\r\n");
            }

            builder.Append($"0x{Convert.ToString(data[i], 16)}, ");
        }
        builder.Append("}\r\n");
        */

        return builder.ToString();
    }

    public ReadOnlySpan<AnsiChar> AsSpan() => new(Data);

    public ConsoleImage Scale(float widthMultiplier, float heightMultiplier)
    {
        int newWidth = (int)(Width * widthMultiplier);
        int newHeight = (int)(Height * heightMultiplier);
        AnsiChar[] newData = new AnsiChar[newWidth * newHeight];

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                newData[x + (y * newWidth)] = Data[(int)(x / widthMultiplier) + ((int)(y / heightMultiplier) * Width)];
            }
        }

        return new ConsoleImage(newData, newWidth, newHeight);
    }
}
