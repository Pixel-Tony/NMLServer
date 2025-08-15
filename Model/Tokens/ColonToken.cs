using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Tokens;

internal sealed class ColonToken(int start) : BaseToken(start, 1)
{
    public override string SemanticType => SemanticTokenTypes.Operator;
}