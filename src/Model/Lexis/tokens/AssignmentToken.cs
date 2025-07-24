using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class AssignmentToken(int start) : Token(start, 1)
{
    public override string SemanticType => SemanticTokenTypes.Operator;
}