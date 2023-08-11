using System.Text;

namespace NMLServer.Lexing.Tokens;

internal abstract class BaseRecordingToken : Token
{
    public string value => _builder.ToString();

    private readonly StringBuilder _builder;

    protected BaseRecordingToken(string s)
    {
        _builder = new StringBuilder();
        _builder.Append(s);
    }

    protected BaseRecordingToken(char c)
    {
        _builder = new StringBuilder();
        _builder.Append(c);
    }

    protected BaseRecordingToken()
    {
        _builder = new StringBuilder();
    }

    protected BaseRecordingToken(BaseRecordingToken another)
    {
        _builder = another._builder;
    }

    public void Add(char prefix) => _builder.Append(prefix);
}