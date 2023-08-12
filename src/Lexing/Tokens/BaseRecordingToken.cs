using System.Text;

namespace NMLServer.Lexing.Tokens;

internal abstract class BaseRecordingToken : Token
{
    public string value => _builder.ToString();

    private readonly StringBuilder _builder;

    protected BaseRecordingToken(string s) : this(new StringBuilder()) => _builder.Append(s);

    protected BaseRecordingToken(char c) : this(new StringBuilder()) => _builder.Append(c);

    protected BaseRecordingToken(BaseRecordingToken other) : this(other._builder)
    { }

    protected BaseRecordingToken() : this(new StringBuilder())
    { }

    private BaseRecordingToken(StringBuilder builder) => _builder = builder;

    public void Add(char prefix) => _builder.Append(prefix);
}