using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;

namespace NMLServer.Model.Lexis;

internal sealed class BinaryOpToken(int start, int end, OperatorType type) : BaseMulticharToken(start, end)
{
    public readonly byte Precedence = Grammar.OperatorPrecedences[type];

    public readonly OperatorType Type = type;

    internal override string SemanticType => SemanticTokenTypes.Operator;
}