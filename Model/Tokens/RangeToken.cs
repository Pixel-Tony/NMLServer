using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Tokens;

internal sealed class RangeToken(int start) : BaseToken(start, 2)
{
    public override string SemanticType => SemanticTokenTypes.Operator;
}