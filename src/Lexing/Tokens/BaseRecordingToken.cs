namespace NMLServer.Lexing.Tokens;

internal abstract class BaseRecordingToken : Token
{
    public string value { get; private set; }

    protected BaseRecordingToken(string s = "")
    {
        value = s;
    }

    protected BaseRecordingToken(char c)
    {
        value = c.ToString();
    }

    public void Add(char prefix) => value += prefix;
}