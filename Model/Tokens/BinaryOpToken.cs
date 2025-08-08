using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using NMLServer.Model.Grammar;

namespace NMLServer.Model.Tokens;

internal sealed class BinaryOpToken(int start, int end, OperatorType type) : BaseMulticharToken(start, end)
{
    public readonly OperatorType Type = type;

    public override string SemanticType => SemanticTokenTypes.Operator;
}