using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

// TODO
#pragma warning disable CS9113 // Parameter is unread.

internal sealed class UnaryOpToken(int start, char sign) : Token(start, 1)
#pragma warning restore CS9113 // Parameter is unread.

{
    public override string SemanticType => SemanticTokenTypes.Operator;
}