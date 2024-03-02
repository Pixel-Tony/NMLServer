namespace NMLServer.Lexing;

internal abstract class BaseMulticharToken(int start, int end) : Token(start)
{
    private int _start = start;
    public sealed override int end { get; protected set; } = end;

    public sealed override int start
    {
        get => _start;
        set
        {
            end += value - _start;
            _start = value;
        }
    }
}