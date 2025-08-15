using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Tokens;

internal sealed class BracketToken(int start, char bracket) : BaseToken(start, 1)
{
    public readonly char Bracket = bracket;

    public override string SemanticType => SemanticTokenTypes.Operator;
}