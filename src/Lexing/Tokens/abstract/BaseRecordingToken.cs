namespace NMLServer.Lexing.Tokens;

internal abstract class BaseRecordingToken : Token
{
    public string Value(string text) => text[Start..End];
    
    public readonly int End;

    protected BaseRecordingToken(int start, int end) : base(start)
    {
        End = end;
    }
}