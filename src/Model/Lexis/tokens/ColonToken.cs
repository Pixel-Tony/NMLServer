using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class ColonToken(int start) : Token(start, 1)
{
    internal override string semanticType => SemanticTokenTypes.Operator;
}