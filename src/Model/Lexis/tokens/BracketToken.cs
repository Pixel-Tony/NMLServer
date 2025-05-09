using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class BracketToken(int start, char bracket) : Token(start, 1)
{
    public readonly char Bracket = bracket;

    internal override string semanticType => SemanticTokenTypes.Operator;
}