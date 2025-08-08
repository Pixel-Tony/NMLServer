using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Tokens;

internal sealed class StringToken(int start, int end) : BaseValueToken(start, end)
{
    public override string SemanticType => SemanticTokenTypes.String;
}