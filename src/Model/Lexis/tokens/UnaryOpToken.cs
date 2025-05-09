using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class UnaryOpToken(int start, char sign) : Token(start, 1)
{
    internal override string semanticType => SemanticTokenTypes.Operator;
}