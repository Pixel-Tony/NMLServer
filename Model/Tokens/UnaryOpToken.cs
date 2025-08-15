using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Tokens;

internal sealed class UnaryOpToken(int start, char sign) : BaseToken(start, 1)
{
    public override string SemanticType => SemanticTokenTypes.Operator;
}