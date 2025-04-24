namespace CLI;

public interface IChar
{
    public char Char { get; }
}

public interface IChar<TColor> : IChar
{
    public TColor Foreground { get; }
    public TColor Background { get; }
}
