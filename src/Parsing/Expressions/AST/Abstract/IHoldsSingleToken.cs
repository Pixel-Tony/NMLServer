using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal interface IHoldsSingleToken
{
    public BaseRecordingToken token { get; }
}