using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class RangeToken(int start) : Token(start, 2)
{
    internal override string semanticType => SemanticTokenTypes.Operator;
}