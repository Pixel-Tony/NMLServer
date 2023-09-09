using NMLServer.Lexing.Tokens;

namespace NMLServer.Parsing.Expression;

internal interface IHoldsSingleToken
{
    public BaseMulticharToken token { get; }
}