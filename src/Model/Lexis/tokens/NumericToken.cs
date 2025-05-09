using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class NumericToken(int start, int end) : BaseValueToken(start, end)
{
    internal override string semanticType => SemanticTokenTypes.Number;
}