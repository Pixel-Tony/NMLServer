using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Tokens;

internal sealed class UnitToken(int start, NML.UnitType value) : BaseToken(start, (byte)value >> 4)
{
    public override string SemanticType => SemanticTokenTypes.Keyword;
}