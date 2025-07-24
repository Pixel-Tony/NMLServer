using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class UnitToken(int start, UnitType value) : Token(start, (byte)value >> 4)
{
    public override string SemanticType => SemanticTokenTypes.Keyword;
}